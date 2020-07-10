using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Serilog;
using SpikeCore.Data;
using SpikeCore.Data.Models;
using SpikeCore.MessageBus;

namespace SpikeCore.Modules
{
    public class UserManagementModule : ModuleBase
    {
        private static readonly List<string> RegularRoles = new List<string> {"regular"};
        private static readonly List<string> OpRoles = new List<string> {"regular", "op"};
        private static readonly Regex CommandRegex = new Regex(@"~\S+\s(list|show|add|remove)\s?(.*)?");

        public override string Name => "Users";
        public override string Description => "Provides user management features. Is only available to admins.";

        public override string Instructions =>
            "list | show <username> | add <username> <email> <irc host> <prefix match?> <role> | remove <username>";

        private readonly UserManager<SpikeCoreUser> _userManager;
        private readonly SpikeCoreDbContext _context;

        public UserManagementModule(UserManager<SpikeCoreUser> userManager, SpikeCoreDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        protected override bool AccessAllowed(SpikeCoreUser user)
        {
            return base.AccessAllowed(user) && user.isAdmin();
        }

        protected override async Task HandleMessageAsyncInternal(IrcPrivMessage request,
            CancellationToken cancellationToken)
        {
            var commandMatch = CommandRegex.Match(request.Text);

            if (commandMatch.Success)
            {
                var command = commandMatch.Groups[1].Value;
                var details = commandMatch.Groups[2].Value;
                var splitDetails = details.Length > 0 ? details.Split(" ") : new string[0];

                // list | show <id> | add <username> <email> <irc host> <match type> <role> | remove <username> 
                if (command.Equals("list", StringComparison.InvariantCultureIgnoreCase))
                {
                    await ListUsers(request);
                }

                // Single argument sub-methods.
                if (splitDetails.Length > 0)
                {
                    if (command.Equals("show", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await ShowUser(request, splitDetails[0]);
                    }

                    if (command.Equals("remove", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await RemoveUser(request, splitDetails[0]);
                    }
                }

                // And this one is really verbose...
                if (command.Equals("add", StringComparison.InvariantCultureIgnoreCase) && splitDetails.Length > 4)
                {
                    await CreateUser(request, splitDetails[0], splitDetails[1], splitDetails[2], splitDetails[3],
                        splitDetails[4], cancellationToken);
                }
            }
        }

        private async Task ListUsers(IrcPrivMessage request)
        {
            var users = string.Join(", ",
                _userManager.Users.Select(user => $"{user.UserName}"));
            await SendResponse(request, $"Users: [{users}]");
        }

        private async Task ShowUser(IrcPrivMessage request, string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (null == user)
            {
                await SendResponse(request, $"User {userName} not found.");
            }
            else
            {
                // Join in our roles
                user.Roles = await _userManager.GetRolesAsync(user);

                // Find all applicable user logins. The bot will only create one, but people with physical DB access can add more.
                var logins = _context.UserLogins.Where(record =>
                        record.LoginProvider == "IrcHost" && record.UserId == user.Id).Select(record =>
                        $"[{record.ProviderKey}, match type {(record.MatchType.Length > 0 ? record.MatchType : "Literal")}]")
                    .ToList();

                await SendResponse(request,
                    $"User ID {user.Id}: {user.Email}, roles [{string.Join(", ", user.Roles)}], logins [{string.Join(", ", logins)}]");
            }
        }

        private async Task CreateUser(IrcPrivMessage request, string userName, string email, string ircHostname,
            string matchType, string role, CancellationToken cancellationToken)
        {
            // If the user already exists, bail.
            if (null != await _userManager.FindByNameAsync(userName))
            {
                await SendResponse(request, $"{userName} already exists, bailing...");
            }
            else
            {
                bool.TryParse(matchType, out var prefixMatch);

                // Create our user.
                var user = new SpikeCoreUser {UserName = userName, Email = email};
                await _userManager.CreateAsync(user);

                // Associate the proper roles with the user.
                var persistedUser = await _userManager.FindByNameAsync(userName);
                var roles = role.Equals("op", StringComparison.InvariantCultureIgnoreCase) ? OpRoles : RegularRoles;
                await _userManager.AddToRolesAsync(persistedUser, roles);

                // Associate a login with the user, so they can use the bot.
                await _userManager.AddLoginAsync(persistedUser, new UserLoginInfo("IrcHost", ircHostname, userName));

                // Prefix match is a custom field, so we'll update it out of band via EF directly.
                if (prefixMatch)
                {
                    var login = _context.UserLogins.Single(record =>
                        record.LoginProvider == "IrcHost" && record.ProviderKey == ircHostname);

                    login.MatchType = "StartsWith";

                    _context.UserLogins.Update(login);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                Log.Information("{0} (identity: {1}) has just created user {2}", request.UserName,
                    request.IdentityUser.UserName, userName);
                await SendResponse(request,
                    $"successfully created user {userName}, with roles [{string.Join(", ", roles)}] (match type: {(prefixMatch ? "StartsWith" : "Literal")})");
            }
        }

        private async Task RemoveUser(IrcPrivMessage request, string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (null != user)
            {
                await _userManager.DeleteAsync(user);
                await SendResponse(request, $"Successfully deleted user {userName}.");

                Log.Information("{0} (identity: {1}) has just deleted user {2} ", request.UserName,
                    request.IdentityUser.UserName, userName);
            }
            else
            {
                await SendResponse(request, $"User {userName} does not exist.");
            }
        }
    }
}
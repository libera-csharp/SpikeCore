using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpikeCore.Data.Models;
using SpikeCore.Web.Models;
using SpikeCore.Web.TokenProviders;

namespace SpikeCore.Web.Controllers
{
    /// <summary>
    /// Provides a controller for authenticating users via validating tokens issued by the 
    /// <see cref="PasswordlessLoginTokenProvider{TUser}"/>.
    ///
    /// This is based on https://www.scottbrady91.com/ASPNET-Identity/Implementing-Mediums-Passwordless-Authentication-using-ASPNET-Core-Identity.
    /// </summary>
    public class AuthenticationController : Controller
    {
        private readonly UserManager<SpikeCoreUser> _userManager;

        public AuthenticationController(UserManager<SpikeCoreUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> TokenCallback(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);

            var isValid =
                await _userManager.VerifyUserTokenAsync(user, "PasswordlessLoginProvider", "passwordless-auth", token);

            if (isValid)
            {
                // This prevents replay attacks by swapping out the security stamp. This works because all  tokens
                // contain the security stamp that was used to generate them.
                await _userManager.UpdateSecurityStampAsync(user);

                await HttpContext.SignInAsync(
                    IdentityConstants.ApplicationScheme,
                    new ClaimsPrincipal(
                        new ClaimsIdentity(new List<Claim> {new Claim("sub", user.Id)},
                            IdentityConstants.ApplicationScheme)
                    ));

                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Error");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}
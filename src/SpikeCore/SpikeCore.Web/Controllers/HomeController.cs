using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using SpikeCore.Data.Models;
using SpikeCore.Web.Models;

namespace SpikeCore.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<SpikeCoreUser> userManager;

        public HomeController(UserManager<SpikeCoreUser> userManager)
        {
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateUser()
        {
            var user = new SpikeCoreUser { UserName = "spikecore@example.com", Email = "spikecore@example.com" };
            var result = await userManager.CreateAsync(user, "spikecore");

            return Content("UN: spikecore@example.com   PW: spikecore");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

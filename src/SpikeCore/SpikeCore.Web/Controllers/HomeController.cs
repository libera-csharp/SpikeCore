using System.Diagnostics;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using SpikeCore.Data.Models;
using SpikeCore.Web.Models;

namespace SpikeCore.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<SpikeCoreUser> _userManager;

        public HomeController(UserManager<SpikeCoreUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

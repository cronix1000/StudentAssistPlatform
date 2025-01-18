using Microsoft.AspNetCore.Mvc;
using StudentAssistPlatform.Models;
using System.Diagnostics;

namespace StudentAssistPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SignUp(string name, string email, string year, double gpa)
        {
            // Add the user to the database
            ViewBag.Name = name.ToUpper();
            ViewBag.Email = email;
            ViewBag.Year = year;
            ViewBag.Gpa = gpa;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
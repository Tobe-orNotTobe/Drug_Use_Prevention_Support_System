using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class AdminController : Controller
	{
		public IActionResult Dashboard()
		{
			return View();
		}

		public IActionResult UserManager()
		{
			return View();
		}
	}
}

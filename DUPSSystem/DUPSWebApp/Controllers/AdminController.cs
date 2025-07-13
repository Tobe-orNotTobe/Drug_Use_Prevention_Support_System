using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class AdminController : Controller
	{
		public IActionResult Dashboard()
		{
			ViewData["Title"] = "Dashboard & Report";
			return View();
		}
	}
}

using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class SurveysController : Controller
	{
		public IActionResult Index()
		{
			// Check if user is logged in
			if (HttpContext.Session.GetString("UserToken") == null)
			{
				TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem khảo sát";
				return RedirectToAction("Login", "Auth");
			}

			return View();
		}

		public IActionResult Take(int id)
		{
			// Check if user is logged in
			if (HttpContext.Session.GetString("UserToken") == null)
			{
				TempData["ErrorMessage"] = "Bạn cần đăng nhập để làm khảo sát";
				return RedirectToAction("Login", "Auth");
			}

			ViewBag.SurveyId = id;
			return View();
		}

		public IActionResult Results()
		{
			// Check if user is logged in
			if (HttpContext.Session.GetString("UserToken") == null)
			{
				TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem kết quả khảo sát";
				return RedirectToAction("Login", "Auth");
			}

			return View();
		}

		public IActionResult Details(int id)
		{
			ViewBag.SurveyId = id;
			return View();
		}
	}
}

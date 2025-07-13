using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class AppointmentsController : Controller
	{
		public IActionResult Index()
		{
			// Check if user is logged in
			if (HttpContext.Session.GetString("UserToken") == null)
			{
				TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem lịch hẹn";
				return RedirectToAction("Login", "Auth");
			}

			return View();
		}

		public IActionResult Consultants()
		{
			// Check if user is logged in
			if (HttpContext.Session.GetString("UserToken") == null)
			{
				TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem danh sách tư vấn viên";
				return RedirectToAction("Login", "Auth");
			}

			return View();
		}

		public IActionResult Book(int consultantId)
		{
			// Check if user is logged in
			if (HttpContext.Session.GetString("UserToken") == null)
			{
				TempData["ErrorMessage"] = "Bạn cần đăng nhập để đặt lịch hẹn";
				return RedirectToAction("Login", "Auth");
			}

			ViewBag.ConsultantId = consultantId;
			return View();
		}

		public IActionResult MyAppointments()
		{
			// Check if user is logged in
			if (HttpContext.Session.GetString("UserToken") == null)
			{
				TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem lịch hẹn của bạn";
				return RedirectToAction("Login", "Auth");
			}

			return View();
		}

		public IActionResult Details(int id)
		{
			// Check if user is logged in
			if (HttpContext.Session.GetString("UserToken") == null)
			{
				TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem chi tiết lịch hẹn";
				return RedirectToAction("Login", "Auth");
			}

			ViewBag.AppointmentId = id;
			return View();
		}
	}
}
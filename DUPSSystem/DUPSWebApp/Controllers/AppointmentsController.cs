using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class AppointmentsController : BaseController
	{
		[RoleAuthorization("Member")]
		public IActionResult Book()
		{
			if (!CanBookAppointments() || !IsMember)
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View("ConsultantList");
		}

		[RoleAuthorization("Member", "Staff", "Consultant", "Manager", "Admin")]
		public IActionResult MyAppointments()
		{
			if (!IsAuthenticated)
			{
				return RedirectToAction("Login", "Auth");
			}

			ViewBag.PageTitle = IsMember ? "Lịch hẹn của tôi"
							  : IsConsultant ? "Lịch hẹn được đặt"
							  : "Quản lý lịch hẹn";

			return View("MyAppointment");
		}

		[RoleAuthorization("Consultant", "Manager", "Admin")]
		public IActionResult MySchedule()
		{
			if (!IsConsultant && !IsManager && !IsAdmin)
			{
				return RedirectToAction("AccessDenied", "Home");
			}

			return RedirectToAction("MyAppointments");
		}

		[RoleAuthorization("Staff", "Manager", "Admin")]
		public IActionResult Manage()
		{
			if (!CanManageAppointments())
			{
				return RedirectToAction("AccessDenied", "Home");
			}

			ViewBag.PageTitle = "Quản lý tất cả lịch hẹn";
			ViewBag.IsManagement = true;

			return View("MyAppointment");
		}

		[RoleAuthorization("Member", "Staff", "Consultant", "Manager", "Admin")]
		public IActionResult Details(int id)
		{
			if (!IsAuthenticated)
			{
				return RedirectToAction("Login", "Auth");
			}

			ViewBag.AppointmentId = id;
			return View();
		}

		public IActionResult AccessDenied()
		{
			return View("~/Views/Shared/AccessDenied.cshtml");
		}
	}
}
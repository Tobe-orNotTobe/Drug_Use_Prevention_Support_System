using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class AppointmentsController : BaseController
	{
		[RoleAuthorization("Member", "Staff", "Consultant", "Manager", "Admin")]
		public IActionResult Book()
		{
			if (!CanBookAppointments())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}

		[RoleAuthorization("Member", "Staff", "Consultant", "Manager", "Admin")]
		public IActionResult MyAppointments()
		{
			return View();
		}

		[RoleAuthorization("Consultant", "Manager", "Admin")]
		public IActionResult MySchedule()
		{
			if (!IsConsultant && !IsManager && !IsAdmin)
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}

		[RoleAuthorization("Staff", "Manager", "Admin")]
		public IActionResult Manage()
		{
			if (!CanManageAppointments())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}
	}
}
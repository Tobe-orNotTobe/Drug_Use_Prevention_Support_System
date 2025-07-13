using BusinessObjects.Constants;
using BusinessObjects.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class AppointmentsController : BaseController
	{
		public IActionResult Consultants()
		{
			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult MyAppointments()
		{
			if (!User.CanViewOwnAppointments())
			{
				return ForbiddenRedirect("Bạn không có quyền xem lịch hẹn");
			}

			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Details(int id)
		{
			ViewBag.AppointmentId = id;
			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Manage()
		{
			if (!User.CanManageAppointments())
			{
				return ForbiddenRedirect("Bạn không có quyền quản lý lịch hẹn");
			}

			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.ConsultantRoles)]
		public IActionResult ConsultantAppointments()
		{
			if (!User.IsConsultant() && !User.CanViewAllAppointments())
			{
				return ForbiddenRedirect("Bạn không có quyền xem lịch hẹn tư vấn");
			}

			SetViewBagPermissions();
			return View();
		}
	}
}
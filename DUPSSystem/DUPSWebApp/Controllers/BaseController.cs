using BusinessObjects.Constants;
using BusinessObjects.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class BaseController : Controller
	{
		protected void SetViewBagPermissions()
		{
			// Role information
			ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
			ViewBag.UserRole = User.GetHighestRole();
			ViewBag.IsGuest = User.IsGuest();
			ViewBag.IsMember = User.IsMember();
			ViewBag.IsStaff = User.IsStaff();
			ViewBag.IsConsultant = User.IsConsultant();
			ViewBag.IsManager = User.IsManager();
			ViewBag.IsAdmin = User.IsAdmin();

			// Permission flags
			ViewBag.CanViewCourses = User.CanViewCourses();
			ViewBag.CanRegisterCourses = User.CanRegisterCourses();
			ViewBag.CanManageCourses = User.CanManageCourses();

			ViewBag.CanTakeSurveys = User.CanTakeSurveys();
			ViewBag.CanManageSurveys = User.CanManageSurveys();

			ViewBag.CanBookAppointments = User.CanBookAppointments();
			ViewBag.CanViewOwnAppointments = User.CanViewOwnAppointments();
			ViewBag.CanViewAllAppointments = User.CanViewAllAppointments();
			ViewBag.CanManageAppointments = User.CanManageAppointments();

			ViewBag.CanViewDashboard = User.CanViewDashboard();
			ViewBag.CanViewReports = User.CanViewReports();
			ViewBag.CanViewAllReports = User.CanViewAllReports();

			ViewBag.CanManageUsers = User.CanManageUsers();
			ViewBag.CanManageConsultants = User.CanManageConsultants();
			ViewBag.CanManagePrograms = User.CanManagePrograms();

			// User information
			ViewBag.CurrentUserId = User.GetUserId();
			ViewBag.CurrentUserEmail = User.GetUserEmail();
			ViewBag.CurrentUserFullName = User.GetUserFullName();
		}

		protected IActionResult ForbiddenRedirect(string message = "Bạn không có quyền truy cập trang này")
		{
			TempData["ErrorMessage"] = message;
			return RedirectToAction("Index", "Home");
		}

		protected void SetSuccessMessage(string message)
		{
			TempData["SuccessMessage"] = message;
		}

		protected void SetErrorMessage(string message)
		{
			TempData["ErrorMessage"] = message;
		}

		protected void SetInfoMessage(string message)
		{
			TempData["InfoMessage"] = message;
		}

		protected void SetWarningMessage(string message)
		{
			TempData["WarningMessage"] = message;
		}
	}
}
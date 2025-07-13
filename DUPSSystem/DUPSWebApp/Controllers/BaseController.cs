using BusinessObjects.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public abstract class BaseController : Controller
	{
		protected int CurrentUserId => User.GetUserId();

		protected string CurrentUserEmail => User.GetUserEmail();

		protected string CurrentUserRole => User.GetHighestRole();

		protected bool IsAuthenticated => User.IsAuthenticated();

		protected bool IsCurrentUserOwner(int resourceUserId)
		{
			return CurrentUserId == resourceUserId || User.CanViewAllReports();
		}

		protected IActionResult RedirectToLogin(string message = "Vui lòng đăng nhập để tiếp tục")
		{
			TempData["ErrorMessage"] = message;
			return RedirectToAction("Login", "Auth");
		}

		protected IActionResult ForbiddenRedirect(string message = "Bạn không có quyền truy cập chức năng này")
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

		// Set ViewBag data cho UI permissions
		protected void SetViewBagPermissions()
		{
			ViewBag.IsAuthenticated = IsAuthenticated;
			ViewBag.CurrentUserId = CurrentUserId;
			ViewBag.CurrentUserRole = CurrentUserRole;
			ViewBag.CanManageCourses = User.CanManageCourses();
			ViewBag.CanManageSurveys = User.CanManageSurveys();
			ViewBag.CanManageUsers = User.CanManageUsers();
			ViewBag.CanManageConsultants = User.CanManageConsultants();
			ViewBag.CanViewReports = User.CanViewReports();
			ViewBag.CanViewAllAppointments = User.CanViewAllAppointments();
			ViewBag.IsAdmin = User.IsAdmin();
			ViewBag.IsManager = User.IsManager();
			ViewBag.IsStaff = User.IsStaff();
			ViewBag.IsConsultant = User.IsConsultant();
		}
	}
}

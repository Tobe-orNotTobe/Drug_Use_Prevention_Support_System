using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DUPSWebApp.Controllers
{
	public class BaseController : Controller
	{
		protected string CurrentUserRole => HttpContext.Session.GetString("Role");
		protected int CurrentUserId => int.TryParse(HttpContext.Session.GetString("UserId"), out int id) ? id : 0;
		protected string CurrentUserEmail => HttpContext.Session.GetString("Email") ?? "";
		protected string CurrentUserName => HttpContext.Session.GetString("FullName") ?? "";

		protected bool IsMember => CurrentUserRole == "Member";
		protected bool IsStaff => CurrentUserRole == "Staff";
		protected bool IsConsultant => CurrentUserRole == "Consultant";
		protected bool IsManager => CurrentUserRole == "Manager";
		protected bool IsAdmin => CurrentUserRole == "Admin";

		protected bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUserRole);

		protected bool HasRole(params string[] roles)
		{
			return IsAuthenticated && roles.Contains(CurrentUserRole);
		}

		protected bool CanViewCourses() => true;
		protected bool CanManageCourses() => HasRole("Staff", "Manager", "Admin");
		protected bool CanRegisterCourses() => HasRole("Member", "Staff", "Consultant", "Manager", "Admin");

		protected bool CanTakeSurveys() => HasRole("Member", "Staff", "Consultant", "Manager", "Admin");
		protected bool CanManageSurveys() => HasRole("Staff", "Manager", "Admin");

		protected bool CanBookAppointments() => HasRole("Member", "Staff", "Consultant", "Manager", "Admin");
		protected bool CanViewAllAppointments() => HasRole("Staff", "Manager", "Admin");
		protected bool CanManageAppointments() => HasRole("Staff", "Manager", "Admin");

		protected bool CanManageConsultants() => HasRole("Manager", "Admin");
		protected bool CanManageUsers() => IsAdmin;
		protected bool CanViewDashboard() => HasRole("Manager", "Admin");

		public IActionResult CheckAuthorization(params string[] allowedRoles)
		{
			if (!IsAuthenticated)
			{
				return RedirectToAction("Login", "Auth");
			}

			if (!allowedRoles.Contains(CurrentUserRole))
			{
				return RedirectToAction("AccessDenied", "Home");
			}

			return null;
		}

		protected bool HasAuthorization(params string[] allowedRoles)
		{
			return IsAuthenticated && allowedRoles.Contains(CurrentUserRole);
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			ViewBag.CurrentUserRole = CurrentUserRole;
			ViewBag.CurrentUserName = CurrentUserName;
			ViewBag.CurrentUserEmail = CurrentUserEmail;
			ViewBag.IsAuthenticated = IsAuthenticated;
			ViewBag.AuthToken = HttpContext.Session.GetString("Token") ?? "";
			ViewBag.CurrentUserId = CurrentUserId;

			base.OnActionExecuting(context);
		}
	}

	public class RoleAuthorizationAttribute : ActionFilterAttribute
	{
		private readonly string[] _allowedRoles;

		public RoleAuthorizationAttribute(params string[] allowedRoles)
		{
			_allowedRoles = allowedRoles;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var controller = context.Controller as BaseController;
			if (controller != null)
			{
				var authResult = controller.CheckAuthorization(_allowedRoles);
				if (authResult != null)
				{
					context.Result = authResult;
					return;
				}
			}
			else
			{
				var currentRole = context.HttpContext.Session.GetString("Role");

				if (string.IsNullOrEmpty(currentRole))
				{
					context.Result = new RedirectToActionResult("Login", "Auth", null);
					return;
				}

				if (!_allowedRoles.Contains(currentRole))
				{
					context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
					return;
				}
			}

			base.OnActionExecuting(context);
		}
	}

	public class RequireRoleAttribute : ActionFilterAttribute
	{
		private readonly string[] _allowedRoles;

		public RequireRoleAttribute(params string[] allowedRoles)
		{
			_allowedRoles = allowedRoles;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var currentRole = context.HttpContext.Session.GetString("Role");

			if (string.IsNullOrEmpty(currentRole))
			{
				context.Result = new RedirectToActionResult("Login", "Auth", null);
				return;
			}

			if (!_allowedRoles.Contains(currentRole))
			{
				context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
				return;
			}

			base.OnActionExecuting(context);
		}
	}
}

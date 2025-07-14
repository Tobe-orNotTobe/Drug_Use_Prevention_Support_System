using BusinessObjects.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace DUPSWebApp.Attributes
{
	public class PermissionAttribute : Attribute, IAuthorizationFilter
	{
		private readonly string _permission;
		private readonly string _errorMessage;

		public PermissionAttribute(string permission, string errorMessage = null)
		{
			_permission = permission;
			_errorMessage = errorMessage ?? $"Bạn không có quyền {permission}";
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var user = context.HttpContext.User;

			if (!user.Identity.IsAuthenticated)
			{
				context.Result = new RedirectToActionResult("Login", "Auth", null);
				return;
			}

			var hasPermission = _permission switch
			{
				"ManageUsers" => user.CanManageUsers(),
				"ManageCourses" => user.CanManageCourses(),
				"ManageSurveys" => user.CanManageSurveys(),
				"ManageAppointments" => user.CanManageAppointments(),
				"ManageConsultants" => user.CanManageConsultants(),
				"ViewDashboard" => user.CanViewDashboard(),
				"ViewReports" => user.CanViewReports(),
				"RegisterCourses" => user.CanRegisterCourses(),
				"TakeSurveys" => user.CanTakeSurveys(),
				"BookAppointments" => user.CanBookAppointments(),
				"ViewOwnAppointments" => user.CanViewOwnAppointments(),
				_ => false
			};

			if (!hasPermission)
			{
				context.HttpContext.Response.StatusCode = 403;
				context.Result = new RedirectToActionResult("HttpStatusCodeHandler", "Error",
					new { statusCode = 403 });
			}
		}
	}

	public class RequireRoleAttribute : Attribute, IAuthorizationFilter
	{
		private readonly string[] _roles;

		public RequireRoleAttribute(params string[] roles)
		{
			_roles = roles;
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var user = context.HttpContext.User;

			if (!user.Identity.IsAuthenticated)
			{
				context.Result = new RedirectToActionResult("Login", "Auth", null);
				return;
			}

			if (!user.IsInAnyRole(_roles))
			{
				context.HttpContext.Response.StatusCode = 403;
				context.Result = new RedirectToActionResult("HttpStatusCodeHandler", "Error",
					new { statusCode = 403 });
			}
		}
	}

	public class RequireOwnershipAttribute : Attribute, IAuthorizationFilter
	{
		private readonly string _ownerIdParameter;
		private readonly string _fallbackPermission;

		public RequireOwnershipAttribute(string ownerIdParameter, string fallbackPermission = null)
		{
			_ownerIdParameter = ownerIdParameter;
			_fallbackPermission = fallbackPermission;
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var user = context.HttpContext.User;

			if (!user.Identity.IsAuthenticated)
			{
				context.Result = new RedirectToActionResult("Login", "Auth", null);
				return;
			}

			var currentUserId = user.GetUserId();

			if (context.RouteData.Values.TryGetValue(_ownerIdParameter, out var ownerIdValue) &&
				int.TryParse(ownerIdValue?.ToString(), out var ownerId))
			{
				if (currentUserId == ownerId) return;
			}

			if (!string.IsNullOrEmpty(_fallbackPermission))
			{
				var hasPermission = _fallbackPermission switch
				{
					"ManageUsers" => user.CanManageUsers(),
					"ManageAppointments" => user.CanManageAppointments(),
					"ViewAllAppointments" => user.CanViewAllAppointments(),
					_ => false
				};

				if (hasPermission) return;
			}

			context.HttpContext.Response.StatusCode = 403;
			context.Result = new RedirectToActionResult("HttpStatusCodeHandler", "Error",
				new { statusCode = 403 });
		}
	}
}
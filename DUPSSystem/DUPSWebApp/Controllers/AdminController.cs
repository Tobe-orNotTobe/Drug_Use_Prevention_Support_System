using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class AdminController : BaseController
	{
		[RoleAuthorization("Admin")]
		public IActionResult Dashboard()
		{
			if (!CanManageUsers())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}

		[RoleAuthorization("Admin")]
		public IActionResult UserManagement()
		{
			if (!CanManageUsers())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}

		[RoleAuthorization("Admin")]
		public IActionResult Users()
		{
			if (!CanManageUsers())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return RedirectToAction("UserManagement");
		}

		[RoleAuthorization("Admin")]
		public IActionResult Roles()
		{
			if (!CanManageUsers())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}
	}
}
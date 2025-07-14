using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class AdminController : BaseController
	{
		[RoleAuthorization("Admin")]
		public IActionResult Dashboard()
		{
			return View();
		}

		[RoleAuthorization("Admin")]
		public IActionResult Users()
		{
			if (!CanManageUsers())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}

		[RoleAuthorization("Admin")]
		public IActionResult Roles()
		{
			return View();
		}
	}
}

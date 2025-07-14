using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class ReportsController : BaseController
	{
		[RoleAuthorization("Manager", "Admin")]
		public IActionResult Dashboard()
		{
			if (!CanViewDashboard())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}

		[RoleAuthorization("Manager", "Admin")]
		public IActionResult Index()
		{
			return View();
		}

		[RoleAuthorization("Admin")]
		public IActionResult ExportExcel()
		{
			return View();
		}
	}
}

using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class ConsultantsController : BaseController
	{
		public IActionResult Index()
		{
			return View();
		}

		[RoleAuthorization("Manager", "Admin")]
		public IActionResult Manage()
		{
			if (!CanManageConsultants())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}
	}
}

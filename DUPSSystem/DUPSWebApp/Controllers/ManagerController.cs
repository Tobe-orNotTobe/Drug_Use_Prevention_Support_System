using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class ManagerController : BaseController
	{
		[RoleAuthorization("Manager", "Admin")]
		public IActionResult Dashboard()
		{
			return View();
		}
	}
}

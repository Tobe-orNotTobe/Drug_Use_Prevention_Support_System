using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class SurveysController : BaseController
	{
		public IActionResult Index()
		{
			return View();
		}

		[RoleAuthorization("Member", "Staff", "Consultant", "Manager", "Admin")]
		public IActionResult Take(int surveyId)
		{
			if (!CanTakeSurveys())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}

		[RoleAuthorization("Member", "Staff", "Consultant", "Manager", "Admin")]
		public IActionResult MyResults()
		{
			return View();
		}

		[RoleAuthorization("Staff", "Manager", "Admin")]
		public IActionResult Manage()
		{
			if (!CanManageSurveys())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}
	}
}

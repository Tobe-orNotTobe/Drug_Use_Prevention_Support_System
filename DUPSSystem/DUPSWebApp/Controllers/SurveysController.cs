using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class SurveysController : BaseController
	{
		public IActionResult Index()
		{
			if (IsMember)
			{
				return View("MemberIndex");
			}

			if (!CanManageSurveys())
			{
				return RedirectToAction("AccessDenied", "Home");
			}

			return View();
		}

		[RoleAuthorization("Staff", "Admin")]
		public IActionResult Create()
		{
			if (!CanManageSurveys())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}

		[RoleAuthorization("Staff", "Admin")]
		public IActionResult Edit(int id)
		{
			if (!CanManageSurveys())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			ViewBag.SurveyId = id;
			return View();
		}

		public IActionResult Details(int id)
		{
			ViewBag.SurveyId = id;
			return View();
		}

		[RoleAuthorization("Member", "Staff", "Consultant", "Manager", "Admin")]
		public IActionResult Take(int id)
		{
			if (!CanTakeSurveys())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			ViewBag.SurveyId = id;
			return View();
		}

		[RoleAuthorization("Member", "Staff", "Admin")]
		public IActionResult Result(int id)
		{
			ViewBag.SurveyId = id;

			if (IsMember)
			{
				return View("MemberResult");
			}

			if (IsAdmin)
			{
				return View("AdminResult");
			}

			return View();
		}

		[RoleAuthorization("Admin")]
		public IActionResult AllResults()
		{
			if (!IsAdmin)
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}
	}
}
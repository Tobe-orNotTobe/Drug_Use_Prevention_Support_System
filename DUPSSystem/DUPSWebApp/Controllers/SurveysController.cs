using BusinessObjects.Constants;
using BusinessObjects.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class SurveysController : BaseController
	{
		public IActionResult Index()
		{
			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Take(int id)
		{
			if (!User.CanTakeSurveys())
			{
				return ForbiddenRedirect("Bạn không có quyền làm khảo sát");
			}

			ViewBag.SurveyId = id;
			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Results()
		{
			SetViewBagPermissions();
			return View();
		}

		public IActionResult Details(int id)
		{
			ViewBag.SurveyId = id;
			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Manage()
		{
			if (!User.CanManageSurveys())
			{
				return ForbiddenRedirect("Bạn không có quyền quản lý khảo sát");
			}

			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Create()
		{
			if (!User.CanManageSurveys())
			{
				return ForbiddenRedirect("Bạn không có quyền tạo khảo sát");
			}

			SetViewBagPermissions();
			return View();
		}

		// Only Staff+ can edit surveys
		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Edit(int id)
		{
			if (!User.CanManageSurveys())
			{
				return ForbiddenRedirect("Bạn không có quyền sửa khảo sát");
			}

			ViewBag.SurveyId = id;
			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.SeniorRoles)]
		public IActionResult AllResults()
		{
			if (!User.CanViewReports())
			{
				return ForbiddenRedirect("Bạn không có quyền xem tất cả kết quả khảo sát");
			}

			SetViewBagPermissions();
			return View();
		}
	}
}

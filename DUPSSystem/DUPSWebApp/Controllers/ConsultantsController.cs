using BusinessObjects.Constants;
using BusinessObjects.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class ConsultantsController : BaseController
	{
		public IActionResult Index()
		{
			SetViewBagPermissions();
			return View();
		}

		public IActionResult Details(int id)
		{
			ViewBag.ConsultantId = id;
			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.SeniorRoles)]
		public IActionResult Manage()
		{
			if (!User.CanManageConsultants())
			{
				return ForbiddenRedirect("Bạn không có quyền quản lý tư vấn viên");
			}

			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.SeniorRoles)]
		public IActionResult Create()
		{
			if (!User.CanManageConsultants())
			{
				return ForbiddenRedirect("Bạn không có quyền tạo tư vấn viên");
			}

			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.ConsultantRoles)]
		public IActionResult Edit(int id)
		{
			// Additional ownership check will be done in the view via API
			ViewBag.ConsultantId = id;
			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.ConsultantRoles)]
		public IActionResult Profile()
		{
			if (!User.IsConsultant() && !User.CanManageConsultants())
			{
				return ForbiddenRedirect("Bạn không có quyền xem hồ sơ tư vấn viên");
			}

			SetViewBagPermissions();
			return View();
		}
	}
}

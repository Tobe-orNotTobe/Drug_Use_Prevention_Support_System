using BusinessObjects.Constants;
using BusinessObjects.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	[Authorize(Roles = Roles.AdminOnly)]
	public class UsersController : BaseController
	{
		public IActionResult Index()
		{
			if (!User.CanManageUsers())
			{
				return ForbiddenRedirect("Bạn không có quyền quản lý người dùng");
			}

			SetViewBagPermissions();
			return View();
		}

		public IActionResult Create()
		{
			if (!User.CanManageUsers())
			{
				return ForbiddenRedirect("Bạn không có quyền tạo người dùng");
			}

			SetViewBagPermissions();
			return View();
		}

		public IActionResult Edit(int id)
		{
			if (!User.CanManageUsers())
			{
				return ForbiddenRedirect("Bạn không có quyền sửa người dùng");
			}

			ViewBag.UserId = id;
			SetViewBagPermissions();
			return View();
		}

		public IActionResult Details(int id)
		{
			if (!User.CanManageUsers())
			{
				return ForbiddenRedirect("Bạn không có quyền xem chi tiết người dùng");
			}

			ViewBag.UserId = id;
			SetViewBagPermissions();
			return View();
		}
	}
}

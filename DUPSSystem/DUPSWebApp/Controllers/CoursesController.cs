using BusinessObjects.Constants;
using BusinessObjects.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class CoursesController : BaseController
	{
		public IActionResult Index()
		{
			SetViewBagPermissions();
			return View();
		}

		public IActionResult Details(int id)
		{
			ViewBag.CourseId = id;
			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult MyCourses()
		{
			if (!User.CanRegisterCourses())
			{
				return ForbiddenRedirect("Bạn không có quyền xem khóa học của mình");
			}

			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Register(int id)
		{
			if (!User.CanRegisterCourses())
			{
				return ForbiddenRedirect("Bạn không có quyền đăng ký khóa học");
			}

			ViewBag.CourseId = id;
			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Book(int id)
		{
			if (!User.CanRegisterCourses())
			{
				return ForbiddenRedirect("Bạn không có quyền đăng ký khóa học");
			}

			ViewBag.CourseId = id;
			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Manage()
		{
			if (!User.CanManageCourses())
			{
				return ForbiddenRedirect("Bạn không có quyền quản lý khóa học");
			}

			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Create()
		{
			if (!User.CanManageCourses())
			{
				return ForbiddenRedirect("Bạn không có quyền tạo khóa học");
			}

			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Edit(int id)
		{
			if (!User.CanManageCourses())
			{
				return ForbiddenRedirect("Bạn không có quyền sửa khóa học");
			}

			ViewBag.CourseId = id;
			SetViewBagPermissions();
			return View();
		}
	}
}
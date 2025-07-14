using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class CoursesController : BaseController
	{
		public IActionResult Index()
		{
			return View();
		}

		[RoleAuthorization("Member")]
		public IActionResult MyCourses()
		{
			if (!IsMember)
			{
				return RedirectToAction("AccessDenied", "Home");
			}

			return View();
		}

		[RoleAuthorization("Member", "Staff", "Consultant", "Manager", "Admin")]
		public IActionResult Register(int courseId)
		{
			return View();
		}

		[RoleAuthorization("Staff", "Manager", "Admin")]
		public IActionResult Manage()
		{
			if (!CanManageCourses())
			{
				return RedirectToAction("AccessDenied", "Home");
			}
			return View();
		}

		[RoleAuthorization("Staff", "Manager", "Admin")]
		public IActionResult Create()
		{
			return View();
		}

		[RoleAuthorization("Staff", "Manager", "Admin")]
		public IActionResult Edit(int id)
		{
			return View();
		}

		[RoleAuthorization("Staff, Admin")]
		public IActionResult Delete(int id)
		{
			return View();
		}
	}
}
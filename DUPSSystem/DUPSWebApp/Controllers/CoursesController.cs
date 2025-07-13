using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class CoursesController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Details(int id)
		{
			ViewBag.CourseId = id;
			return View();
		}

		public IActionResult MyCourses()
		{
			return View();
		}
	}
}
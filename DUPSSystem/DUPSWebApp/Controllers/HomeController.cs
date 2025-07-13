using BusinessObjects.Constants;
using BusinessObjects.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class HomeController : BaseController
	{
		public IActionResult Index()
		{
			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Dashboard()
		{
			if (!User.CanViewDashboard())
			{
				return ForbiddenRedirect("Bạn không có quyền truy cập Dashboard");
			}

			SetViewBagPermissions();

			// Redirect to role-specific dashboard
			if (User.IsAdmin())
			{
				return View("AdminDashboard");
			}
			else if (User.IsManager())
			{
				return View("ManagerDashboard");
			}
			else if (User.IsStaff())
			{
				return View("StaffDashboard");
			}
			else if (User.IsConsultant())
			{
				return View("ConsultantDashboard");
			}	
			else
			{
				return View("UserDashboard");
			}
		}

		[Authorize(Roles = Roles.SeniorRoles)]
		public IActionResult Reports()
		{
			if (!User.CanViewReports())
			{
				return ForbiddenRedirect("Bạn không có quyền xem báo cáo");
			}

			SetViewBagPermissions();
			return View();
		}

		[Authorize(Roles = Roles.AdminOnly)]
		public IActionResult SystemManagement()
		{
			SetViewBagPermissions();
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}
	}
}
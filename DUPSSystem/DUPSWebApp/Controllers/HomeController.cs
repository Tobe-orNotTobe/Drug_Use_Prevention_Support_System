using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class HomeController : BaseController
	{
		public IActionResult Index()
		{
			ViewBag.WelcomeMessage = GetWelcomeMessage();
			return View();
		}

		public IActionResult About()
		{
			return View();
		}

		public IActionResult Contact()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		public IActionResult AccessDenied()
		{
			ViewBag.CurrentRole = CurrentUserRole;
			ViewBag.UserName = CurrentUserName;
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View();
		}

		private string GetWelcomeMessage()
		{
			return CurrentUserRole switch
			{
				"Admin" => $"Chào mừng Quản trị viên {CurrentUserName}! Bạn có toàn quyền quản lý hệ thống.",
				"Manager" => $"Chào mừng Quản lý {CurrentUserName}! Bạn có thể quản lý tư vấn viên và xem báo cáo.",
				"Staff" => $"Chào mừng Nhân viên {CurrentUserName}! Bạn có thể quản lý khóa học và khảo sát.",
				"Consultant" => $"Chào mừng Tư vấn viên {CurrentUserName}! Bạn có thể quản lý lịch hẹn của mình.",
				"Member" => $"Chào mừng {CurrentUserName}! Hãy khám phá các khóa học và dịch vụ tư vấn.",
				_ => "Chào mừng bạn đến với Hệ thống Hỗ trợ Phòng ngừa Sử dụng Ma túy!"
			};
		}
	}
}
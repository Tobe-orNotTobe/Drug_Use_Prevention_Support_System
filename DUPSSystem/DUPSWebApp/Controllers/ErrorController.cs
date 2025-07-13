using Microsoft.AspNetCore.Mvc;

namespace DUPSWebApp.Controllers
{
	public class ErrorController : BaseController
	{
		[Route("Error/{statusCode}")]
		public IActionResult HttpStatusCodeHandler(int statusCode)
		{
			switch (statusCode)
			{
				case 403:
					ViewBag.ErrorMessage = "Bạn không có quyền truy cập trang này";
					ViewBag.ErrorCode = "403";
					break;
				case 404:
					ViewBag.ErrorMessage = "Trang bạn đang tìm không tồn tại";
					ViewBag.ErrorCode = "404";
					break;
				case 401:
					ViewBag.ErrorMessage = "Bạn cần đăng nhập để truy cập trang này";
					ViewBag.ErrorCode = "401";
					return RedirectToAction("Login", "Auth");
				default:
					ViewBag.ErrorMessage = "Đã xảy ra lỗi không mong muốn";
					ViewBag.ErrorCode = statusCode.ToString();
					break;
			}

			return View("Error");
		}
	}
}

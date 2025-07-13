using BusinessObjects.Constants;
using BusinessObjects.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace DUPSWebAPI.Controllers
{
	public abstract class BaseODataController : ODataController
	{
		protected int CurrentUserId => User.GetUserId();

		protected string CurrentUserEmail => User.GetUserEmail();

		protected bool IsCurrentUserOwner(int resourceUserId)
		{
			return CurrentUserId == resourceUserId || User.CanViewAllReports();
		}

		protected IActionResult ForbiddenResponse(string message = "Bạn không có quyền truy cập chức năng này")
		{
			return StatusCode(403, new
			{
				success = false,
				message,
				statusCode = 403
			});
		}

		protected IActionResult UnauthorizedResponse(string message = "Vui lòng đăng nhập để tiếp tục")
		{
			return StatusCode(401, new
			{
				success = false,
				message,
				statusCode = 401
			});
		}

		protected IActionResult SuccessResponse(object data = null, string message = "Thành công")
		{
			return Ok(new
			{
				success = true,
				message,
				data
			});
		}

		protected IActionResult ErrorResponse(string message = "Đã xảy ra lỗi", object errors = null)
		{
			return BadRequest(new
			{
				success = false,
				message,
				errors
			});
		}

		protected IActionResult NotFoundResponse(string message = "Không tìm thấy dữ liệu")
		{
			return NotFound(new
			{
				success = false,
				message,
				statusCode = 404
			});
		}
	}
}

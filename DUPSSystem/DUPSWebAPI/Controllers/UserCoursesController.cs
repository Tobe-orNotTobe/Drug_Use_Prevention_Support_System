using BusinessObjects;
using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services;
using Services.Interfaces;

namespace DUPSWebAPI.Controllers
{
	public class UserCoursesController : ODataController
	{
		private readonly IUserCourseService _userCourseService;

		public UserCoursesController(IUserCourseService userCourseService)
		{
			_userCourseService = userCourseService;
		}

		[EnableQuery]
		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Get()
		{
			try
			{
				var userCourses = _userCourseService.GetAllUserCourses().AsQueryable();

				if (User.CanViewAllReports())
				{
					return Ok(userCourses);
				}
				else
				{
					var userId = User.GetUserId();
					var filtered = userCourses.Where(uc => uc.UserId == userId);
					return Ok(filtered);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[EnableQuery]
		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Get([FromODataUri] int key)
		{
			try
			{
				var userCourse = _userCourseService.GetUserCourseById(key);
				if (userCourse == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy đăng ký khóa học" });
				}

				if (!User.IsOwnerOrCanViewAll(userCourse.UserId))
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền xem thông tin này" });
				}

				return Ok(userCourse);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Post([FromBody] CourseRegistrationRequest request)
		{
			try
			{
				if (!User.CanRegisterCourses())
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền đăng ký khóa học" });
				}

				// Force UserId to be current user (security)
				request.UserId = User.GetUserId();

				if (!ModelState.IsValid)
				{
					return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
				}

				var result = _userCourseService.RegisterUserForCourse(request);
				return Created(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Delete([FromODataUri] int key)
		{
			try
			{
				var userCourse = _userCourseService.GetUserCourseById(key);
				if (userCourse == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy đăng ký khóa học" });
				}

				// Check permission to delete
				if (!User.IsOwnerOrCanViewAll(userCourse.UserId))
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền hủy đăng ký này" });
				}

				_userCourseService.DeleteUserCourse(userCourse);
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

	}
}
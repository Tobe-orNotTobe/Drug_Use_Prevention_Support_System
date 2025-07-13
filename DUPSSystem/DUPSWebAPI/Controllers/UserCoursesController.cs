using BusinessObjects;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
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
		public IActionResult Get()
		{
			try
			{
				var userCourses = _userCourseService.GetAllUserCourses().AsQueryable();
				return Ok(userCourses);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[EnableQuery]
		public IActionResult Get([FromODataUri] int key)
		{
			try
			{
				var userCourse = _userCourseService.GetUserCourseById(key);
				if (userCourse == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy đăng ký khóa học" });
				}
				return Ok(userCourse);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		public IActionResult Post([FromBody] CourseRegistrationRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var result = _userCourseService.RegisterUserForCourse(request);

				if (result.Success)
				{
					return Created("", new { success = true, message = result.Message, data = new { UserCourseId = result.UserCourseId } });
				}
				else
				{
					return BadRequest(new { success = false, message = result.Message });
				}
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		public IActionResult Delete([FromODataUri] int key)
		{
			try
			{
				var userCourse = _userCourseService.GetUserCourseById(key);
				if (userCourse == null)
				{
					return NotFound();
				}

				_userCourseService.DeleteUserCourse(userCourse);
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[HttpPost("odata/UserCourses/Complete")]
		public IActionResult MarkAsCompleted([FromBody] CourseRegistrationRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				_userCourseService.MarkCourseAsCompleted(request.UserId, request.CourseId);

				return Ok(new { success = true, message = "Đánh dấu hoàn thành khóa học thành công" });
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}
	}
}
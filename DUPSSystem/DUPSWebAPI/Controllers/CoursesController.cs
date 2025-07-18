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
using Services.Interfaces;

namespace DUPSWebAPI.Controllers
{
	public class CoursesController : ODataController
	{
		private readonly ICourseService _courseService;

		public CoursesController(ICourseService courseService)
		{
			_courseService = courseService;
		}

		[EnableQuery(PageSize = 20)]
		[AllowAnonymous]
		public IActionResult Get()
		{
			try
			{
				var courses = _courseService.GetAllCourses().AsQueryable();
				return Ok(courses);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[EnableQuery]
		[AllowAnonymous]
		public IActionResult Get([FromODataUri] int key)
		{
			try
			{
				var course = _courseService.GetCourseById(key);
				if (course == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy khóa học" });
				}
				return Ok(course);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Put([FromODataUri] int key, [FromBody] Course course)
		{
			try
			{
				if (!User.CanManageCourses())
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền sửa khóa học" });
				}

				if (!ModelState.IsValid)
				{
					return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
				}

				if (key != course.CourseId)
				{
					return BadRequest(new { success = false, message = "Key mismatch" });
				}

				var existingCourse = _courseService.GetCourseById(key);
				if (existingCourse == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy khóa học" });
				}

				course.CreatedAt = existingCourse.CreatedAt;

				_courseService.UpdateCourse(course);
				return Updated(course);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Post([FromBody] Course course)
		{
			try
			{
				if (!User.CanManageCourses())
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền tạo khóa học" });
				}

				if (!ModelState.IsValid)
				{
					return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
				}

				_courseService.SaveCourse(course);
				return Created(course);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Patch([FromODataUri] int key, [FromBody] Delta<Course> delta)
		{
			try
			{
				if (!User.CanManageCourses())
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền sửa khóa học" });
				}

				var course = _courseService.GetCourseById(key);
				if (course == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy khóa học" });
				}

				delta.Patch(course);
				_courseService.UpdateCourse(course);

				return Updated(course);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Delete([FromODataUri] int key)
		{
			try
			{
				var course = _courseService.GetCourseById(key);
				if (course == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy khóa học" });
				}

				_courseService.DeleteCourse(course);
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = "Xóa khóa học thất bại" });
			}
		}
	}
}
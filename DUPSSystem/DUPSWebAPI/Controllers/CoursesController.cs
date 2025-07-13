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
	public class CoursesController : ODataController
	{
		private readonly ICourseService _courseService;

		public CoursesController(ICourseService courseService)
		{
			_courseService = courseService;
		}

		[EnableQuery(PageSize = 20)]
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

		public IActionResult Post([FromBody] Course course)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				course.CreatedAt = DateTime.UtcNow;
				course.IsActive = true;
				_courseService.SaveCourse(course);

				return Created(course);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		public IActionResult Patch([FromODataUri] int key, [FromBody] Delta<Course> delta)
		{
			try
			{
				var course = _courseService.GetCourseById(key);
				if (course == null)
				{
					return NotFound();
				}

				delta.Patch(course);
				course.UpdatedAt = DateTime.UtcNow;
				_courseService.UpdateCourse(course);

				return Updated(course);
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
				var course = _courseService.GetCourseById(key);
				if (course == null)
				{
					return NotFound();
				}

				_courseService.DeleteCourse(course);
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[HttpGet("odata/Courses/Active")]
		[EnableQuery]
		public IActionResult GetActiveCourses()
		{
			try
			{
				var courses = _courseService.GetActiveCourses().AsQueryable();
				return Ok(courses);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}
	}
}
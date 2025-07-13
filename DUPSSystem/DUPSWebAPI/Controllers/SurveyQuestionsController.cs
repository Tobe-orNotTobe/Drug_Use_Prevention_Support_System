using BusinessObjects;
using BusinessObjects.Constants;
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
	public class SurveyQuestionsController : ODataController
	{
		private readonly ISurveyQuestionService _questionService;

		public SurveyQuestionsController(ISurveyQuestionService questionService)
		{
			_questionService = questionService;
		}

		[EnableQuery(MaxExpansionDepth = 3)]
		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Get()
		{
			try
			{
				var questions = _questionService.GetAll().AsQueryable();
				return Ok(questions);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[EnableQuery(MaxExpansionDepth = 3)]
		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Get([FromODataUri] int key)
		{
			try
			{
				var question = _questionService.GetQuestionById(key);
				if (question == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy câu hỏi" });
				}
				return Ok(question);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Post([FromBody] SurveyQuestion question)
		{
			try
			{
				if (!User.CanManageSurveys())
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền tạo câu hỏi khảo sát" });
				}

				if (!ModelState.IsValid)
				{
					return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
				}

				_questionService.SaveQuestion(question);
				return Created(question);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Patch([FromODataUri] int key, [FromBody] Delta<SurveyQuestion> delta)
		{
			try
			{
				if (!User.CanManageSurveys())
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền sửa câu hỏi khảo sát" });
				}

				var question = _questionService.GetQuestionById(key);
				if (question == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy câu hỏi" });
				}

				delta.Patch(question);
				_questionService.UpdateQuestion(question);

				return Updated(question);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.SeniorRoles)]
		public IActionResult Delete([FromODataUri] int key)
		{
			try
			{
				var question = _questionService.GetQuestionById(key);
				if (question == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy câu hỏi" });
				}

				_questionService.DeleteQuestion(question);
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}
	}
}
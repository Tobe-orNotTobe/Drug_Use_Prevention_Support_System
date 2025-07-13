using BusinessObjects;
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

		[EnableQuery]
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

		[EnableQuery]
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

		[Authorize(Roles = "Admin,Manager,Staff")]
		public IActionResult Post([FromBody] SurveyQuestion question)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				_questionService.SaveQuestion(question);
				return Created(question);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = "Admin,Manager,Staff")]
		public IActionResult Patch([FromODataUri] int key, [FromBody] Delta<SurveyQuestion> delta)
		{
			try
			{
				var question = _questionService.GetQuestionById(key);
				if (question == null)
				{
					return NotFound();
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

		[Authorize(Roles = "Admin,Manager")]
		public IActionResult Delete([FromODataUri] int key)
		{
			try
			{
				var question = _questionService.GetQuestionById(key);
				if (question == null)
				{
					return NotFound();
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

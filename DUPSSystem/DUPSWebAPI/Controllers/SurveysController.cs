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
	public class SurveysController : ODataController
	{
		private readonly ISurveyService _surveyService;

		public SurveysController(ISurveyService surveyService)
		{
			_surveyService = surveyService;
		}

		[EnableQuery(PageSize = 20)]
		public IActionResult Get()
		{
			try
			{
				var surveys = _surveyService.GetAllSurveys().AsQueryable();
				return Ok(surveys);
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
				var survey = _surveyService.GetSurveyById(key);
				if (survey == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy khảo sát" });
				}
				return Ok(survey);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		// FIX: Đổi route để tránh conflict
		[HttpGet]
		[Route("odata/Surveys({key})/Details")]
		public IActionResult GetSurveyDetails([FromRoute] int key)
		{
			try
			{
				var surveyDetails = _surveyService.GetSurveyWithDetails(key);
				if (surveyDetails == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy khảo sát" });
				}

				return Ok(new
				{
					success = true,
					message = "Lấy chi tiết khảo sát thành công",
					data = surveyDetails
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		// THÊM ENDPOINT ĐƠN GIẢN HỢP
		[HttpGet]
		[Route("api/Surveys/{id}/Details")]
		public IActionResult GetSurveyDetailsSimple(int id)
		{
			try
			{
				var surveyDetails = _surveyService.GetSurveyWithDetails(id);
				if (surveyDetails == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy khảo sát" });
				}

				return Ok(new
				{
					success = true,
					message = "Lấy chi tiết khảo sát thành công",
					data = surveyDetails
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = "Admin,Manager,Staff")]
		public IActionResult Post([FromBody] Survey survey)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				_surveyService.SaveSurvey(survey);
				return Created(survey);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = "Admin,Manager,Staff")]
		public IActionResult Patch([FromODataUri] int key, [FromBody] Delta<Survey> delta)
		{
			try
			{
				var survey = _surveyService.GetSurveyById(key);
				if (survey == null)
				{
					return NotFound();
				}

				delta.Patch(survey);
				_surveyService.UpdateSurvey(survey);

				return Updated(survey);
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
				var survey = _surveyService.GetSurveyById(key);
				if (survey == null)
				{
					return NotFound();
				}

				_surveyService.DeleteSurvey(survey);
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[HttpGet("odata/Surveys/Search")]
		[EnableQuery]
		public IActionResult Search([FromQuery] string term = "")
		{
			try
			{
				var surveys = _surveyService.SearchSurveys(term).AsQueryable();
				return Ok(surveys);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}
	}
}

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
	public class SurveysController : ODataController
	{
		private readonly ISurveyService _surveyService;

		public SurveysController(ISurveyService surveyService)
		{
			_surveyService = surveyService;
		}

		[EnableQuery(PageSize = 20)]
		[AllowAnonymous]
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
		[AllowAnonymous]
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

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Post([FromBody] Survey survey)
		{
			try
			{
				if (!User.CanManageSurveys())
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền tạo khảo sát" });
				}


				_surveyService.SaveSurvey(survey);

				var createdSurvey = _surveyService.GetSurveyById(survey.SurveyId);

				return Created($"/odata/Surveys({survey.SurveyId})", new
				{
					success = true,
					message = "Tạo khảo sát thành công",
					data = createdSurvey,
					surveyId = survey.SurveyId
				});
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi tạo khảo sát" });
			}
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Patch([FromODataUri] int key, [FromBody] Delta<Survey> delta)
		{
			try
			{
				if (!User.CanManageSurveys())
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền sửa khảo sát" });
				}

				var survey = _surveyService.GetSurveyById(key);
				if (survey == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy khảo sát" });
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

		[Authorize(Roles = Roles.SeniorRoles)]
		public IActionResult Delete([FromODataUri] int key)
		{
			try
			{
				var survey = _surveyService.GetSurveyById(key);
				if (survey == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy khảo sát" });
				}

				_surveyService.DeleteSurvey(survey);
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}
	}
}
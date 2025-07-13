using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace DUPSWebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class SurveyResultsController : ControllerBase
	{
		private readonly ISurveyResultService _surveyResultService;

		public SurveyResultsController(ISurveyResultService surveyResultService)
		{
			_surveyResultService = surveyResultService;
		}

		[HttpPost("submit")]
		[Authorize]
		public IActionResult SubmitSurvey([FromBody] SurveySubmissionRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(new SurveySubmissionResponse
					{
						Success = false,
						Message = "Dữ liệu không hợp lệ"
					});
				}

				var result = _surveyResultService.SubmitSurvey(request);

				if (result.Success)
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new SurveySubmissionResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi gửi khảo sát: " + ex.Message
				});
			}
		}

		[HttpGet("user/{userId}")]
		[Authorize]
		public IActionResult GetUserSurveyHistory(int userId)
		{
			try
			{
				var result = _surveyResultService.GetUserSurveyHistory(userId);

				if (result.Success)
				{
					return Ok(result);
				}
				else
				{
					return BadRequest(result);
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UserSurveyHistoryResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi lấy lịch sử khảo sát: " + ex.Message,
					Data = new List<UserSurveyHistoryItem>()
				});
			}
		}

		[HttpGet("check/{userId}/{surveyId}")]
		[Authorize]
		public IActionResult CheckUserSurvey(int userId, int surveyId)
		{
			try
			{
				var hasTaken = _surveyResultService.HasUserTakenSurvey(userId, surveyId);

				return Ok(new
				{
					success = true,
					message = "Kiểm tra thành công",
					data = new { hasTaken = hasTaken }
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					success = false,
					message = "Đã xảy ra lỗi khi kiểm tra: " + ex.Message
				});
			}
		}
	}
}

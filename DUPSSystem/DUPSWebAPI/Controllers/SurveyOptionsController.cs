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
	public class SurveyOptionsController : ODataController
	{
		private readonly ISurveyOptionService _optionService;

		public SurveyOptionsController(ISurveyOptionService optionService)
		{
			_optionService = optionService;
		}

		[EnableQuery]
		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Get()
		{
			try
			{
				var options = _optionService.GetOptionsByQuestionId(0).AsQueryable(); // Default empty
				return Ok(options);
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
				var option = _optionService.GetOptionById(key);
				if (option == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy tùy chọn" });
				}
				return Ok(option);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Post([FromBody] SurveyOption option)
		{
			try
			{
				if (!User.CanManageSurveys())
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền tạo tùy chọn khảo sát" });
				}

				if (!ModelState.IsValid)
				{
					return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
				}

				_optionService.SaveOption(option);
				return Created(option);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.ManagementRoles)]
		public IActionResult Patch([FromODataUri] int key, [FromBody] Delta<SurveyOption> delta)
		{
			try
			{
				if (!User.CanManageSurveys())
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền sửa tùy chọn khảo sát" });
				}

				var option = _optionService.GetOptionById(key);
				if (option == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy tùy chọn" });
				}

				delta.Patch(option);
				_optionService.UpdateOption(option);

				return Updated(option);
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
				var option = _optionService.GetOptionById(key);
				if (option == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy tùy chọn" });
				}

				_optionService.DeleteOption(option);
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}
	}
}

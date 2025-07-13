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
	public class SurveyOptionsController : ODataController
	{
		private readonly ISurveyOptionService _optionService;

		public SurveyOptionsController(ISurveyOptionService optionService)
		{
			_optionService = optionService;
		}

		[EnableQuery]
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

		[Authorize(Roles = "Admin,Manager,Staff")]
		public IActionResult Post([FromBody] SurveyOption option)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				_optionService.SaveOption(option);
				return Created(option);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = "Admin,Manager,Staff")]
		public IActionResult Patch([FromODataUri] int key, [FromBody] Delta<SurveyOption> delta)
		{
			try
			{
				var option = _optionService.GetOptionById(key);
				if (option == null)
				{
					return NotFound();
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

		[Authorize(Roles = "Admin,Manager")]
		public IActionResult Delete([FromODataUri] int key)
		{
			try
			{
				var option = _optionService.GetOptionById(key);
				if (option == null)
				{
					return NotFound();
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

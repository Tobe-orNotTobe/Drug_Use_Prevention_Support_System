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
	public class ConsultantsController : ODataController
	{
		private readonly IConsultantService _consultantService;

		public ConsultantsController(IConsultantService consultantService)
		{
			_consultantService = consultantService;
		}

		// GET: odata/Consultants
		[EnableQuery(PageSize = 20)]
		public IActionResult Get()
		{
			try
			{
				var consultants = _consultantService.GetConsultants().AsQueryable();
				return Ok(consultants);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		// GET: odata/Consultants(5)
		[EnableQuery]
		public IActionResult Get([FromODataUri] int key)
		{
			try
			{
				var consultant = _consultantService.GetConsultant(key);
				if (consultant == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy tư vấn viên" });
				}
				return Ok(consultant);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		// POST: odata/Consultants
		[Authorize(Roles = "Admin,Manager")]
		public IActionResult Post([FromBody] Consultant consultant)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				_consultantService.SaveConsultant(consultant);
				return Created(consultant);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		// PATCH: odata/Consultants(5)
		[Authorize(Roles = "Admin,Manager")]
		public IActionResult Patch([FromODataUri] int key, [FromBody] Delta<Consultant> delta)
		{
			try
			{
				var consultant = _consultantService.GetConsultant(key);
				if (consultant == null)
				{
					return NotFound();
				}

				delta.Patch(consultant);
				_consultantService.UpdateConsultant(consultant);

				return Updated(consultant);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		// PUT: odata/Consultants(5)
		[Authorize(Roles = "Admin,Manager")]
		public IActionResult Put([FromODataUri] int key, [FromBody] Consultant consultant)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				if (key != consultant.ConsultantId)
				{
					return BadRequest("Key mismatch");
				}

				var existingConsultant = _consultantService.GetConsultant(key);
				if (existingConsultant == null)
				{
					return NotFound();
				}

				_consultantService.UpdateConsultant(consultant);
				return Updated(consultant);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		// DELETE: odata/Consultants(5)
		[Authorize(Roles = "Admin,Manager")]
		public IActionResult Delete([FromODataUri] int key)
		{
			try
			{
				var consultant = _consultantService.GetConsultant(key);
				if (consultant == null)
				{
					return NotFound();
				}

				_consultantService.DeleteConsultant(consultant);
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		// GET: odata/Consultants/Available
		[HttpGet("odata/Consultants/Available")]
		[EnableQuery]
		public IActionResult GetAvailable()
		{
			try
			{
				var response = _consultantService.GetAvailableConsultants();
				if (response.Success)
				{
					return Ok(response.Data.AsQueryable());
				}
				else
				{
					return BadRequest(response);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		// GET: odata/Consultants/Search
		[HttpGet("odata/Consultants/Search")]
		[EnableQuery]
		public IActionResult Search([FromQuery] string term = "")
		{
			try
			{
				var response = _consultantService.SearchConsultants(term);
				if (response.Success)
				{
					return Ok(response.Data.AsQueryable());
				}
				else
				{
					return BadRequest(response);
				}
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}
	}
}
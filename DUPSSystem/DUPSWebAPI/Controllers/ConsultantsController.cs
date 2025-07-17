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
	public class ConsultantsController : ODataController
	{
		private readonly IConsultantService _consultantService;

		public ConsultantsController(IConsultantService consultantService)
		{
			_consultantService = consultantService;
		}

		// GET: odata/Consultants
		[EnableQuery(PageSize = 20)]
		[AllowAnonymous]
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
		[AllowAnonymous]
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
		[Authorize(Roles = Roles.SeniorRoles)]
		public IActionResult Post([FromBody] Consultant consultant)
		{
			try
			{
				if (!User.CanManageConsultants())
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền tạo tư vấn viên" });
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
		[Authorize(Roles = Roles.ConsultantRoles)]
		public IActionResult Patch([FromODataUri] int key, [FromBody] Delta<Consultant> delta)
		{
			try
			{
				var consultant = _consultantService.GetConsultant(key);
				if (consultant == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy tư vấn viên" });
				}

				var currentUserId = User.GetUserId();

				// Chỉ Manager+ hoặc consultant chính mình mới sửa được
				if (!User.CanManageConsultants() && consultant.UserId != currentUserId)
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền sửa thông tin tư vấn viên này" });
				}

				// Validate delta trước khi patch
				if (!TryValidateModel(delta))
				{
					return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
				}

				delta.Patch(consultant);

				// Validate model sau khi patch
				if (!TryValidateModel(consultant))
				{
					return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
				}

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
		[Authorize(Roles = Roles.AdminOnly)]
		public IActionResult Delete([FromODataUri] int key)
		{
			try
			{
				var consultant = _consultantService.GetConsultant(key);
				if (consultant == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy tư vấn viên" });
				}

				_consultantService.DeleteConsultant(consultant);
				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}
	}
}
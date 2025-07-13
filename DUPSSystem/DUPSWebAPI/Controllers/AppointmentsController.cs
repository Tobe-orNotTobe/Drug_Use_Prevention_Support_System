using BusinessObjects;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Services;
using Services.Interfaces;

namespace DUPSWebAPI.Controllers
{
	public class AppointmentsController : ODataController
	{
		private readonly IAppointmentService _appointmentService;

		public AppointmentsController(IAppointmentService appointmentService)
		{
			_appointmentService = appointmentService;
		}

		// GET: odata/Appointments
		[EnableQuery(PageSize = 20)]
		[Authorize]
		public IActionResult Get()
		{
			try
			{
				var appointments = _appointmentService.GetAppointments().AsQueryable();
				return Ok(appointments);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[HttpPost("odata/Appointments")]
		[Authorize]
		public IActionResult Post([FromBody] AppointmentCreateRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
				}

				var response = _appointmentService.CreateAppointment(request);
				if (response.Success)
				{
					return Ok(response);
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

		[HttpGet("odata/Appointments/user/{userId}")]
		[Authorize]
		public IActionResult GetUserAppointments([FromRoute] int userId)
		{
			try
			{
				var response = _appointmentService.GetUserAppointments(userId);
				if (response.Success)
				{
					return Ok(response);
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

		// GET: odata/Appointments/consultant/{consultantId}
		[HttpGet("odata/Appointments/consultant/{consultantId}")]
		[EnableQuery]
		[Authorize(Roles = "Admin,Manager,Consultant")]
		public IActionResult GetConsultantAppointments([FromRoute] int consultantId)
		{
			try
			{
				var response = _appointmentService.GetConsultantAppointments(consultantId);
				if (response.Success)
				{
					return Ok(response);
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

		// GET: odata/Appointments/status/{status}
		[HttpGet("odata/Appointments/status/{status}")]
		[EnableQuery]
		[Authorize(Roles = "Admin,Manager,Consultant")]
		public IActionResult GetAppointmentsByStatus([FromRoute] string status)
		{
			try
			{
				var response = _appointmentService.GetAppointmentsByStatus(status);
				if (response.Success)
				{
					return Ok(response);
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

		// POST: odata/Appointments/{id}/cancel
		[HttpPost("odata/Appointments({id})/cancel")]
		[Authorize]
		public IActionResult CancelAppointment([FromRoute] int id, [FromBody] CancelAppointmentRequest request)
		{
			try
			{
				var response = _appointmentService.CancelAppointment(id, request.UserId);
				if (response.Success)
				{
					return Ok(response);
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

		// POST: odata/Appointments/{id}/confirm
		[HttpPost("odata/Appointments({id})/confirm")]
		[Authorize(Roles = "Admin,Manager,Consultant")]
		public IActionResult ConfirmAppointment([FromRoute] int id)
		{
			try
			{
				var response = _appointmentService.ConfirmAppointment(id);
				if (response.Success)
				{
					return Ok(response);
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

		// POST: odata/Appointments/{id}/complete
		[HttpPost("odata/Appointments({id})/complete")]
		[Authorize(Roles = "Admin,Manager,Consultant")]
		public IActionResult CompleteAppointment([FromRoute] int id)
		{
			try
			{
				var response = _appointmentService.CompleteAppointment(id);
				if (response.Success)
				{
					return Ok(response);
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

	// Helper DTOs for specific actions
	public class CancelAppointmentRequest
	{
		public int UserId { get; set; }
	}
}

using BusinessObjects;
using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Extensions;
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
		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Get()
		{
			try
			{
				var appointments = _appointmentService.GetAppointments().AsQueryable();
				var currentUserId = User.GetUserId();

				if (User.CanViewAllAppointments())
				{
					return Ok(appointments);
				}
				else if (User.IsConsultant())
				{
					var consultantId = _appointmentService.GetConsultantIdByUserId(currentUserId);

					if (consultantId == null)
					{
						return BadRequest(new { success = false, message = "Không tìm thấy thông tin tư vấn viên cho người dùng này" });
					}

					var filtered = appointments.Where(a => a.ConsultantId == consultantId.Value);
					return Ok(filtered);
				}
				else
				{
					// Member và các role khác chỉ xem appointments của chính họ (UserId)
					var filtered = appointments.Where(a => a.UserId == currentUserId);
					return Ok(filtered);
				}
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
				var appointment = _appointmentService.GetAppointments().FirstOrDefault(a => a.AppointmentId == key);
				if (appointment == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy lịch hẹn" });
				}

				// Kiểm tra quyền xem
				if (!User.CanViewAppointment(appointment.UserId, appointment.ConsultantId))
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền xem lịch hẹn này" });
				}

				return Ok(appointment);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Post([FromBody] AppointmentCreateRequest request)
		{
			try
			{
				if (!User.CanBookAppointments())
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền đặt lịch hẹn" });
				}

				if (!ModelState.IsValid)
				{
					return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
				}

				// Auto-assign UserId từ JWT
				request.UserId = User.GetUserId();

				var response = _appointmentService.CreateAppointment(request);
				return response.Success ? Created(response) : BadRequest(response);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}

		[Authorize(Roles = Roles.AuthenticatedRoles)]
		public IActionResult Patch([FromODataUri] int key, [FromBody] Delta<Appointment> delta)
		{
			try
			{
				var appointment = _appointmentService.GetAppointment(key);
				if (appointment == null)
				{
					return NotFound(new { success = false, message = "Không tìm thấy lịch hẹn" });
				}

				var currentUserId = User.GetUserId();

				// Kiểm tra quyền sửa
				bool canUpdate = false;

				if (User.CanManageAppointments())
				{
					canUpdate = true; // Admin/Manager/Staff
				}
				else if (appointment.UserId == currentUserId)
				{
					canUpdate = true; // Member sửa appointment của mình
				}
				else if (User.IsConsultant())
				{
					// Consultant sửa appointment được đặt với họ
					var consultantId = _appointmentService.GetConsultantIdByUserId(currentUserId);
					canUpdate = (consultantId != null && appointment.ConsultantId == consultantId.Value);
				}

				if (!canUpdate)
				{
					return StatusCode(403, new { success = false, message = "Bạn không có quyền sửa lịch hẹn này" });
				}

				// Apply changes
				delta.Patch(appointment);
				_appointmentService.UpdateAppointment(appointment);

				return Updated(appointment);
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
		}
	}
}

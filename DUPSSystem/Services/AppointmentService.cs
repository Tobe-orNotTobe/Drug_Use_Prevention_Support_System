using BusinessObjects;
using BusinessObjects.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
	public class AppointmentService : IAppointmentService
	{
		private readonly IAppointmentRepository _appointmentRepository;
		private readonly IConsultantRepository _consultantRepository;
		private readonly IUserRepository _userRepository;

		public AppointmentService(
			IAppointmentRepository appointmentRepository,
			IConsultantRepository consultantRepository,
			IUserRepository userRepository)
		{
			_appointmentRepository = appointmentRepository;
			_consultantRepository = consultantRepository;
			_userRepository = userRepository;
		}

		public AppointmentListResponse GetAllAppointments()
		{
			try
			{
				var appointments = _appointmentRepository.GetAllAppointments();
				var appointmentDetails = appointments.Select(MapToAppointmentDetail).ToList();

				return new AppointmentListResponse
				{
					Success = true,
					Message = "Lấy danh sách lịch hẹn thành công",
					Data = appointmentDetails
				};
			}
			catch (Exception ex)
			{
				return new AppointmentListResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi lấy danh sách lịch hẹn: " + ex.Message,
					Data = new List<AppointmentDetail>()
				};
			}
		}

		public AppointmentResponse GetAppointmentById(int appointmentId)
		{
			try
			{
				var appointment = _appointmentRepository.GetAppointmentById(appointmentId);
				if (appointment == null)
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Không tìm thấy lịch hẹn"
					};
				}

				return new AppointmentResponse
				{
					Success = true,
					Message = "Lấy thông tin lịch hẹn thành công",
					Data = MapToAppointmentDetail(appointment)
				};
			}
			catch (Exception ex)
			{
				return new AppointmentResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi lấy thông tin lịch hẹn: " + ex.Message
				};
			}
		}

		public AppointmentListResponse GetUserAppointments(int userId)
		{
			try
			{
				var appointments = _appointmentRepository.GetUserAppointments(userId);
				var appointmentDetails = appointments.Select(MapToAppointmentDetail).ToList();

				return new AppointmentListResponse
				{
					Success = true,
					Message = "Lấy lịch hẹn của người dùng thành công",
					Data = appointmentDetails
				};
			}
			catch (Exception ex)
			{
				return new AppointmentListResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi lấy lịch hẹn: " + ex.Message,
					Data = new List<AppointmentDetail>()
				};
			}
		}

		public AppointmentListResponse GetConsultantAppointments(int consultantId)
		{
			try
			{
				var appointments = _appointmentRepository.GetConsultantAppointments(consultantId);
				var appointmentDetails = appointments.Select(MapToAppointmentDetail).ToList();

				return new AppointmentListResponse
				{
					Success = true,
					Message = "Lấy lịch hẹn của tư vấn viên thành công",
					Data = appointmentDetails
				};
			}
			catch (Exception ex)
			{
				return new AppointmentListResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi lấy lịch hẹn: " + ex.Message,
					Data = new List<AppointmentDetail>()
				};
			}
		}

		public AppointmentListResponse GetAppointmentsByStatus(string status)
		{
			try
			{
				var appointments = _appointmentRepository.GetAppointmentsByStatus(status);
				var appointmentDetails = appointments.Select(MapToAppointmentDetail).ToList();

				return new AppointmentListResponse
				{
					Success = true,
					Message = $"Lấy lịch hẹn với trạng thái {status} thành công",
					Data = appointmentDetails
				};
			}
			catch (Exception ex)
			{
				return new AppointmentListResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi lấy lịch hẹn: " + ex.Message,
					Data = new List<AppointmentDetail>()
				};
			}
		}

		public AppointmentResponse CreateAppointment(AppointmentCreateRequest request)
		{
			try
			{
				// Validate user exists
				var user = _userRepository.GetAccountById(request.UserId);
				if (user == null)
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Người dùng không tồn tại"
					};
				}

				// Validate consultant exists
				var consultant = _consultantRepository.GetConsultantById(request.ConsultantId);
				if (consultant == null)
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Tư vấn viên không tồn tại"
					};
				}

				// Check if consultant is active
				if (!consultant.User.IsActive)
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Tư vấn viên hiện không khả dụng"
					};
				}

				// Validate appointment date
				if (request.AppointmentDate <= DateTime.Now)
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Ngày hẹn phải sau thời gian hiện tại"
					};
				}

				// Check for conflicting appointments
				if (_appointmentRepository.HasConflictingAppointment(request.ConsultantId, request.AppointmentDate))
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Tư vấn viên đã có lịch hẹn vào thời gian này"
					};
				}

				var appointment = new Appointment
				{
					UserId = request.UserId,
					ConsultantId = request.ConsultantId,
					AppointmentDate = request.AppointmentDate,
					DurationMinutes = request.DurationMinutes,
					Notes = request.Notes,
					Status = "Pending"
				};

				_appointmentRepository.SaveAppointment(appointment);

				// Reload with related data
				var savedAppointment = _appointmentRepository.GetAppointmentById(appointment.AppointmentId);

				return new AppointmentResponse
				{
					Success = true,
					Message = "Đặt lịch hẹn thành công",
					AppointmentId = appointment.AppointmentId,
					Data = MapToAppointmentDetail(savedAppointment!)
				};
			}
			catch (Exception ex)
			{
				return new AppointmentResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi đặt lịch hẹn: " + ex.Message
				};
			}
		}

		public AppointmentResponse UpdateAppointment(AppointmentUpdateRequest request)
		{
			try
			{
				var appointment = _appointmentRepository.GetAppointmentById(request.AppointmentId);
				if (appointment == null)
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Không tìm thấy lịch hẹn"
					};
				}

				// Check if appointment can be updated
				if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Không thể cập nhật lịch hẹn đã hoàn thành hoặc đã hủy"
					};
				}

				// Validate new appointment date if changed
				if (request.AppointmentDate != appointment.AppointmentDate)
				{
					if (request.AppointmentDate <= DateTime.Now)
					{
						return new AppointmentResponse
						{
							Success = false,
							Message = "Ngày hẹn phải sau thời gian hiện tại"
						};
					}

					// Check for conflicting appointments
					if (_appointmentRepository.HasConflictingAppointment(
						appointment.ConsultantId,
						request.AppointmentDate,
						appointment.AppointmentId))
					{
						return new AppointmentResponse
						{
							Success = false,
							Message = "Tư vấn viên đã có lịch hẹn vào thời gian này"
						};
					}
				}

				appointment.AppointmentDate = request.AppointmentDate;
				appointment.DurationMinutes = request.DurationMinutes;
				appointment.Notes = request.Notes;

				if (!string.IsNullOrEmpty(request.Status))
				{
					appointment.Status = request.Status;
				}

				_appointmentRepository.UpdateAppointment(appointment);

				return new AppointmentResponse
				{
					Success = true,
					Message = "Cập nhật lịch hẹn thành công",
					Data = MapToAppointmentDetail(appointment)
				};
			}
			catch (Exception ex)
			{
				return new AppointmentResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi cập nhật lịch hẹn: " + ex.Message
				};
			}
		}

		public AppointmentResponse CancelAppointment(int appointmentId, int userId)
		{
			try
			{
				var appointment = _appointmentRepository.GetAppointmentById(appointmentId);
				if (appointment == null)
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Không tìm thấy lịch hẹn"
					};
				}

				// Check if user owns this appointment
				if (appointment.UserId != userId)
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Bạn không có quyền hủy lịch hẹn này"
					};
				}

				// Check if appointment can be cancelled
				if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Không thể hủy lịch hẹn đã hoàn thành hoặc đã hủy"
					};
				}

				appointment.Status = "Cancelled";
				_appointmentRepository.UpdateAppointment(appointment);

				return new AppointmentResponse
				{
					Success = true,
					Message = "Hủy lịch hẹn thành công",
					Data = MapToAppointmentDetail(appointment)
				};
			}
			catch (Exception ex)
			{
				return new AppointmentResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi hủy lịch hẹn: " + ex.Message
				};
			}
		}

		public AppointmentResponse ConfirmAppointment(int appointmentId)
		{
			try
			{
				var appointment = _appointmentRepository.GetAppointmentById(appointmentId);
				if (appointment == null)
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Không tìm thấy lịch hẹn"
					};
				}

				if (appointment.Status != "Pending")
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Chỉ có thể xác nhận lịch hẹn đang chờ"
					};
				}

				appointment.Status = "Confirmed";
				_appointmentRepository.UpdateAppointment(appointment);

				return new AppointmentResponse
				{
					Success = true,
					Message = "Xác nhận lịch hẹn thành công",
					Data = MapToAppointmentDetail(appointment)
				};
			}
			catch (Exception ex)
			{
				return new AppointmentResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi xác nhận lịch hẹn: " + ex.Message
				};
			}
		}

		public AppointmentResponse CompleteAppointment(int appointmentId)
		{
			try
			{
				var appointment = _appointmentRepository.GetAppointmentById(appointmentId);
				if (appointment == null)
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Không tìm thấy lịch hẹn"
					};
				}

				if (appointment.Status != "Confirmed")
				{
					return new AppointmentResponse
					{
						Success = false,
						Message = "Chỉ có thể hoàn thành lịch hẹn đã xác nhận"
					};
				}

				appointment.Status = "Completed";
				_appointmentRepository.UpdateAppointment(appointment);

				return new AppointmentResponse
				{
					Success = true,
					Message = "Hoàn thành lịch hẹn thành công",
					Data = MapToAppointmentDetail(appointment)
				};
			}
			catch (Exception ex)
			{
				return new AppointmentResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi hoàn thành lịch hẹn: " + ex.Message
				};
			}
		}

		// Direct entity methods for OData controllers
		public List<Appointment> GetAppointments() => _appointmentRepository.GetAllAppointments();

		public Appointment? GetAppointment(int appointmentId) => _appointmentRepository.GetAppointmentById(appointmentId);

		public void SaveAppointment(Appointment appointment)
		{
			if (appointment == null)
				throw new ArgumentNullException(nameof(appointment));

			_appointmentRepository.SaveAppointment(appointment);
		}

		public void UpdateAppointment(Appointment appointment)
		{
			if (appointment == null)
				throw new ArgumentNullException(nameof(appointment));

			_appointmentRepository.UpdateAppointment(appointment);
		}

		public void DeleteAppointment(Appointment appointment)
		{
			if (appointment == null)
				throw new ArgumentNullException(nameof(appointment));

			_appointmentRepository.DeleteAppointment(appointment);
		}

		private AppointmentDetail MapToAppointmentDetail(Appointment appointment)
		{
			return new AppointmentDetail
			{
				AppointmentId = appointment.AppointmentId,
				UserId = appointment.UserId,
				UserName = appointment.User?.FullName ?? "Không xác định",
				UserEmail = appointment.User?.Email ?? "Không xác định",
				ConsultantId = appointment.ConsultantId,
				ConsultantName = appointment.Consultant?.User?.FullName ?? "Không xác định",
				ConsultantExpertise = appointment.Consultant?.Expertise ?? "Không xác định",
				AppointmentDate = appointment.AppointmentDate,
				DurationMinutes = appointment.DurationMinutes,
				Status = appointment.Status,
				Notes = appointment.Notes,
				CreatedAt = appointment.CreatedAt
			};
		}
	}
}
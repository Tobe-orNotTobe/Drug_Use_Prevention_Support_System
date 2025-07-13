using BusinessObjects;
using BusinessObjects.DTOs;

namespace Services.Interfaces
{
	public interface IAppointmentService
	{
		AppointmentListResponse GetAllAppointments();
		AppointmentResponse GetAppointmentById(int appointmentId);
		AppointmentListResponse GetUserAppointments(int userId);
		AppointmentListResponse GetConsultantAppointments(int consultantId);
		AppointmentListResponse GetAppointmentsByStatus(string status);
		AppointmentResponse CreateAppointment(AppointmentCreateRequest request);
		AppointmentResponse UpdateAppointment(AppointmentUpdateRequest request);
		AppointmentResponse CancelAppointment(int appointmentId, int userId);
		AppointmentResponse ConfirmAppointment(int appointmentId);
		AppointmentResponse CompleteAppointment(int appointmentId);

		// Direct entity methods for OData controllers
		List<Appointment> GetAppointments();
		Appointment? GetAppointment(int appointmentId);
		void SaveAppointment(Appointment appointment);
		void UpdateAppointment(Appointment appointment);
		void DeleteAppointment(Appointment appointment);
	}
}
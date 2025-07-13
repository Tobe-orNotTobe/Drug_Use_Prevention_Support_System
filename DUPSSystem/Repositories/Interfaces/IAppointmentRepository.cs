using BusinessObjects;

namespace Repositories.Interfaces
{
	public interface IAppointmentRepository
	{
		List<Appointment> GetAllAppointments();
		Appointment? GetAppointmentById(int appointmentId);
		List<Appointment> GetUserAppointments(int userId);
		List<Appointment> GetConsultantAppointments(int consultantId);
		List<Appointment> GetAppointmentsByStatus(string status);
		bool HasConflictingAppointment(int consultantId, DateTime appointmentDate, int? excludeAppointmentId = null);
		void SaveAppointment(Appointment appointment);
		void UpdateAppointment(Appointment appointment);
		void DeleteAppointment(Appointment appointment);
	}
}

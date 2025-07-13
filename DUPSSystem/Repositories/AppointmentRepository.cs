using BusinessObjects;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class AppointmentRepository : IAppointmentRepository
	{
		public List<Appointment> GetAllAppointments() => AppointmentDAO.GetAll();

		public Appointment? GetAppointmentById(int appointmentId) => AppointmentDAO.GetById(appointmentId);

		public List<Appointment> GetUserAppointments(int userId) => AppointmentDAO.GetByUserId(userId);

		public List<Appointment> GetConsultantAppointments(int consultantId) => AppointmentDAO.GetByConsultantId(consultantId);

		public List<Appointment> GetAppointmentsByStatus(string status) => AppointmentDAO.GetByStatus(status);

		public bool HasConflictingAppointment(int consultantId, DateTime appointmentDate, int? excludeAppointmentId = null)
			=> AppointmentDAO.HasConflictingAppointment(consultantId, appointmentDate, excludeAppointmentId);

		public void SaveAppointment(Appointment appointment) => AppointmentDAO.Save(appointment);

		public void UpdateAppointment(Appointment appointment) => AppointmentDAO.Update(appointment);

		public void DeleteAppointment(Appointment appointment) => AppointmentDAO.Delete(appointment);
	}
}

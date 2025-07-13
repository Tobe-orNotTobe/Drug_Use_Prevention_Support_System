using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
	public class AppointmentDAO
	{
		public static Appointment GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.Appointments
				.Include(a => a.User)
				.Include(a => a.Consultant)
					.ThenInclude(c => c.User)
				.FirstOrDefault(c => c.AppointmentId.Equals(id));
		}

		public static List<Appointment> GetAll()
		{
			var list = new List<Appointment>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Appointments
					.Include(a => a.User)
					.Include(a => a.Consultant)
						.ThenInclude(c => c.User)
					.OrderByDescending(a => a.CreatedAt)
					.ToList();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error in AppointmentDAO.GetAll: {e.Message}");
			}
			return list;
		}

		public static List<Appointment> GetByUserId(int userId)
		{
			var list = new List<Appointment>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Appointments
					.Include(a => a.User)
					.Include(a => a.Consultant)
						.ThenInclude(c => c.User)
					.Where(a => a.UserId == userId)
					.OrderByDescending(a => a.AppointmentDate)
					.ToList();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error in AppointmentDAO.GetByUserId: {e.Message}");
			}
			return list;
		}

		public static List<Appointment> GetByConsultantId(int consultantId)
		{
			var list = new List<Appointment>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Appointments
					.Include(a => a.User)
					.Include(a => a.Consultant)
						.ThenInclude(c => c.User)
					.Where(a => a.ConsultantId == consultantId)
					.OrderByDescending(a => a.AppointmentDate)
					.ToList();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error in AppointmentDAO.GetByConsultantId: {e.Message}");
			}
			return list;
		}

		public static List<Appointment> GetByStatus(string status)
		{
			var list = new List<Appointment>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Appointments
					.Include(a => a.User)
					.Include(a => a.Consultant)
						.ThenInclude(c => c.User)
					.Where(a => a.Status == status)
					.OrderByDescending(a => a.AppointmentDate)
					.ToList();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error in AppointmentDAO.GetByStatus: {e.Message}");
			}
			return list;
		}

		public static bool HasConflictingAppointment(int consultantId, DateTime appointmentDate, int? excludeAppointmentId = null)
		{
			try
			{
				using var db = new DrugPreventionDbContext();
				var query = db.Appointments
					.Where(a => a.ConsultantId == consultantId &&
							   a.AppointmentDate == appointmentDate &&
							   a.Status != "Cancelled" &&
							   a.Status != "Completed");

				if (excludeAppointmentId.HasValue)
				{
					query = query.Where(a => a.AppointmentId != excludeAppointmentId.Value);
				}

				return query.Any();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error in AppointmentDAO.HasConflictingAppointment: {e.Message}");
				return false;
			}
		}

		public static void Save(Appointment s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				s.CreatedAt = DateTime.UtcNow;
				s.Status = "Pending";
				context.Appointments.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(Appointment s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.Entry(s).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Delete(Appointment s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.Appointments.SingleOrDefault(c => c.AppointmentId == s.AppointmentId);
				if (s1 != null)
				{
					context.Appointments.Remove(s1);
					context.SaveChanges();
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}

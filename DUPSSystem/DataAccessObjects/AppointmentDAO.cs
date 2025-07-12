using BusinessObjects;

namespace DataAccessObjects
{
	public class AppointmentDAO
	{
		public static Appointment GetById (int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.Appointments.FirstOrDefault(c => c.AppointmentId.Equals(id));
		}

		public static List<Appointment> GetAll()
		{
			var list = new List<Appointment>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Appointments.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(Appointment s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
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
				context.Appointments.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}

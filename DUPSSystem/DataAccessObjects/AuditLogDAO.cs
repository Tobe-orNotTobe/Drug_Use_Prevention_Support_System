using BusinessObjects;

namespace DataAccessObjects
{
	public class AuditLogDAO
	{
		public static AuditLog GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.AuditLogs.FirstOrDefault(c => c.LogId.Equals(id));
		}

		public static List<AuditLog> GetAll()
		{
			var list = new List<AuditLog>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.AuditLogs.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(AuditLog s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.AuditLogs.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(AuditLog s)
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

		public static void Delete(AuditLog s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.AuditLogs.SingleOrDefault(c => c.LogId == s.LogId);
				context.AuditLogs.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}

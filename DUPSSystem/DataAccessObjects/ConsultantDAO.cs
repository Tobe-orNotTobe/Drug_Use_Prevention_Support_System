using BusinessObjects;

namespace DataAccessObjects
{
	public class ConsultantDAO
	{
		public static Consultant GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.Consultants.FirstOrDefault(c => c.ConsultantId.Equals(id));
		}

		public static List<Consultant> GetAll()
		{
			var list = new List<Consultant>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Consultants.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(Consultant s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.Consultants.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(Consultant s)
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

		public static void Delete(Consultant s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.Consultants.SingleOrDefault(c => c.ConsultantId == s.ConsultantId);
				context.Consultants.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}

using BusinessObjects;

namespace DataAccessObjects
{
	public class UserProgramDAO
	{
		public static UserProgram GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.UserPrograms.FirstOrDefault(c => c.UserProgramId.Equals(id));
		}

		public static List<UserProgram> GetAll()
		{
			var list = new List<UserProgram>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.UserPrograms.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(UserProgram s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.UserPrograms.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(UserProgram s)
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

		public static void Delete(UserProgram s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.UserPrograms.SingleOrDefault(c => c.UserProgramId == s.UserProgramId);
				context.UserPrograms.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}

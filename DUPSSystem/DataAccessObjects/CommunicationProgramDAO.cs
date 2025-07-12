using BusinessObjects;

namespace DataAccessObjects
{
	public class CommunicationProgramDAO
	{
		public static CommunicationProgram GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.CommunicationPrograms.FirstOrDefault(c => c.ProgramId.Equals(id));
		}

		public static List<CommunicationProgram> GetAll()
		{
			var list = new List<CommunicationProgram>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.CommunicationPrograms.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(CommunicationProgram s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.CommunicationPrograms.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(CommunicationProgram s)
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

		public static void Delete(CommunicationProgram s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.CommunicationPrograms.SingleOrDefault(c => c.ProgramId == s.ProgramId);
				context.CommunicationPrograms.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}

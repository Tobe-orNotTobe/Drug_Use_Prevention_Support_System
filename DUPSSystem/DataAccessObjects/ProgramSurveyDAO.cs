using BusinessObjects;

namespace DataAccessObjects
{
	public class ProgramSurveyDAO
	{
		public static ProgramSurvey GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.ProgramSurveys.FirstOrDefault(c => c.ProgramSurveyId.Equals(id));
		}

		public static List<ProgramSurvey> GetAll()
		{
			var list = new List<ProgramSurvey>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.ProgramSurveys.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(ProgramSurvey s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.ProgramSurveys.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(ProgramSurvey s)
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

		public static void Delete(ProgramSurvey s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.ProgramSurveys.SingleOrDefault(c => c.ProgramSurveyId == s.ProgramSurveyId);
				context.ProgramSurveys.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}

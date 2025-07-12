using BusinessObjects;

namespace DataAccessObjects
{
	public class SurveyDAO
	{
		public static Survey GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.Surveys.FirstOrDefault(c => c.SurveyId.Equals(id));
		}

		public static List<Survey> GetAll()
		{
			var list = new List<Survey>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Surveys.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(Survey s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.Surveys.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(Survey s)
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

		public static void Delete(Survey s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.Surveys.SingleOrDefault(c => c.SurveyId == s.SurveyId);
				context.Surveys.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}

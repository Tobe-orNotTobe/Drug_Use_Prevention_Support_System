using BusinessObjects;

namespace DataAccessObjects
{
	public class UserSurveyResultDAO
	{
		public static UserSurveyResult GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.UserSurveyResults.FirstOrDefault(c => c.ResultId.Equals(id));
		}

		public static List<UserSurveyResult> GetAll()
		{
			var list = new List<UserSurveyResult>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.UserSurveyResults.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(UserSurveyResult s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.UserSurveyResults.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(UserSurveyResult s)
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

		public static void Delete(UserSurveyResult s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.UserSurveyResults.SingleOrDefault(c => c.ResultId == s.ResultId);
				context.UserSurveyResults.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}

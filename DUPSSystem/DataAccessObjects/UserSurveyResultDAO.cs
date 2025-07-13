using BusinessObjects;
using Microsoft.EntityFrameworkCore;

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

		public static List<UserSurveyResult> GetByUserId(int userId)
		{
			var list = new List<UserSurveyResult>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.UserSurveyResults
					.Include(r => r.Survey)
					.Where(r => r.UserId == userId)
					.OrderByDescending(r => r.TakenAt)
					.ToList();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error getting user survey results: {e.Message}");
			}
			return list;
		}

		public static UserSurveyResult? GetByUserAndSurvey(int userId, int surveyId)
		{
			try
			{
				using var db = new DrugPreventionDbContext();
				return db.UserSurveyResults
					.Include(r => r.Survey)
					.FirstOrDefault(r => r.UserId == userId && r.SurveyId == surveyId);
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error getting user survey result: {e.Message}");
				return null;
			}
		}
	}
}

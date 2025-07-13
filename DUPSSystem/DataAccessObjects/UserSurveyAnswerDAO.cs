using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
	public class UserSurveyAnswerDAO
	{
		public static UserSurveyAnswer GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.UserSurveyAnswers.FirstOrDefault(c => c.AnswerId.Equals(id));
		}

		public static List<UserSurveyAnswer> GetAll()
		{
			var list = new List<UserSurveyAnswer>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.UserSurveyAnswers.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(UserSurveyAnswer s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.UserSurveyAnswers.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(UserSurveyAnswer s)
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

		public static void Delete(UserSurveyAnswer s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.UserSurveyAnswers.SingleOrDefault(c => c.AnswerId == s.AnswerId);
				context.UserSurveyAnswers.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static List<UserSurveyAnswer> GetByResultId(int resultId)
		{
			var list = new List<UserSurveyAnswer>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.UserSurveyAnswers
					.Include(a => a.Question)
					.Include(a => a.Option)
					.Where(a => a.ResultId == resultId)
					.ToList();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error getting answers by result ID: {e.Message}");
			}
			return list;
		}

		public static void SaveBatch(List<UserSurveyAnswer> answers)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.UserSurveyAnswers.AddRange(answers);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception($"Error saving batch answers: {e.Message}");
			}
		}

		public static void DeleteByResultId(int resultId)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var answers = context.UserSurveyAnswers.Where(a => a.ResultId == resultId);
				context.UserSurveyAnswers.RemoveRange(answers);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception($"Error deleting answers: {e.Message}");
			}
		}
	}
}

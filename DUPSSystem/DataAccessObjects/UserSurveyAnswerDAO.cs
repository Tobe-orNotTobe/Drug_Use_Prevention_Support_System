using BusinessObjects;

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
	}
}

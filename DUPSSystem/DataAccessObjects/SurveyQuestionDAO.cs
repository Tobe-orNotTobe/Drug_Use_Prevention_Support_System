using BusinessObjects;

namespace DataAccessObjects
{
	public class SurveyQuestionDAO
	{
		public static SurveyQuestion GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.SurveyQuestions.FirstOrDefault(c => c.QuestionId.Equals(id));
		}

		public static List<SurveyQuestion> GetAll()
		{
			var list = new List<SurveyQuestion>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.SurveyQuestions.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(SurveyQuestion s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.SurveyQuestions.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(SurveyQuestion s)
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

		public static void Delete(SurveyQuestion s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.SurveyQuestions.SingleOrDefault(c => c.QuestionId == s.QuestionId);
				context.SurveyQuestions.Remove(s1);

				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}

using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
	public class SurveyOptionDAO
	{
		public static SurveyOption GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.SurveyOptions.FirstOrDefault(c => c.OptionId.Equals(id));
		}

		public static List<SurveyOption> GetByQuestionId(int questionId)
		{
			var list = new List<SurveyOption>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.SurveyOptions
					.Where(o => o.QuestionId == questionId)
					.OrderBy(o => o.OptionId)
					.ToList();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error getting options by question ID: {e.Message}");
			}
			return list;
		}

		public static List<SurveyOption> GetAll()
		{
			var list = new List<SurveyOption>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.SurveyOptions.ToList();
			}
			catch (Exception e) { }
			return list;
		}

		public static void Save(SurveyOption s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				context.SurveyOptions.Add(s);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static void Update(SurveyOption s)
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

		public static void Delete(SurveyOption s)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				var s1 = context.SurveyOptions.SingleOrDefault(c => c.OptionId == s.OptionId);
				context.SurveyOptions.Remove(s1);
				context.SaveChanges();
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}

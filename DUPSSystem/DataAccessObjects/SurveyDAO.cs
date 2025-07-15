using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
	public class SurveyDAO
	{
		public static Survey GetById(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.Surveys
				.Include(s => s.SurveyQuestions.OrderBy(q => q.QuestionId))
					.ThenInclude(q => q.SurveyOptions.OrderBy(o => o.OptionId)).FirstOrDefault(c => c.SurveyId.Equals(id));
		}

		// THÊM METHOD MỚI
		public static Survey? GetWithQuestionsAndOptions(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.Surveys
				.Include(s => s.SurveyQuestions.OrderBy(q => q.QuestionId))
					.ThenInclude(q => q.SurveyOptions.OrderBy(o => o.OptionId))
				.FirstOrDefault(s => s.SurveyId == id);
		}

		// THÊM METHOD SEARCH
		public static List<Survey> Search(string searchTerm)
		{
			var list = new List<Survey>();
			try
			{
				using var db = new DrugPreventionDbContext();
				if (string.IsNullOrWhiteSpace(searchTerm))
				{
					list = db.Surveys.OrderByDescending(s => s.CreatedAt).ToList();
				}
				else
				{
					searchTerm = searchTerm.ToLower();
					list = db.Surveys
						.Where(s => s.Name.ToLower().Contains(searchTerm) ||
								   (s.Description != null && s.Description.ToLower().Contains(searchTerm)))
						.OrderByDescending(s => s.CreatedAt)
						.ToList();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error searching surveys: {e.Message}");
			}
			return list;
		}

		public static List<Survey> GetAll()
		{
			var list = new List<Survey>();
			try
			{
				using var db = new DrugPreventionDbContext();
				list = db.Surveys.Include(s => s.SurveyQuestions.OrderBy(q => q.QuestionId))

					.ThenInclude(q => q.SurveyOptions.OrderBy(o => o.OptionId)).OrderByDescending(s => s.CreatedAt).ToList();
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

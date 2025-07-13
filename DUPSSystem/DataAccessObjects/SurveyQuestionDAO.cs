using BusinessObjects;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;

public class SurveyQuestionDAO
{
	public static SurveyQuestion GetById(int id)
	{
		using var db = new DrugPreventionDbContext();
		return db.SurveyQuestions.FirstOrDefault(c => c.QuestionId.Equals(id));
	}

	public static List<SurveyQuestion> GetBySurveyId(int surveyId)
	{
		var list = new List<SurveyQuestion>();
		try
		{
			using var db = new DrugPreventionDbContext();
			list = db.SurveyQuestions
				.Include(q => q.SurveyOptions.OrderBy(o => o.OptionId))
				.Where(q => q.SurveyId == surveyId)
				.OrderBy(q => q.QuestionId)
				.ToList();
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error getting questions by survey ID: {e.Message}");
		}
		return list;
	}

	public static List<SurveyQuestion> GetAll()
	{
		var list = new List<SurveyQuestion>();
		try
		{
			using var db = new DrugPreventionDbContext();
			list = db.SurveyQuestions
				.Include(q => q.SurveyOptions.OrderBy(o => o.OptionId))
				.ToList();
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
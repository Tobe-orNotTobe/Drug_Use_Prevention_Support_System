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
					.ThenInclude(q => q.SurveyOptions.OrderBy(o => o.OptionId))
				.FirstOrDefault(c => c.SurveyId.Equals(id));
		}

		public static Survey? GetWithQuestionsAndOptions(int id)
		{
			using var db = new DrugPreventionDbContext();
			return db.Surveys
				.Include(s => s.SurveyQuestions.OrderBy(q => q.QuestionId))
					.ThenInclude(q => q.SurveyOptions.OrderBy(o => o.OptionId))
				.FirstOrDefault(s => s.SurveyId == id);
		}

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
				list = db.Surveys
					.Include(s => s.SurveyQuestions.OrderBy(q => q.QuestionId))
						.ThenInclude(q => q.SurveyOptions.OrderBy(o => o.OptionId))
					.OrderByDescending(s => s.CreatedAt)
					.ToList();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error getting all surveys: {e.Message}");
			}
			return list;
		}

		public static void SaveWithQuestionsAndOptions(Survey survey)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				using var transaction = context.Database.BeginTransaction();

				try
				{
					// Save survey first
					context.Surveys.Add(survey);
					context.SaveChanges(); // This generates SurveyId

					// Save questions and options
					if (survey.SurveyQuestions != null && survey.SurveyQuestions.Any())
					{
						foreach (var question in survey.SurveyQuestions)
						{
							question.SurveyId = survey.SurveyId;
							context.SurveyQuestions.Add(question);
							context.SaveChanges(); // This generates QuestionId

							// Save options
							if (question.SurveyOptions != null && question.SurveyOptions.Any())
							{
								foreach (var option in question.SurveyOptions)
								{
									option.QuestionId = question.QuestionId;
									context.SurveyOptions.Add(option);
								}
								context.SaveChanges(); // Save all options for this question
							}
						}
					}

					transaction.Commit();
				}
				catch
				{
					transaction.Rollback();
					throw;
				}
			}
			catch (Exception e)
			{
				throw new Exception($"Error saving survey with questions and options: {e.Message}", e);
			}
		}

		public static void UpdateWithQuestionsAndOptions(Survey survey)
		{
			try
			{
				using var context = new DrugPreventionDbContext();
				using var transaction = context.Database.BeginTransaction();

				try
				{
					// Update survey basic info
					var existingSurvey = context.Surveys
						.Include(s => s.SurveyQuestions)
							.ThenInclude(q => q.SurveyOptions)
						.FirstOrDefault(s => s.SurveyId == survey.SurveyId);

					if (existingSurvey == null)
						throw new ArgumentException("Survey not found");

					// Update survey properties
					existingSurvey.Name = survey.Name;
					existingSurvey.Description = survey.Description;

					// Remove existing questions and options
					if (existingSurvey.SurveyQuestions != null)
					{
						foreach (var existingQuestion in existingSurvey.SurveyQuestions.ToList())
						{
							if (existingQuestion.SurveyOptions != null)
							{
								context.SurveyOptions.RemoveRange(existingQuestion.SurveyOptions);
							}
							context.SurveyQuestions.Remove(existingQuestion);
						}
					}

					context.SaveChanges();

					// Add new questions and options
					if (survey.SurveyQuestions != null && survey.SurveyQuestions.Any())
					{
						foreach (var question in survey.SurveyQuestions)
						{
							question.SurveyId = survey.SurveyId;
							question.QuestionId = 0; // Reset ID for new question

							context.SurveyQuestions.Add(question);
							context.SaveChanges(); // Generate new QuestionId

							if (question.SurveyOptions != null && question.SurveyOptions.Any())
							{
								foreach (var option in question.SurveyOptions)
								{
									option.QuestionId = question.QuestionId;
									option.OptionId = 0; // Reset ID for new option
									context.SurveyOptions.Add(option);
								}
								context.SaveChanges();
							}
						}
					}

					transaction.Commit();
				}
				catch
				{
					transaction.Rollback();
					throw;
				}
			}
			catch (Exception e)
			{
				throw new Exception($"Error updating survey with questions and options: {e.Message}", e);
			}
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
				context.Entry(s).State = EntityState.Modified;
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
				var existingSurvey = context.Surveys
					.Include(survey => survey.SurveyQuestions)
						.ThenInclude(q => q.SurveyOptions)
					.SingleOrDefault(c => c.SurveyId == s.SurveyId);

				if (existingSurvey != null)
				{
					// EF Core will handle cascade delete if configured properly
					context.Surveys.Remove(existingSurvey);
					context.SaveChanges();
				}
			}
			catch (Exception e)
			{
				throw new Exception($"Error deleting survey: {e.Message}", e);
			}
		}
	}
}
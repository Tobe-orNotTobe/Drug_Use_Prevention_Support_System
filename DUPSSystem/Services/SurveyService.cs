using BusinessObjects;
using BusinessObjects.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
	public class SurveyService : ISurveyService
	{
		private readonly ISurveyRepository _surveyRepository;

		public SurveyService(ISurveyRepository surveyRepository)
		{
			_surveyRepository = surveyRepository;
		}

		public List<Survey> GetAllSurveys() => _surveyRepository.GetAllSurveys();

		public Survey? GetSurveyById(int surveyId) => _surveyRepository.GetSurveyById(surveyId);

		public SurveyDetailResponse? GetSurveyWithDetails(int surveyId)
		{
			var survey = _surveyRepository.GetSurveyWithQuestionsAndOptions(surveyId);
			if (survey == null) return null;

			return new SurveyDetailResponse
			{
				SurveyId = survey.SurveyId,
				Name = survey.Name,
				Description = survey.Description,
				CreatedAt = survey.CreatedAt,
				Questions = survey.SurveyQuestions.Select(q => new SurveyQuestionDetail
				{
					QuestionId = q.QuestionId,
					QuestionText = q.QuestionText,
					QuestionType = q.QuestionType,
					Options = q.SurveyOptions.Select(o => new SurveyOptionDetail
					{
						OptionId = o.OptionId,
						OptionText = o.OptionText,
						Score = o.Score
					}).ToList()
				}).ToList()
			};
		}

		public void SaveSurvey(Survey survey)
		{
			ValidateSurvey(survey);
			ValidateSurveyQuestions(survey.SurveyQuestions);

			survey.CreatedAt = DateTime.UtcNow;

			// Process questions and options
			ProcessSurveyQuestions(survey);

			_surveyRepository.SaveSurvey(survey);
		}

		public void UpdateSurvey(Survey survey)
		{
			ValidateSurvey(survey);
			ValidateSurveyQuestions(survey.SurveyQuestions);

			// Process questions and options for update
			ProcessSurveyQuestions(survey);

			_surveyRepository.UpdateSurvey(survey);
		}

		public void DeleteSurvey(Survey survey)
		{
			if (survey == null)
				throw new ArgumentNullException(nameof(survey));

			_surveyRepository.DeleteSurvey(survey);
		}

		public List<Survey> SearchSurveys(string searchTerm) => _surveyRepository.SearchSurveys(searchTerm);

		#region Private Validation Methods

		private void ValidateSurvey(Survey survey)
		{
			if (survey == null)
				throw new ArgumentNullException(nameof(survey));

			if (string.IsNullOrWhiteSpace(survey.Name))
				throw new ArgumentException("Tên khảo sát không được để trống");

			if (survey.Name.Length > 255)
				throw new ArgumentException("Tên khảo sát không được vượt quá 255 ký tự");
		}

		private void ValidateSurveyQuestions(ICollection<SurveyQuestion>? questions)
		{
			if (questions == null || !questions.Any())
				throw new ArgumentException("Khảo sát phải có ít nhất một câu hỏi");

			var validTypes = new[] { "SingleChoice", "MultipleChoice", "Text" };

			foreach (var question in questions)
			{
				// Validate question text
				if (string.IsNullOrWhiteSpace(question.QuestionText))
					throw new ArgumentException("Nội dung câu hỏi không được để trống");

				if (question.QuestionText.Length > 1000)
					throw new ArgumentException("Nội dung câu hỏi không được vượt quá 1000 ký tự");

				// Validate question type
				if (string.IsNullOrWhiteSpace(question.QuestionType))
					throw new ArgumentException("Loại câu hỏi không được để trống");

				if (!validTypes.Contains(question.QuestionType))
					throw new ArgumentException($"Loại câu hỏi '{question.QuestionType}' không hợp lệ");

				// Validate options for choice questions
				if (question.QuestionType != "Text")
				{
					ValidateSurveyOptions(question.SurveyOptions, question.QuestionType);
				}
				else
				{
					// Text questions shouldn't have options
					if (question.SurveyOptions != null && question.SurveyOptions.Any())
					{
						question.SurveyOptions.Clear();
					}
				}
			}
		}

		private void ValidateSurveyOptions(ICollection<SurveyOption>? options, string questionType)
		{
			if (options == null || !options.Any())
				throw new ArgumentException("Câu hỏi trắc nghiệm phải có ít nhất một lựa chọn");

			if (options.Count < 2)
				throw new ArgumentException("Câu hỏi trắc nghiệm phải có ít nhất 2 lựa chọn");

			if (options.Count > 10)
				throw new ArgumentException("Câu hỏi không được có quá 10 lựa chọn");

			foreach (var option in options)
			{
				if (string.IsNullOrWhiteSpace(option.OptionText))
					throw new ArgumentException("Nội dung lựa chọn không được để trống");

				if (option.OptionText.Length > 500)
					throw new ArgumentException("Nội dung lựa chọn không được vượt quá 500 ký tự");

				// Set default score if not provided
				if (!option.Score.HasValue)
				{
					option.Score = 0;
				}
			}

			// Check for duplicate options
			var duplicateOptions = options
				.GroupBy(o => o.OptionText.Trim().ToLower())
				.Where(g => g.Count() > 1)
				.Select(g => g.Key);

			if (duplicateOptions.Any())
				throw new ArgumentException("Không được có các lựa chọn trùng lặp trong cùng một câu hỏi");
		}

		private void ProcessSurveyQuestions(Survey survey)
		{
			if (survey.SurveyQuestions == null) return;

			foreach (var question in survey.SurveyQuestions)
			{
				// Set survey reference
				question.SurveyId = survey.SurveyId;
				question.Survey = survey;

				// Process options
				ProcessSurveyOptions(question);
			}
		}

		private void ProcessSurveyOptions(SurveyQuestion question)
		{
			if (question.SurveyOptions == null) return;

			foreach (var option in question.SurveyOptions)
			{
				// Set question reference
				option.QuestionId = question.QuestionId;
				option.Question = question;

				// Ensure score is set
				if (!option.Score.HasValue)
				{
					option.Score = 0;
				}

				// Trim whitespace
				option.OptionText = option.OptionText?.Trim();
			}
		}

		#endregion
	}
}
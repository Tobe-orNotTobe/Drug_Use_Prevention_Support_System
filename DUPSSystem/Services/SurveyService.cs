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
			if (survey == null)
				throw new ArgumentNullException(nameof(survey));

			if (string.IsNullOrWhiteSpace(survey.Name))
				throw new ArgumentException("Tên khảo sát không được để trống");

			survey.CreatedAt = DateTime.UtcNow;
			_surveyRepository.SaveSurvey(survey);
		}

		public void UpdateSurvey(Survey survey)
		{
			if (survey == null)
				throw new ArgumentNullException(nameof(survey));

			if (string.IsNullOrWhiteSpace(survey.Name))
				throw new ArgumentException("Tên khảo sát không được để trống");

			_surveyRepository.UpdateSurvey(survey);
		}

		public void DeleteSurvey(Survey survey)
		{
			if (survey == null)
				throw new ArgumentNullException(nameof(survey));

			_surveyRepository.DeleteSurvey(survey);
		}

		public List<Survey> SearchSurveys(string searchTerm) => _surveyRepository.SearchSurveys(searchTerm);
	}
}
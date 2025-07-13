using BusinessObjects;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
	public class SurveyQuestionService : ISurveyQuestionService
	{
		private readonly ISurveyQuestionRepository _questionRepository;

		public SurveyQuestionService(ISurveyQuestionRepository questionRepository)
		{
			_questionRepository = questionRepository;
		}

		public List<SurveyQuestion> GetQuestionsBySurveyId(int surveyId) => _questionRepository.GetQuestionsBySurveyId(surveyId);

		public List<SurveyQuestion> GetAll() => _questionRepository.GetAll();

		public SurveyQuestion? GetQuestionById(int questionId) => _questionRepository.GetQuestionById(questionId);

		public void SaveQuestion(SurveyQuestion question)
		{
			if (question == null)
				throw new ArgumentNullException(nameof(question));

			if (string.IsNullOrWhiteSpace(question.QuestionText))
				throw new ArgumentException("Nội dung câu hỏi không được để trống");

			_questionRepository.SaveQuestion(question);
		}

		public void UpdateQuestion(SurveyQuestion question)
		{
			if (question == null)
				throw new ArgumentNullException(nameof(question));

			if (string.IsNullOrWhiteSpace(question.QuestionText))
				throw new ArgumentException("Nội dung câu hỏi không được để trống");

			_questionRepository.UpdateQuestion(question);
		}

		public void DeleteQuestion(SurveyQuestion question)
		{
			if (question == null)
				throw new ArgumentNullException(nameof(question));

			_questionRepository.DeleteQuestion(question);
		}
	}
}

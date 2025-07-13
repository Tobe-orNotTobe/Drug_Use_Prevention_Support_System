using BusinessObjects;

namespace Services.Interfaces
{
	public interface ISurveyQuestionService
	{
		List<SurveyQuestion> GetQuestionsBySurveyId(int surveyId);
		List<SurveyQuestion> GetAll();
		SurveyQuestion? GetQuestionById(int questionId);
		void SaveQuestion(SurveyQuestion question);
		void UpdateQuestion(SurveyQuestion question);
		void DeleteQuestion(SurveyQuestion question);
	}
}

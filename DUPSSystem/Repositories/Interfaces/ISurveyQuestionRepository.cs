using BusinessObjects;

namespace Repositories.Interfaces
{
	public interface ISurveyQuestionRepository
	{
		List<SurveyQuestion> GetQuestionsBySurveyId(int surveyId);
		List<SurveyQuestion> GetAll();
		SurveyQuestion? GetQuestionById(int questionId);
		void SaveQuestion(SurveyQuestion question);
		void UpdateQuestion(SurveyQuestion question);
		void DeleteQuestion(SurveyQuestion question);
	}

}

using BusinessObjects;

namespace Repositories.Interfaces
{
	public interface ISurveyRepository
	{
		List<Survey> GetAllSurveys();
		Survey? GetSurveyById(int surveyId);
		Survey? GetSurveyWithQuestionsAndOptions(int surveyId);
		void SaveSurvey(Survey survey);
		void UpdateSurvey(Survey survey);
		void DeleteSurvey(Survey survey);
		List<Survey> SearchSurveys(string searchTerm);
	}
}

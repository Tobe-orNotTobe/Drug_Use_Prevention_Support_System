using BusinessObjects;
using BusinessObjects.DTOs;

namespace Services.Interfaces
{
	public interface ISurveyService
	{
		List<Survey> GetAllSurveys();
		Survey? GetSurveyById(int surveyId);
		SurveyDetailResponse? GetSurveyWithDetails(int surveyId);
		void SaveSurvey(Survey survey);
		void UpdateSurvey(Survey survey);
		void DeleteSurvey(Survey survey);
		List<Survey> SearchSurveys(string searchTerm);
	}
}

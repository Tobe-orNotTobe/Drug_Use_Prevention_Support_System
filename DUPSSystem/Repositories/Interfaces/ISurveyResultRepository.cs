using BusinessObjects;

namespace Repositories.Interfaces
{
	public interface ISurveyResultRepository
	{
		List<UserSurveyResult> GetUserSurveyResults(int userId);
		UserSurveyResult? GetSurveyResultById(int resultId);
		UserSurveyResult? GetUserSurveyResult(int userId, int surveyId);
		void SaveSurveyResult(UserSurveyResult result);
		void UpdateSurveyResult(UserSurveyResult result);
		void DeleteSurveyResult(UserSurveyResult result);
	}
}

using BusinessObjects;
using BusinessObjects.DTOs;

namespace Services.Interfaces
{
	public interface ISurveyResultService
	{
		SurveySubmissionResponse SubmitSurvey(SurveySubmissionRequest request);
		UserSurveyHistoryResponse GetUserSurveyHistory(int userId);
		UserSurveyResult? GetSurveyResultById(int resultId);
		void DeleteSurveyResult(UserSurveyResult result);
		bool HasUserTakenSurvey(int userId, int surveyId);
	}
}

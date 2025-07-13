using BusinessObjects;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class SurveyResultRepository : ISurveyResultRepository
	{
		public List<UserSurveyResult> GetUserSurveyResults(int userId) => UserSurveyResultDAO.GetByUserId(userId);

		public UserSurveyResult? GetSurveyResultById(int resultId) => UserSurveyResultDAO.GetById(resultId);

		public UserSurveyResult? GetUserSurveyResult(int userId, int surveyId) => UserSurveyResultDAO.GetByUserAndSurvey(userId, surveyId);

		public void SaveSurveyResult(UserSurveyResult result) => UserSurveyResultDAO.Save(result);

		public void UpdateSurveyResult(UserSurveyResult result) => UserSurveyResultDAO.Update(result);

		public void DeleteSurveyResult(UserSurveyResult result) => UserSurveyResultDAO.Delete(result);
	}
}

using BusinessObjects;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class SurveyAnswerRepository : ISurveyAnswerRepository
	{
		public List<UserSurveyAnswer> GetAnswersByResultId(int resultId) => UserSurveyAnswerDAO.GetByResultId(resultId);

		public void SaveAnswer(UserSurveyAnswer answer) => UserSurveyAnswerDAO.Save(answer);

		public void SaveAnswers(List<UserSurveyAnswer> answers) => UserSurveyAnswerDAO.SaveBatch(answers);

		public void DeleteAnswersByResultId(int resultId) => UserSurveyAnswerDAO.DeleteByResultId(resultId);
	}
}

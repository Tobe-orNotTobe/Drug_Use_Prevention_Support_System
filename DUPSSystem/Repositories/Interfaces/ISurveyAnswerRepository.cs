using BusinessObjects;

namespace Repositories.Interfaces
{
	public interface ISurveyAnswerRepository
	{
		List<UserSurveyAnswer> GetAnswersByResultId(int resultId);
		void SaveAnswer(UserSurveyAnswer answer);
		void SaveAnswers(List<UserSurveyAnswer> answers);
		void DeleteAnswersByResultId(int resultId);
	}
}

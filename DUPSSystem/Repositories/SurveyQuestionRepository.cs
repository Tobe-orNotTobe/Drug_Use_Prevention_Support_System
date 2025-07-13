using BusinessObjects;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class SurveyQuestionRepository : ISurveyQuestionRepository
	{
		public List<SurveyQuestion> GetQuestionsBySurveyId(int surveyId) => SurveyQuestionDAO.GetBySurveyId(surveyId);
		public List<SurveyQuestion> GetAll() => SurveyQuestionDAO.GetAll();
		public SurveyQuestion? GetQuestionById(int questionId) => SurveyQuestionDAO.GetById(questionId);

		public void SaveQuestion(SurveyQuestion question) => SurveyQuestionDAO.Save(question);

		public void UpdateQuestion(SurveyQuestion question) => SurveyQuestionDAO.Update(question);

		public void DeleteQuestion(SurveyQuestion question) => SurveyQuestionDAO.Delete(question);
	}
}

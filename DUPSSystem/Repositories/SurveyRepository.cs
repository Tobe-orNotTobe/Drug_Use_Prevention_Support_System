using BusinessObjects;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class SurveyRepository : ISurveyRepository
	{
		public List<Survey> GetAllSurveys() => SurveyDAO.GetAll();

		public Survey? GetSurveyById(int surveyId) => SurveyDAO.GetById(surveyId);

		public Survey? GetSurveyWithQuestionsAndOptions(int surveyId) => SurveyDAO.GetWithQuestionsAndOptions(surveyId);

		public void SaveSurvey(Survey survey) => SurveyDAO.Save(survey);

		public void UpdateSurvey(Survey survey) => SurveyDAO.Update(survey);

		public void DeleteSurvey(Survey survey) => SurveyDAO.Delete(survey);

		public List<Survey> SearchSurveys(string searchTerm) => SurveyDAO.Search(searchTerm);
	}
}

using BusinessObjects;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories
{
	public class SurveyOptionRepository : ISurveyOptionRepository
	{
		public List<SurveyOption> GetOptionsByQuestionId(int questionId) => SurveyOptionDAO.GetByQuestionId(questionId);

		public SurveyOption? GetOptionById(int optionId) => SurveyOptionDAO.GetById(optionId);

		public void SaveOption(SurveyOption option) => SurveyOptionDAO.Save(option);

		public void UpdateOption(SurveyOption option) => SurveyOptionDAO.Update(option);

		public void DeleteOption(SurveyOption option) => SurveyOptionDAO.Delete(option);
	}
}

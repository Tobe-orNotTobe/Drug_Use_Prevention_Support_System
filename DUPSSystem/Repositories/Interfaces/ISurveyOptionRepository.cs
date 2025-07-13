using BusinessObjects;

namespace Repositories.Interfaces
{
	public interface ISurveyOptionRepository
	{
		List<SurveyOption> GetOptionsByQuestionId(int questionId);
		SurveyOption? GetOptionById(int optionId);
		void SaveOption(SurveyOption option);
		void UpdateOption(SurveyOption option);
		void DeleteOption(SurveyOption option);
	}
}

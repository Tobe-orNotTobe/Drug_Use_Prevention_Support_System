using BusinessObjects;

namespace Services.Interfaces
{
	public interface ISurveyOptionService
	{
		List<SurveyOption> GetOptionsByQuestionId(int questionId);
		SurveyOption? GetOptionById(int optionId);
		void SaveOption(SurveyOption option);
		void UpdateOption(SurveyOption option);
		void DeleteOption(SurveyOption option);
	}
}

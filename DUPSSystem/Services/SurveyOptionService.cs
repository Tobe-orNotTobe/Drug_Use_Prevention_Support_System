using BusinessObjects;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
	public class SurveyOptionService : ISurveyOptionService
	{
		private readonly ISurveyOptionRepository _optionRepository;

		public SurveyOptionService(ISurveyOptionRepository optionRepository)
		{
			_optionRepository = optionRepository;
		}

		public List<SurveyOption> GetOptionsByQuestionId(int questionId) => _optionRepository.GetOptionsByQuestionId(questionId);

		public SurveyOption? GetOptionById(int optionId) => _optionRepository.GetOptionById(optionId);

		public void SaveOption(SurveyOption option)
		{
			if (option == null)
				throw new ArgumentNullException(nameof(option));

			if (string.IsNullOrWhiteSpace(option.OptionText))
				throw new ArgumentException("Nội dung tùy chọn không được để trống");

			_optionRepository.SaveOption(option);
		}

		public void UpdateOption(SurveyOption option)
		{
			if (option == null)
				throw new ArgumentNullException(nameof(option));

			if (string.IsNullOrWhiteSpace(option.OptionText))
				throw new ArgumentException("Nội dung tùy chọn không được để trống");

			_optionRepository.UpdateOption(option);
		}

		public void DeleteOption(SurveyOption option)
		{
			if (option == null)
				throw new ArgumentNullException(nameof(option));

			_optionRepository.DeleteOption(option);
		}
	}
}

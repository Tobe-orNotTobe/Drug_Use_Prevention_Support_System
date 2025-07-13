using BusinessObjects;
using BusinessObjects.DTOs;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
	public class SurveyResultService : ISurveyResultService
	{
		private readonly ISurveyResultRepository _resultRepository;
		private readonly ISurveyAnswerRepository _answerRepository;
		private readonly ISurveyRepository _surveyRepository;
		private readonly IUserRepository _userRepository;

		public SurveyResultService(
			ISurveyResultRepository resultRepository,
			ISurveyAnswerRepository answerRepository,
			ISurveyRepository surveyRepository,
			IUserRepository userRepository)
		{
			_resultRepository = resultRepository;
			_answerRepository = answerRepository;
			_surveyRepository = surveyRepository;
			_userRepository = userRepository;
		}

		public SurveySubmissionResponse SubmitSurvey(SurveySubmissionRequest request)
		{
			try
			{
				// Validate user exists
				var user = _userRepository.GetAccountById(request.UserId);
				if (user == null)
				{
					return new SurveySubmissionResponse
					{
						Success = false,
						Message = "Người dùng không tồn tại"
					};
				}

				// Validate survey exists
				var survey = _surveyRepository.GetSurveyById(request.SurveyId);
				if (survey == null)
				{
					return new SurveySubmissionResponse
					{
						Success = false,
						Message = "Khảo sát không tồn tại"
					};
				}

				// Check if user already took this survey
				var existingResult = _resultRepository.GetUserSurveyResult(request.UserId, request.SurveyId);
				if (existingResult != null)
				{
					return new SurveySubmissionResponse
					{
						Success = false,
						Message = "Bạn đã làm khảo sát này rồi"
					};
				}

				// Calculate total score
				int totalScore = request.Answers.Sum(a => a.Score);

				// Generate recommendation based on score
				string recommendation = GenerateRecommendation(survey.Name, totalScore, request.Answers.Count);

				// Create survey result
				var result = new UserSurveyResult
				{
					UserId = request.UserId,
					SurveyId = request.SurveyId,
					TakenAt = DateTime.UtcNow,
					TotalScore = totalScore,
					Recommendation = recommendation
				};

				_resultRepository.SaveSurveyResult(result);

				// Create answers
				var answers = request.Answers.Select(a => new UserSurveyAnswer
				{
					ResultId = result.ResultId,
					QuestionId = a.QuestionId,
					OptionId = a.OptionId,
					AnswerText = a.AnswerText
				}).ToList();

				_answerRepository.SaveAnswers(answers);

				return new SurveySubmissionResponse
				{
					Success = true,
					Message = "Gửi khảo sát thành công",
					ResultId = result.ResultId,
					TotalScore = totalScore,
					Recommendation = recommendation
				};
			}
			catch (Exception ex)
			{
				return new SurveySubmissionResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi gửi khảo sát: " + ex.Message
				};
			}
		}

		public UserSurveyHistoryResponse GetUserSurveyHistory(int userId)
		{
			try
			{
				var results = _resultRepository.GetUserSurveyResults(userId);

				var historyItems = results.Select(r => new UserSurveyHistoryItem
				{
					ResultId = r.ResultId,
					SurveyId = r.SurveyId,
					SurveyName = r.Survey?.Name ?? "Khảo sát không xác định",
					TakenAt = r.TakenAt,
					TotalScore = r.TotalScore ?? 0,
					Recommendation = r.Recommendation
				}).ToList();

				return new UserSurveyHistoryResponse
				{
					Success = true,
					Message = "Lấy lịch sử khảo sát thành công",
					Data = historyItems
				};
			}
			catch (Exception ex)
			{
				return new UserSurveyHistoryResponse
				{
					Success = false,
					Message = "Đã xảy ra lỗi khi lấy lịch sử khảo sát: " + ex.Message,
					Data = new List<UserSurveyHistoryItem>()
				};
			}
		}

		public UserSurveyResult? GetSurveyResultById(int resultId) => _resultRepository.GetSurveyResultById(resultId);

		public void DeleteSurveyResult(UserSurveyResult result)
		{
			if (result == null)
				throw new ArgumentNullException(nameof(result));

			// Delete answers first
			_answerRepository.DeleteAnswersByResultId(result.ResultId);

			// Then delete result
			_resultRepository.DeleteSurveyResult(result);
		}

		public bool HasUserTakenSurvey(int userId, int surveyId)
		{
			var result = _resultRepository.GetUserSurveyResult(userId, surveyId);
			return result != null;
		}

		private string GenerateRecommendation(string surveyName, int totalScore, int totalQuestions)
		{
			double percentage = totalQuestions > 0 ? (double)totalScore / (totalQuestions * 3) * 100 : 0; // Assuming max 3 points per question

			return surveyName.ToLower() switch
			{
				var name when name.Contains("assist") => percentage switch
				{
					>= 70 => "Mức độ rủi ro cao. Khuyến nghị gặp chuyên gia tư vấn ngay lập tức và tham gia các khóa học phòng ngừa.",
					>= 40 => "Mức độ rủi ro trung bình. Nên tham gia các khóa học đào tạo và theo dõi định kỳ.",
					>= 20 => "Mức độ rủi ro thấp. Khuyến nghị tham gia các chương trình giáo dục phòng ngừa.",
					_ => "Mức độ rủi ro rất thấp. Tiếp tục duy trì lối sống lành mạnh."
				},
				var name when name.Contains("crafft") => percentage switch
				{
					>= 60 => "Cần can thiệp chuyên sâu. Liên hệ với chuyên gia tư vấn để được hỗ trợ.",
					>= 30 => "Cần quan tâm và theo dõi. Tham gia các hoạt động tích cực.",
					_ => "Tình trạng ổn định. Tiếp tục duy trì các hoạt động tích cực."
				},
				_ => percentage switch
				{
					>= 70 => "Điểm số cao. Cần chú ý và có biện pháp can thiệp phù hợp.",
					>= 40 => "Điểm số trung bình. Nên tham gia các hoạt động hỗ trợ.",
					_ => "Điểm số tốt. Tiếp tục duy trì các hoạt động tích cực."
				}
			};
		}
	}
}
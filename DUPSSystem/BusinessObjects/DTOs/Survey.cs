using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs
{
	public class SurveyCreateRequest
	{
		[Required(ErrorMessage = "Tên khảo sát là bắt buộc")]
		[StringLength(255, ErrorMessage = "Tên khảo sát không được vượt quá 255 ký tự")]
		public string Name { get; set; } = null!;

		public string? Description { get; set; }
	}

	public class SurveyUpdateRequest
	{
		[Required(ErrorMessage = "SurveyId là bắt buộc")]
		public int SurveyId { get; set; }

		[Required(ErrorMessage = "Tên khảo sát là bắt buộc")]
		[StringLength(255, ErrorMessage = "Tên khảo sát không được vượt quá 255 ký tự")]
		public string Name { get; set; } = null!;

		public string? Description { get; set; }
	}

	public class SurveyQuestionCreateRequest
	{
		[Required(ErrorMessage = "SurveyId là bắt buộc")]
		public int SurveyId { get; set; }

		[Required(ErrorMessage = "Câu hỏi là bắt buộc")]
		public string QuestionText { get; set; } = null!;

		[Required(ErrorMessage = "Loại câu hỏi là bắt buộc")]
		[StringLength(50)]
		public string QuestionType { get; set; } = null!; // SingleChoice, MultipleChoice, TextInput
	}

	public class SurveyOptionCreateRequest
	{
		[Required(ErrorMessage = "QuestionId là bắt buộc")]
		public int QuestionId { get; set; }

		[Required(ErrorMessage = "Nội dung tùy chọn là bắt buộc")]
		public string OptionText { get; set; } = null!;

		public int? Score { get; set; }
	}

	public class SurveySubmissionRequest
	{
		[Required(ErrorMessage = "UserId là bắt buộc")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "SurveyId là bắt buộc")]
		public int SurveyId { get; set; }

		[Required(ErrorMessage = "Answers là bắt buộc")]
		public List<SurveyAnswerSubmission> Answers { get; set; } = new List<SurveyAnswerSubmission>();
	}

	public class SurveyAnswerSubmission
	{
		[Required(ErrorMessage = "QuestionId là bắt buộc")]
		public int QuestionId { get; set; }

		public int? OptionId { get; set; }

		public string? AnswerText { get; set; }

		public int Score { get; set; }
	}

	public class SurveySubmissionResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public int? ResultId { get; set; }
		public int TotalScore { get; set; }
		public string? Recommendation { get; set; }
	}

	public class UserSurveyHistoryResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public List<UserSurveyHistoryItem> Data { get; set; } = new List<UserSurveyHistoryItem>();
	}

	public class UserSurveyHistoryItem
	{
		public int ResultId { get; set; }
		public int SurveyId { get; set; }
		public string SurveyName { get; set; } = null!;
		public DateTime TakenAt { get; set; }
		public int TotalScore { get; set; }
		public string? Recommendation { get; set; }
	}

	public class SurveyDetailResponse
	{
		public int SurveyId { get; set; }
		public string Name { get; set; } = null!;
		public string? Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public List<SurveyQuestionDetail> Questions { get; set; } = new List<SurveyQuestionDetail>();
	}

	public class SurveyQuestionDetail
	{
		public int QuestionId { get; set; }
		public string QuestionText { get; set; } = null!;
		public string QuestionType { get; set; } = null!;
		public List<SurveyOptionDetail> Options { get; set; } = new List<SurveyOptionDetail>();
	}

	public class SurveyOptionDetail
	{
		public int OptionId { get; set; }
		public string OptionText { get; set; } = null!;
		public int? Score { get; set; }
	}
}
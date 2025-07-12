using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects;

public partial class UserSurveyResult
{
	[Key]
	public int ResultId { get; set; }

    public int UserId { get; set; }

    public int SurveyId { get; set; }

    public DateTime TakenAt { get; set; }

    public int? TotalScore { get; set; }

    public string? Recommendation { get; set; }

    public virtual Survey Survey { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserSurveyAnswer> UserSurveyAnswers { get; set; } = new List<UserSurveyAnswer>();
}

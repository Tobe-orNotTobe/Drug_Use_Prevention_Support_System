using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects;

public partial class SurveyQuestion
{
	[Key]
	public int QuestionId { get; set; }

    public int SurveyId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string QuestionType { get; set; } = null!;

    public virtual Survey Survey { get; set; } = null!;

    public virtual ICollection<SurveyOption> SurveyOptions { get; set; } = new List<SurveyOption>();

    public virtual ICollection<UserSurveyAnswer> UserSurveyAnswers { get; set; } = new List<UserSurveyAnswer>();
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects;

public partial class UserSurveyAnswer
{
	[Key]
	public int AnswerId { get; set; }

    public int ResultId { get; set; }

    public int QuestionId { get; set; }

    public int? OptionId { get; set; }

    public string? AnswerText { get; set; }

    public virtual SurveyOption? Option { get; set; }

    public virtual SurveyQuestion Question { get; set; } = null!;

    public virtual UserSurveyResult Result { get; set; } = null!;
}

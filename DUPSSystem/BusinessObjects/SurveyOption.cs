using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class SurveyOption
{
    public int OptionId { get; set; }

    public int QuestionId { get; set; }

    public string OptionText { get; set; } = null!;

    public int? Score { get; set; }

    public virtual SurveyQuestion Question { get; set; } = null!;

    public virtual ICollection<UserSurveyAnswer> UserSurveyAnswers { get; set; } = new List<UserSurveyAnswer>();
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects;

public partial class Survey
{
	[Key]
	public int SurveyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ProgramSurvey> ProgramSurveys { get; set; } = new List<ProgramSurvey>();

    public virtual ICollection<SurveyQuestion> SurveyQuestions { get; set; } = new List<SurveyQuestion>();

    public virtual ICollection<UserSurveyResult> UserSurveyResults { get; set; } = new List<UserSurveyResult>();
}

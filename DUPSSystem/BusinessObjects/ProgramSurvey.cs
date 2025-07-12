using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class ProgramSurvey
{
    public int ProgramSurveyId { get; set; }

    public int ProgramId { get; set; }

    public int SurveyId { get; set; }

    public string SurveyType { get; set; } = null!;

    public virtual CommunicationProgram Program { get; set; } = null!;

    public virtual Survey Survey { get; set; } = null!;
}

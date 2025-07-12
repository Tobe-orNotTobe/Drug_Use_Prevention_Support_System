using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects;

public partial class CommunicationProgram
{
	[Key]
	public int ProgramId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ProgramSurvey> ProgramSurveys { get; set; } = new List<ProgramSurvey>();

    public virtual ICollection<UserProgram> UserPrograms { get; set; } = new List<UserProgram>();
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects;

public partial class UserProgram
{
	[Key]
	public int UserProgramId { get; set; }

    public int UserId { get; set; }

    public int ProgramId { get; set; }

    public DateTime JoinedAt { get; set; }

    public virtual CommunicationProgram Program { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

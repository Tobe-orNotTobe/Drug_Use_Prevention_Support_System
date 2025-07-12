using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class Consultant
{
    public int ConsultantId { get; set; }

    public int UserId { get; set; }

    public string? Qualification { get; set; }

    public string? Expertise { get; set; }

    public string? WorkSchedule { get; set; }

    public string? Bio { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual User User { get; set; } = null!;
}

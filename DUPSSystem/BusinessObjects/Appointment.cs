using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int UserId { get; set; }

    public int ConsultantId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public int? DurationMinutes { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Consultant Consultant { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

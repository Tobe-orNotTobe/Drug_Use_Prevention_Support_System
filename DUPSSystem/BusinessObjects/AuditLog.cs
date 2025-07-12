using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class AuditLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? TableName { get; set; }

    public int? RecordId { get; set; }

    public DateTime LogDate { get; set; }

    public string? Details { get; set; }

    public virtual User? User { get; set; }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects;

public partial class AuditLog
{
    [Key]
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? TableName { get; set; }

    public int? RecordId { get; set; }

    public DateTime LogDate { get; set; }

    public string? Details { get; set; }

    public virtual User? User { get; set; }
}

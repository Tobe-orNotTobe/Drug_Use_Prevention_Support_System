using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class UserCourse
{
    public int UserCourseId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime RegisteredAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

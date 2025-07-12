using System;
using System.Collections.Generic;

namespace BusinessObjects;

public partial class Course
{
    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? TargetAudience { get; set; }

    public int? DurationMinutes { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<UserCourse> UserCourses { get; set; } = new List<UserCourse>();
}

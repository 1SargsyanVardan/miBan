using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class GroupCourse
{
    public int GroupCourseId { get; set; }

    public int GroupId { get; set; }

    public int CourseId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Group Group { get; set; } = null!;
}

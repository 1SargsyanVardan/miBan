using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class StudentCourse
{
    public int StudentCourseId { get; set; }

    public int? StudentId { get; set; }

    public int? CourseId { get; set; }

    public virtual Course? Course { get; set; }

    public virtual User? Student { get; set; }
}

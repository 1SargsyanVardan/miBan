using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public string? Description { get; set; }

    public int? TeacherId { get; set; }

    public virtual ICollection<GroupCourse> GroupCourses { get; set; } = new List<GroupCourse>();

    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    public virtual User? Teacher { get; set; }
}

using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Criterion
{
    public int CriteriaId { get; set; }

    public string CriteriaName { get; set; } = null!;

    public string? Role { get; set; }

    public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    public virtual ICollection<StudentReport> StudentReports { get; set; } = new List<StudentReport>();

    public virtual ICollection<TeacherReport> TeacherReports { get; set; } = new List<TeacherReport>();
}

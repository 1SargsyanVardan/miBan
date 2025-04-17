using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class TeacherReport
{
    public int ReportId { get; set; }

    public DateTime? ReportDate { get; set; }

    public int? TeacherId { get; set; }

    public int? TotalScores { get; set; }

    public double? AverageScore { get; set; }

    public int CriteriaId { get; set; }

    public virtual Criterion Criteria { get; set; } = null!;

    public virtual User? Teacher { get; set; }
}

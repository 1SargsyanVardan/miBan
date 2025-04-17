using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class StudentReport
{
    public int ReportId { get; set; }

    public DateTime ReportDate { get; set; }

    public int StudentId { get; set; }

    public int TotalScores { get; set; }

    public double AverageScore { get; set; }

    public int CriteriaId { get; set; }

    public virtual Criterion Criteria { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? DateJoined { get; set; }

    public string? Description { get; set; }

    public int? GroupId { get; set; }

    public string? VerificationCode { get; set; }

    public bool IsEmailConfirmed { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Evaluation> EvaluationEvaluatees { get; set; } = new List<Evaluation>();

    public virtual ICollection<Evaluation> EvaluationEvaluators { get; set; } = new List<Evaluation>();

    public virtual Group? Group { get; set; }

    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    public virtual ICollection<StudentReport> StudentReports { get; set; } = new List<StudentReport>();

    public virtual ICollection<TeacherReport> TeacherReports { get; set; } = new List<TeacherReport>();
}

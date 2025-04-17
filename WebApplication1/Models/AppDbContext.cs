using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Criterion> Criteria { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Evaluation> Evaluations { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupCourse> GroupCourses { get; set; }

    public virtual DbSet<StudentCourse> StudentCourses { get; set; }

    public virtual DbSet<StudentReport> StudentReports { get; set; }

    public virtual DbSet<TeacherReport> TeacherReports { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=miBan;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D7187CB8DB096");

            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.CourseName).HasMaxLength(100);
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Courses)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK__Courses__Teacher__3B75D760");
        });

        modelBuilder.Entity<Criterion>(entity =>
        {
            entity.HasKey(e => e.CriteriaId).HasName("PK__Criteria__FE6ADB2D76C859C6");

            entity.Property(e => e.CriteriaId).HasColumnName("CriteriaID");
            entity.Property(e => e.CriteriaName).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(10);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BCD25AFC7F0");

            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.ModeName)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ProgramName)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Evaluation>(entity =>
        {
            entity.HasKey(e => e.EvaluationId).HasName("PK__Evaluati__36AE68D3DAB986D6");

            entity.Property(e => e.EvaluationId).HasColumnName("EvaluationID");
            entity.Property(e => e.CriteriaId).HasColumnName("CriteriaID");
            entity.Property(e => e.EvaluateeId).HasColumnName("EvaluateeID");
            entity.Property(e => e.EvaluationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EvaluatorId).HasColumnName("EvaluatorID");
            entity.Property(e => e.Role).HasMaxLength(10);

            entity.HasOne(d => d.Criteria).WithMany(p => p.Evaluations)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Evaluatio__Crite__29221CFB");

            entity.HasOne(d => d.Evaluatee).WithMany(p => p.EvaluationEvaluatees)
                .HasForeignKey(d => d.EvaluateeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Evaluatio__Evalu__2A164134");

            entity.HasOne(d => d.Evaluator).WithMany(p => p.EvaluationEvaluators)
                .HasForeignKey(d => d.EvaluatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Evaluatio__Evalu__282DF8C2");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Groups__149AF30A0583D99B");

            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");

            entity.HasOne(d => d.Department).WithMany(p => p.Groups)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__Groups__Departme__7C4F7684");
        });

        modelBuilder.Entity<GroupCourse>(entity =>
        {
            entity.HasKey(e => e.GroupCourseId).HasName("PK__GroupCou__2BD533F805B1B24D");

            entity.Property(e => e.GroupCourseId).HasColumnName("GroupCourseID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");

            entity.HasOne(d => d.Course).WithMany(p => p.GroupCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GroupCour__Cours__00200768");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupCourses)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GroupCour__Group__7F2BE32F");
        });

        modelBuilder.Entity<StudentCourse>(entity =>
        {
            entity.HasKey(e => e.StudentCourseId).HasName("PK__StudentC__7E3E2FB20D3E9D80");

            entity.Property(e => e.StudentCourseId).HasColumnName("StudentCourseID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.Course).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__StudentCo__Cours__5CD6CB2B");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__StudentCo__Stude__5BE2A6F2");
        });

        modelBuilder.Entity<StudentReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__StudentR__D5BD48E5C8DE64A0");

            entity.Property(e => e.ReportId).HasColumnName("ReportID");
            entity.Property(e => e.ReportDate).HasColumnType("datetime");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.Criteria).WithMany(p => p.StudentReports)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentReports_Criteria");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentReports)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentReports_Students");
        });

        modelBuilder.Entity<TeacherReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__AdminRep__D5BD48E5AE0CFB81");

            entity.Property(e => e.ReportId).HasColumnName("ReportID");
            entity.Property(e => e.ReportDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.Criteria).WithMany(p => p.TeacherReports)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TeacherReports_Criteria");

            entity.HasOne(d => d.Teacher).WithMany(p => p.TeacherReports)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK__AdminRepo__Teach__4E88ABD4");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC373FCF4D");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105340B441469").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.DateJoined)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VerificationCode).HasMaxLength(10);

            entity.HasOne(d => d.Group).WithMany(p => p.Users)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Users_Groups");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

namespace WebApplication1.Models.MyModels
{
    public class TeacherReportModel
    {
        public DateTime ReportDate { get; set; }

        public int TeacherId { get; set; }

        public int TotalScores { get; set; }

        public double AverageScore { get; set; }
    }
}

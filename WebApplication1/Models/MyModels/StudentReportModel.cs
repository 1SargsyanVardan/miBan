namespace WebApplication1.Models.MyModels
{
    public class StudentReportModel
    {
        public DateTime ReportDate { get; set; }

        public int StudentId { get; set; }

        public int TotalScores { get; set; }

        public double AverageScore { get; set; }
    }
}

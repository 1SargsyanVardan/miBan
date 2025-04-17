namespace WebApplication1.Models.MyModels
{
    public class CourseModel
    {
        public string CourseName { get; set; } = null!;

        public string? Description { get; set; }

        public string TeacherName { get; set; }
        public CourseModel(string courseName,string description,string teacherName) {
            CourseName = courseName;
            Description = description;
            TeacherName = teacherName;
        }

    }
}

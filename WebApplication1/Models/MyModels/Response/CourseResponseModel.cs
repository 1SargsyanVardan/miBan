namespace WebApplication1.Models.MyModels.Response
{
    public class CourseResponseModel
    {
        public string CourseName { get; set; } = null!;

        public string? Description { get; set; }

        public string TeacherName { get; set; }
        public CourseResponseModel(string courseName, string description, string teacherName)
        {
            CourseName = courseName;
            Description = description;
            TeacherName = teacherName;
        }

    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using WebApplication1.HelperClasses;
using WebApplication1.Models;
using WebApplication1.Models.MyModels;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Teacher")]
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeacherController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Students/{courseId}")]
        public IActionResult GetStudentsByCourseId(int courseId)
        {
            var students = _context.StudentCourses
                .Where(sc => sc.CourseId == courseId)
                .Select(sc => sc.Student)
                .ToList();

            return Ok(students);
        }

        [HttpGet("CoursesByTeacher")]
        public IActionResult GetAllGroups()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courses = _context.Courses
                .Where(c => c.TeacherId == int.Parse(teacherId)) 
                .Select(c => new
                {
                    c.CourseId,
                    c.CourseName,
                    c.Description
                })
                .ToList();

            if (!courses.Any())
            {
                return NotFound("No courses found for the current teacher.");
            }

            return Ok(courses);
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassDtoModel model)
        {
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return BadRequest("Նոր գաղտնաբառերը չեն համընկնում");
            }

            if (!Regex.IsMatch(model.NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))
            {
                return BadRequest("Գաղտնաբառը պետք է պարունակի առնվազն 8 նիշ, 1 մեծատառ, 1 փոքրատառ և 1 թիվ:");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("Դասախոսը չի գտնվել։");
            }

            int teacherId = int.Parse(userId);
            var teacher = await _context.Users.FindAsync(teacherId);

            if (teacher == null)
            {
                return NotFound("Դասախոսը գտնված չէ");
            }

            if (!PasswordHelper.VerifyPassword(teacher.PasswordHash, model.OldPassword))
            {
                return BadRequest("Հին գաղտնաբառը սխալ է");
            }

            teacher.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            return Ok("Գաղտնաբառը հաջողությամբ փոփոխվեց");
        }

        [HttpGet("StudentsForEvaluate/{departmentYear}/{groupNumber}")]
        public async Task<IActionResult> GetStudentsGroupName(int departmentYear, int groupNumber)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentYear == departmentYear);

            if (department == null)
            {
                return NotFound("Department not found.");
            }

            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.DepartmentId == department.DepartmentId && g.GroupId == groupNumber);

            if (group == null)
            {
                return NotFound("Group not found in this department.");
            }

            var students = await _context.Users
                .Where(u => u.Role == "Student" && u.GroupId == group.GroupId)
                .ToListAsync();

            var criteriaList = await _context.Criteria
                .Where(c => c.Role == "Student")
                .Select(c => new CriterionModel
                {
                    Id = c.CriteriaId,
                    Name = c.CriteriaName
                })
                .ToListAsync();

            var studentModels = students.Select(s => new EvaluationModel
            {
                Id = s.UserId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Description = s.Description,
                IsEvaluated = _context.Evaluations.Any(e => e.EvaluateeId == s.UserId), 
                Criterias = criteriaList 
            }).ToList();

            return Ok(studentModels);
        }
        [HttpPost("EvaluateStudent")]
        public async Task<IActionResult> EvaluateStudent([FromBody] EvaluationRequestModel evaluation)
        {
            var evaluatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Check checking = new Check();
            string check = checking.CheckEvaluationByUser(evaluation, evaluatorId, "Student", _context);

            if (check != string.Empty)
            {
                return BadRequest(check);
            }

            var criteriaIds = evaluation.Scores.Select(s => s.CriteriaID).ToList();

            foreach (var criteriaId in criteriaIds)
            {
                var evaluationEntities = evaluation.Scores.Where(c => c.CriteriaID == criteriaId)
                    .Select(score => new Evaluation
                    {
                        EvaluatorId = int.Parse(evaluatorId),
                        EvaluateeId = evaluation.EvaluateeID,
                        CriteriaId = criteriaId,
                        Score = score.Score,
                        Role = "Student",
                        EvaluationDate = DateTime.Now
                    }).ToList();

                _context.Evaluations.AddRange(evaluationEntities);
            }
            await _context.SaveChangesAsync();

            return Ok("Գնահատման պրոցեսը հաջող է ընթացել!");
        }
    }

}

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using WebApplication1.HelperClasses;
using WebApplication1.Models;
using WebApplication1.Models.MyModels;
using WebApplication1.Models.MyModels.Request;
using WebApplication1.Models.MyModels.Response;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Teacher")]
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TeacherController(AppDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("Students/{courseId}")]
        [ProducesResponseType(typeof(List<UserGetResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public IActionResult GetStudentsByCourseId(int courseId)
        {
            
            List<User> students;
            try
            {
                students = getStudents(courseId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            List<UserGetResponse> userGetResponse = new List<UserGetResponse>();

            foreach (var student in students)
            {
                userGetResponse.Add(_mapper.Map<UserGetResponse>(students));
            }

            return Ok(userGetResponse);
        }
        [HttpPost("SendMail/{courseId}")]
        [ProducesResponseType(typeof(List<UserGetResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public IActionResult SendMailByCourseId(int courseId,string mail)
        {
            var course = _context.Courses.Where(cId => cId.CourseId == courseId);
            if (!course.Any())
            {
                return BadRequest("Սխալ CourseId");
            }
            var students = _context.StudentCourses
                .Where(sc => sc.CourseId == courseId)
                .Select(sc => sc.Student)
                .ToList();
            if (students.Count() == 0)
            {
                return NotFound("Տվյալ կուրսում ուսանող չկա");
            }

            var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var teacher = _context.Users.FirstOrDefault(u => u.UserId == teacherId && u.Role == "Teacher");

            if (teacher == null)
            {
                return NotFound("Համակարգում մի բան այն չէ փորձեք կրկին։");
            }
            EmailSender emailSender = new EmailSender();
            foreach (var student in students)
            {
                emailSender.SendEmail(teacher.FirstName+" "+teacher.LastName,student.Email,mail);
            }

            return Ok();
        }

        [HttpGet("CoursesByTeacher")]
        [ProducesResponseType(typeof(List<Course>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
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
                return NotFound("Տվյալ դասախոսի համար չկան դասընթացներ");
            }

            return Ok(courses);
        }

        [HttpPost("ChangePassword")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassRequest model)
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
        [ProducesResponseType(typeof(List<EvaluationResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
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

            var studentModels = students.Select(s => new EvaluationResponseModel
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
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> EvaluateStudent([FromBody] EvaluationRequest evaluation)
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

        private List<User> getStudents(int courseId)
        {
            var course = _context.Courses.Where(cId => cId.CourseId == courseId);
            if (!course.Any())
            {
                throw new Exception("Սխալ CourseId");
            }
            var students = _context.StudentCourses
                .Where(sc => sc.CourseId == courseId)
                .Select(sc => sc.Student)
                .ToList();
            if (students.Count() == 0)
            {
                throw new Exception("Տվյալ կուրսում ուսանող չկա");
            }

            return students;
        }
    }

}

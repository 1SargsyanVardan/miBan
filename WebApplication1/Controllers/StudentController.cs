using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using WebApplication1.HelperClasses;
using WebApplication1.Models;
using WebApplication1.Models.MyModels.Request;
using WebApplication1.Models.MyModels.Response;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Student")]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

        public StudentController(AppDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        
        [HttpGet("Teachers")]
        [ProducesResponseType(typeof(List<TeacherModelForStudent>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public IActionResult GetTeachersForStudent()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(userId, out int studentId))
                {
                    return BadRequest("Սխալ userID.");
                }
                var teachers = _context.Users
                                .Where(student => student.UserId == studentId)
                                .SelectMany(student => student.Group.GroupCourses)
                                .Select(gc => gc.Course.Teacher)
                                .GroupBy(t => t.UserId)
                                .Select(g => g.First())
                                .ToList();
                var teacherModels = _mapper.Map<List<TeacherModelForStudent>>(teachers);
                return Ok(teacherModels);
            }
            catch (Exception ex)
            {
                Logger.Error("GetTeachersForStudent: {0} : {1} ", DateTime.Now, ex.Message);
                return BadRequest();
            }
            finally
            {
                Logger.Info("{0} : Success GetTeachersForStudent ", DateTime.Now);
            }
        }

        [HttpGet("Courses")]
        [ProducesResponseType(typeof(List<CourseResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public IActionResult GetCourses()
        {
            try
            {
                List<CourseResponseModel> result = new List<CourseResponseModel>();

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = _context.Users.FirstOrDefault(x => x.UserId == userId);

                var courses = _context.GroupCourses
                    .Where(gc => gc.GroupId == user.GroupId)
                    .Select(gc => new
                    {
                        gc.Course.CourseName,
                        gc.Course.Description,
                        TeacherName = gc.Course.Teacher != null
                            ? gc.Course.Teacher.FirstName + " " + gc.Course.Teacher.LastName
                            : "Unknown"
                    })
                    .ToList();

                foreach (var c in courses)
                {
                    result.Add(new CourseResponseModel(c.CourseName, c.Description, c.TeacherName));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.Error("GetCourses: {0} : {1} ", DateTime.Now, ex.Message);
                return BadRequest();
            }
            finally
            {
                Logger.Info("{0} : Success GetCourses ", DateTime.Now);
            }
        }

        [HttpGet("GetCoursesByGroup")]
        [ProducesResponseType(typeof(List<CourseResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCoursesByGroup([FromBody] DepAndGroupRequest model)
        {
            try
            {
                List<CourseResponseModel> result = new List<CourseResponseModel>();

                var groupId = _context.Groups
                            .Where(g => g.GroupNumber == model.GroupNumber &&
                                    g.Department.DepartmentYear == model.DepartamentYear)
                            .Select(g => g.GroupId)
                            .FirstOrDefault();

                if (groupId == 0)
                {
                    return BadRequest("Այդպիսի խումբ գոյություն չունի");
                }

                var courses = _context.GroupCourses
                    .Where(gc => gc.GroupId == groupId)
                    .Select(gc => new
                    {
                        gc.Course.CourseName,
                        gc.Course.Description,
                        TeacherName = gc.Course.Teacher != null
                            ? gc.Course.Teacher.FirstName + " " + gc.Course.Teacher.LastName
                            : "Unknown"
                    })
                    .ToList();

                foreach (var c in courses)
                {
                    result.Add(new CourseResponseModel(c.CourseName, c.Description, c.TeacherName));
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.Error("GetCoursesByGroup: {0} : {1} ", DateTime.Now, ex.Message);
                return BadRequest();
            }
            finally
            {
                Logger.Info("{0} : Success GetCoursesByGroup ", DateTime.Now);
            }
        }

        [HttpPost("ChangePassword")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassRequest model)
        {
            try
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
                    return Unauthorized("ՈՒսանողը չի գտնվել։");
                }

                int studentId = int.Parse(userId);
                var student = await _context.Users.FindAsync(studentId);

                if (student == null)
                {
                    return NotFound("Ուսանողը գտնված չէ");
                }

                if (!PasswordHelper.VerifyPassword(student.PasswordHash, model.OldPassword))
                {
                    return BadRequest("Հին գաղտնաբառը սխալ է");
                }

                student.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
                await _context.SaveChangesAsync();

                return Ok("Գաղտնաբառը հաջողությամբ փոփոխվեց");
            }
            catch (Exception ex)
            {
                Logger.Error("ChangePassword: {0} : {1} ", DateTime.Now, ex.Message);
                return BadRequest();
            }
            finally
            {
                Logger.Info("{0} : Success ChangePassword ", DateTime.Now);
            }
        }

        [HttpGet("GetTeachersToEvaluate")]
        [ProducesResponseType(typeof(List<EvaluationResponseModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetTeachersToEvaluate()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(userId, out int studentId))
                {
                    return Unauthorized("Սխալ userID.");
                }

                var teachers = await _context.Users
                    .Where(student => student.UserId == studentId)
                    .Include(student => student.Group)
                    .ThenInclude(group => group.GroupCourses)
                    .ThenInclude(gc => gc.Course)
                    .ThenInclude(course => course.Teacher)
                    .SelectMany(student => student.Group.GroupCourses.Select(gc => gc.Course.Teacher))
                    .GroupBy(t => t.UserId)
                    .Select(g => g.First())
                    .ToListAsync();

                var criteriaList = await _context.Criteria
                     .Where(c => c.Role == "Teacher")
                    .Select(c => new CriterionModel
                    {
                        Id = c.CriteriaId,
                        Name = c.CriteriaName
                    })
                    .ToListAsync();

                var teacherModels = teachers.Select(t => new EvaluationResponseModel
                {
                    Id = t.UserId,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    Description = t.Description,
                    IsEvaluated = _context.Evaluations.Any(e => e.EvaluateeId == t.UserId),
                    Criterias = criteriaList
                }).ToList();

                return Ok(teacherModels);
            }
            catch (Exception ex)
            {
                Logger.Error("GetTeachersToEvaluate: {0} : {1} ", DateTime.Now, ex.Message);
                return BadRequest();
            }
            finally
            {
                Logger.Info("{0} : Success GetTeachersToEvaluate ", DateTime.Now);
            }
        }
        [HttpPost("EvaluateTeacher")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> EvaluateTeacher([FromBody] EvaluationRequest evaluation)
        {
            try
            {
                var evaluatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                Check checking = new Check();
                string check = checking.CheckEvaluationByUser(evaluation, evaluatorId, "Teacher", _context);

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
                            Role = "Teacher",
                            EvaluationDate = DateTime.Now
                        }).ToList();

                    _context.Evaluations.AddRange(evaluationEntities);
                }
                await _context.SaveChangesAsync();

                return Ok("Գնահատման պրոցեսը հաջող է ընթացել!");
            }
            catch (Exception ex)
            {
                Logger.Error("EvaluateTeacher: {0} : {1} ", DateTime.Now, ex.Message);
                return BadRequest();
            }
            finally
            {
                Logger.Info("{0} : Success EvaluateTeacher ", DateTime.Now);
            }
        }
    }
}

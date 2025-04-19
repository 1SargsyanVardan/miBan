using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Models;
using WebApplication1.HelperClasses;
using AutoMapper;
using WebApplication1.Models.MyModels;
using System.Text.RegularExpressions;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebApplication1.Models.MyModels.Response;

namespace WebApplication1.Controllers
{
    //[Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly Check _check;
        public AdminController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _check = new Check();
        }
        [HttpGet("whoami")]
        [Authorize]
        public IActionResult WhoAmI()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(claims);
        }
        // 1. Օգտվողի գրանցում
        [HttpPost("Register")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public IActionResult RegisterUser([FromBody] UserModel userModel)
        {
            string check = _check.CheckUser(userModel, _context);
            if (check != string.Empty)
            {
                return BadRequest(check);
            }

            try
            {
                string code = new Random().Next(100000, 999999).ToString();
                userModel.VerificationCode = code;
                userModel.IsEmailConfirmed = false;

                var user = _mapper.Map<User>(userModel);
                user.PasswordHash = PasswordHelper.HashPassword(userModel.PasswordHash); // Հեշավորում ենք գաղտնաբառը

                EmailSender emailSender = new EmailSender();
                emailSender.SendVerificationEmail(userModel.Email,code);

                if (userModel.GroupId == 0)
                {
                    user.GroupId = null;
                }

                _context.Users.Add(user);
                _context.SaveChanges();

                return Ok("Օգտվողը հաջողությամբ գրանցվեց:");
            }
            catch (Exception ex)
            {
                // Լոգ անել պետք է այստեղ
                return StatusCode(500, "Սերվերի սխալ: Խնդրում ենք փորձել կրկին:");
            }
        }
        [HttpPost("VerifyEmail")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult VerifyEmail([FromBody] EmailVerificationModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
                return BadRequest("Օգտվողը գտնված չէ։");

            if (user.VerificationCode != model.Code)
                return BadRequest("Սխալ հաստատման կոդ։");

            user.IsEmailConfirmed = true;
            user.VerificationCode = null;
            _context.SaveChanges();

            return Ok("Էլ․ հասցեն հաստատվեց հաջողությամբ։");
        }

        [HttpPost("RegisterBalk")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RegisterUsers([FromBody] List<UserModel> userModels)
        {
            foreach (var userModel in userModels)
            {
                string check = _check.CheckUser(userModel, _context);
                if (check != string.Empty)
                {
                    continue;
                }

                var user = _mapper.Map<User>(userModel);
                user.PasswordHash = PasswordHelper.HashPassword(userModel.PasswordHash); // Հեշավորում ենք գաղտնաբառը

                _context.Users.Add(user);
                _context.SaveChanges();
            }
            return Ok("Օգտվողները հաջողությամբ գրանցվեցին:");
        }

        [HttpPost("ChangePassword")]
        [Authorize]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
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
                return Unauthorized("Ադմինը չի գտնվել։");
            }

            int adminId = int.Parse(userId);
            var admin = await _context.Users.FindAsync(adminId);

            if (admin == null)
            {
                return NotFound("Ադմինը գտնված չէ");
            }

            if (!PasswordHelper.VerifyPassword(admin.PasswordHash, model.OldPassword))
            {
                return BadRequest("Հին գաղտնաբառը սխալ է");
            }

            admin.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            return Ok("Գաղտնաբառը հաջողությամբ փոփոխվեց");
        }

        // 2. Ուսանողների ռեպորտների ստացում
        [HttpGet("Reports/Students")]
        [ProducesResponseType(typeof(List<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GenerateStudentReports()
        {
            var criteriaIds = await _context.Evaluations
                                    .Select(e => e.CriteriaId)
                                    .Distinct()
                                    .ToListAsync();

            var allEvaluations = new List<object>();
            var reports = new List<StudentReport>();

            foreach (var criteriaId in criteriaIds)
            {
                var studentEvaluations = await _context.Evaluations
                    .Where(e => e.Role == "Student" && e.CriteriaId == criteriaId)
                    .GroupBy(e => new { e.CriteriaId, e.EvaluateeId })
                    .Select(g => new
                    {
                        StudentID = g.Key.EvaluateeId,
                        TotalScores = g.Sum(e => e.Score),
                        AverageScore = g.Average(e => e.Score),
                    })
                    .OrderByDescending(e => e.TotalScores)
                    .ToListAsync();

                if (studentEvaluations.Any())
                {
                    allEvaluations.Add(new
                    {
                        CriteriaID = criteriaId,
                        Students = studentEvaluations
                    });

                    var studentReports = studentEvaluations.Select(evaluation => new StudentReport
                    {
                        ReportDate = DateTime.Now,
                        StudentId = evaluation.StudentID,
                        TotalScores = evaluation.TotalScores,
                        AverageScore = evaluation.AverageScore,
                        CriteriaId = criteriaId
                    }).ToList();

                    reports.AddRange(studentReports);
                }
            }

            if (reports.Any())
            {
                _context.StudentReports.AddRange(reports);
                await _context.SaveChangesAsync();

                var evaluationsToDelete = await _context.Evaluations
                    .Where(e => e.Role == "Student")
                    .ToListAsync();

                _context.Evaluations.RemoveRange(evaluationsToDelete);
                await _context.SaveChangesAsync();
            }

            var criteriaDict = _context.Criteria
                .ToDictionary(c => c.CriteriaId, c => c.CriteriaName);

            var usersDict = _context.Users
                 .Where(u => u.Role == "Student")
                .ToDictionary(u => u.UserId, u => u.FirstName + " " + u.LastName);

            var formattedReports = allEvaluations
                .Select(e => new
                {
                    CriteriaName = criteriaDict.TryGetValue((int)e.GetType().GetProperty("CriteriaID")?.GetValue(e), out var cName) ? cName : "Unknown",

                    Students = ((IEnumerable<dynamic>)e.GetType().GetProperty("Students")?.GetValue(e))
                        .Select(s => new
                        {
                            FullName = usersDict.TryGetValue((int)s.StudentID, out var fullName) ? fullName : "Unknown",
                            TotalScores = s.TotalScores,
                            AverageScore = s.AverageScore
                        })
                        .ToList()
                })
                .ToList();

            return Ok(formattedReports);
        }

        // 3. Դասախոսների ռեպորտների ստացում
        [HttpGet("Reports/Teachers")]
        [ProducesResponseType(typeof(List<User>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GenerateTeacherReports()
        {
            var criteriaIds = await _context.Evaluations
                                    .Select(e => e.CriteriaId)
                                    .Distinct()
                                    .ToListAsync();

            var allEvaluations = new List<object>();
            var reports = new List<TeacherReport>();

            foreach (var criteriaId in criteriaIds)
            {
                var teacherEvaluations = await _context.Evaluations
                    .Where(e => e.Role == "Teacher" && e.CriteriaId == criteriaId)
                    .GroupBy(e => new { e.CriteriaId, e.EvaluateeId })
                    .Select(g => new
                    {
                        TeacherID = g.Key.EvaluateeId,
                        TotalScores = g.Sum(e => e.Score),
                        AverageScore = g.Average(e => e.Score),
                    })
                    .OrderByDescending(e => e.TotalScores)
                    .ToListAsync();

                if (teacherEvaluations.Any())
                {
                    allEvaluations.Add(new
                    {
                        CriteriaID = criteriaId,
                        Students = teacherEvaluations
                    });

                    var teacherReports = teacherEvaluations.Select(evaluation => new TeacherReport
                    {
                        ReportDate = DateTime.Now,
                        TeacherId = evaluation.TeacherID,
                        TotalScores = evaluation.TotalScores,
                        AverageScore = evaluation.AverageScore,
                        CriteriaId = criteriaId
                    }).ToList();

                    reports.AddRange(teacherReports);
                }
            }

            if (reports.Any())
            {
                _context.TeacherReports.AddRange(reports);
                await _context.SaveChangesAsync();

                var evaluationsToDelete = await _context.Evaluations
                    .Where(e => e.Role == "Teacher")
                    .ToListAsync();

                _context.Evaluations.RemoveRange(evaluationsToDelete);
                await _context.SaveChangesAsync();
            }

            var criteriaDict = _context.Criteria
                .ToDictionary(c => c.CriteriaId, c => c.CriteriaName);

            var usersDict = _context.Users
                 .Where(u => u.Role == "Teacher")
                .ToDictionary(u => u.UserId, u => u.FirstName + " " + u.LastName);

            var formattedReports = allEvaluations
                .Select(e => new
                {
                    CriteriaName = criteriaDict.TryGetValue((int)e.GetType().GetProperty("CriteriaID")?.GetValue(e), out var cName) ? cName : "Unknown",

                    Teachers = ((IEnumerable<dynamic>)e.GetType().GetProperty("Teachers")?.GetValue(e))
                        .Select(s => new
                        {
                            FullName = usersDict.TryGetValue((int)s.StudentID, out var fullName) ? fullName : "Unknown",
                            TotalScores = s.TotalScores,
                            AverageScore = s.AverageScore
                        })
                        .ToList()
                })
                .ToList();

            return Ok(formattedReports);
        }

        // 4. Ուսանող կամ դասախոս ստանալ ըստ անունի և կարգավիճակի
        [HttpGet("User/Search")]
        [ProducesResponseType(typeof(List<User>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public IActionResult SearchUser(string name, string role)
        {
            var users = _context.Users
                .Where(u => (u.FirstName.Contains(name) || u.LastName.Contains(name)) && u.Role == role)
                .ToList();

            if (!users.Any())
            {
                return NotFound("Համապատասխան օգտվողներ չգտնվեցին:");
            }

            return Ok(users);
        }

        // 5. Ուսանող հեռացում ըստ ID-ի
        [HttpDelete("Delete-student/{studentId}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public IActionResult DeleteStudent(int studentId)
        {
            var student = _context.Users.FirstOrDefault(u => u.UserId == studentId && u.Role == "Student");
            if (student == null)
            {
                return NotFound($"{studentId}-ով ուսանող չի գտնվել։");
            }

            _context.Users.Remove(student);
            _context.SaveChanges();

            return Ok($"{studentId}-ով ուսանողը հաջողությամբ ջնջվել է։");
        }

        // 6. Դասախոսի հեռացում ըստ ID-ի
        [HttpDelete("DeleteTeacher/{teacherId}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public IActionResult DeleteTeacher(int teacherId)
        {
            var teacher = _context.Users.FirstOrDefault(u => u.UserId == teacherId && u.Role == "Teacher");
            if (teacher == null)
            {
                return NotFound($"{teacherId}-ով դասախոսը չի գտնվել։");
            }

            _context.Users.Remove(teacher);
            _context.SaveChanges();

            return Ok($"{teacherId}-ով ուսանողը հաջողությամբ ջնջվել է։");
        }

        // 7. Դասախոսների ստացում
        [HttpGet("Teachers")]
        [ProducesResponseType(typeof(List<User>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public IActionResult GetTeachers()
        {
            var users = _context.Users.Where(u => u.Role == "Teacher").ToList();

            if (users == null || users.Count == 0)
            {
                return NotFound("Ոչ մի դասախոս չգտնվեց:");
            }

            return Ok(users);
        }
        // 8. Ուսանողների ստացում
        [HttpGet("Students")]
        [ProducesResponseType(typeof(List<UserGetResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public IActionResult GetStudents()
        {
            var users = _context.Users.Where(u => u.Role == "Student").ToList();

            List<UserGetResponse> userResponse = new();
            foreach (var user in users)
            {
                userResponse.Add(_mapper.Map<UserGetResponse>(user));
            }
            //var user = _mapper.Map<UserGetResponse>(users);

            if (userResponse == null || userResponse.Count == 0)
            {
                return NotFound("Ոչ մի ուսանող չգտնվեց:");
            }

            return Ok(userResponse);
        }
        // 9. Ադմինների ստացում
        [HttpGet("Admins")]
        [ProducesResponseType(typeof(List<User>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public IActionResult GetAdmins()
        {
            var users = _context.Users.Where(u => u.Role == "Admin").ToList();

            if (users == null || users.Count == 0)
            {
                return NotFound("Ոչ մի ադմին չգտնվեց:");
            }

            return Ok(users);
        }
    }

}

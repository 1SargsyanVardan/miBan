using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebApplication1.Models;
using WebApplication1.Models.MyModels.Request;

namespace WebApplication1.HelperClasses
{
    public class Check
    {
        public bool EmailCheck(string email)
        {
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return false;
            }
            else 
                return true;
        }

        public string CheckUser(UserRegisterRequest userModel, AppDbContext _context)
        {
            if (_context.Users.Any(u => u.Email == userModel.Email))
            {
                return "Օգտվողն արդեն գրանցված է այս էլ. հասցեով:";
            }

            if (!Regex.IsMatch(userModel.PasswordHash, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))
            {
                return "Գաղտնաբառը պետք է պարունակի առնվազն 8 նիշ, 1 մեծատառ, 1 փոքրատառ և 1 թիվ:";
            }

            if (!Regex.IsMatch(userModel.FirstName, @"^[\p{L}]+$") || !Regex.IsMatch(userModel.LastName, @"^[\p{L}]+$"))
            {
                return "Անունն ու ազգանունը պետք է պարունակեն միայն տառեր:";
            }

            var validRoles = new[] { "Student", "Teacher", "Admin" };
            if (!validRoles.Contains(userModel.Role))
            {
                return "Սխալ դեր: Հնարավոր դերերը՝ Student, Teacher, Admin:(Անպայման մեծատառ առաջին տառը)";
            }
            if (userModel.Role == "Student" && userModel.GroupId == 0)
            {
                return "Ուսանողը պետք է գրանցված լինի որևէ խմբում։ Կխնդրենք վերանայել";
            }
            if (userModel.GroupId != 0)
            {
                var groups = _context.Groups.
                    Select(c => c.GroupId)
                    .ToList();
                if (!groups.Contains(userModel.GroupId))
                    return "Տվյալ GroupId-ով խումբ գոյություն չունի";
            }

            return string.Empty;
        }

        public string CheckEvaluationByUser(EvaluationRequest evaluation, string evaluatorId, string whomEvaluate, AppDbContext _context)
        {

            if (!int.TryParse(evaluatorId, out int parsedEvaluatorId))
            {
                return "Սխալ evaluatorID.";
            }

            var evaluatee = _context.Users
                .FirstOrDefaultAsync(u => u.UserId == evaluation.EvaluateeID && u.Role == whomEvaluate);

            if (evaluatee == null)
            {
                return "Տվյալ ID-ով դասախոս չի գտնվել.";
            }

            var submitedEvaluation = _context.Evaluations
                                    .Where(c => c.EvaluatorId == parsedEvaluatorId && c.EvaluateeId == evaluatee.Result.UserId)
                                    .Select(e => new {
                                        e.EvaluationId,
                                        e.EvaluatorId,
                                        e.EvaluateeId,
                                        e.CriteriaId,
                                        e.Score,
                                        e.Role,
                                        e.EvaluationDate
                                    })
                                    .ToList();
            if (submitedEvaluation.Count != 0)
            {
                return "Տվյալ ID-ով դասախոսին Դուք արդեն գնահատել եք.";
            }

            foreach (var score in evaluation.Scores)
            {
                if (score.Score < 1 || score.Score > 10)
                    return "Գնահատականները պետք է լինեն 1ից 10 միջակայքի թվեր.";
            }

            var courseId = _context.Courses
                .Where(c => c.TeacherId == evaluation.EvaluateeID)
                .Select(c => c.CourseId)
                .FirstOrDefaultAsync();

            var criteriaIds = evaluation.Scores.Select(s => s.CriteriaID).ToList();

            var validCriteriaIds = _context.Criteria
                .Where(c => criteriaIds.Contains(c.CriteriaId) && c.Role == whomEvaluate)
                .Select(c => c.CriteriaId)
                .ToList();

            if (validCriteriaIds.Count != criteriaIds.Count)
            {
                return "Սխալ criteriaID.";
            }
            return string.Empty;
        }
    }
}

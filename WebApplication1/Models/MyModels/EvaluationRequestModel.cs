namespace WebApplication1.Models.MyModels
{
    public class EvaluationRequestModel
    {
        public int EvaluateeID { get; set; }  // gnahatvoxi ID
        public List<EvaluationScoreRequest> Scores { get; set; }
       
    }
    public class EvaluationScoreRequest
    {
        public int CriteriaID { get; set; }
        public int Score { get; set; }
    }
}

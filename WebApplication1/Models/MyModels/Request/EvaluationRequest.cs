﻿namespace WebApplication1.Models.MyModels.Request
{
    public class EvaluationRequest
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

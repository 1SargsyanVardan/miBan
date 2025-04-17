using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Evaluation
{
    public int EvaluationId { get; set; }

    public int CriteriaId { get; set; }

    public int EvaluatorId { get; set; }

    public int EvaluateeId { get; set; }

    public int Score { get; set; }

    public string? Role { get; set; }

    public DateTime? EvaluationDate { get; set; }

    public virtual Criterion Criteria { get; set; } = null!;

    public virtual User Evaluatee { get; set; } = null!;

    public virtual User Evaluator { get; set; } = null!;
}

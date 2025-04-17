namespace WebApplication1.Models.MyModels
{
    public class EvaluationModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public bool IsEvaluated { get; set; }
        public List<CriterionModel> Criterias { get; set; }
    }
    public class CriterionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

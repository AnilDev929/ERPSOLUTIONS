namespace SchoolERP.Models.DTOS
{
    public class GradeRuleDto
    {
        public int GradeRuleID { get; set; }
        public string RuleName { get; set; }
        public decimal MinScore { get; set; }
        public decimal MaxScore { get; set; }
        public string GradeLetter { get; set; }
        public decimal? GradePoint { get; set; }
        public string ResultStatus { get; set; }
        public int? DisplayOrder { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string ColorCode { get; set; }
    }
}

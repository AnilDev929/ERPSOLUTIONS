namespace SchoolERP.Models.DTOS
{
    public class ClassPendingFeeDto
    {
        public string ClassName { get; set; }

        public int PendingStudents { get; set; }

        public decimal PendingAmount { get; set; }

        public int PendingPercentage { get; set; }
    }

    public class StudentPendingFeeDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string AdmissionNo  { get; set; }
        public string MobileNo { get; set; }
        public string ClassName { get; set; }
        public decimal ToatlFees { get; set; }
        public decimal PendingAmount { get; set; }

        public DateTime DueDate { get; set; }

        public bool IsOverdue { get; set; }
    }
}

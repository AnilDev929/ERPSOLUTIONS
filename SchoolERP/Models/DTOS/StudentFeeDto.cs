namespace SchoolERP.Models.DTOS
{
    public class StudentFeeDto
    {
        public int MonthNum { get; set; }
        public string MonthName { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime FinalDueDate { get; set; }

        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public decimal Fine { get; set; }

        public string Status { get; set; }
    }
}

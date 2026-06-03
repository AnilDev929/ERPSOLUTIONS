namespace SchoolERP.Models.ViewModels
{
    public class StudentPaymentVM
    {
        public int StudentId { get; set; }
        public string PaymentOption { get; set; }
        public decimal DiscountApplied { get; set; }
        public decimal TotalFee { get; set; }

        public int? Months { get; set; }
        public int? PaidMonths { get; set; }
        public decimal? MonthlyAmount { get; set; }
        public decimal? RemainingAmount { get; set; }

        public bool IncludeTransport { get; set; }
    }
}

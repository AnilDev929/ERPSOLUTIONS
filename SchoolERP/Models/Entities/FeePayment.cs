using SchoolERP.Enum;

namespace SchoolERP.Models.Entities
{
    public class FeePayment
    {
        public int PaymentID { get; set; }

        public int FeeId { get; set; }
        public int InstallmentNo { get; set; } = 0;
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal LateFine { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public int FeeMonth { get; set; } = 0;
        public int FeeYear { get; set; } = 0;
        public string PaymentMethod { get; set; } // Cash, UPI, Bank Transfer
        //public PaymentMethod PaymentMethod { get; set; }
        public string TransactionID { get; set; }

        public string? Remarks { get; set; }

        // Navigation property (optional but recommended)
        public StudentFee StudentFee { get; set; }
    }
}

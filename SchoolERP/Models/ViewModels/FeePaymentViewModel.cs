using SchoolERP.Enum;
using SchoolERP.Models.Entities;

namespace SchoolERP.Models.ViewModels
{

    public class FeePaymentViewModel
    {
        public int PaymentId { get; set; }

        public int FeeId { get; set; }

        public StudentInfoViewModel StudentInfoView { get; set; }

        public decimal TotalFee { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal PendingAmount { get; set; }

        public int InstallmentNo { get; set; }

        public decimal MonthlyAmount { get; set; }

        public DateTime DueDate { get; set; }

        // ADD THIS
        //public string SelectedPaymentMethod { get; set; }
        public PaymentMethod SelectedPaymentMethod { get; set; }
        public decimal LateFine { get; set; }

        public decimal ExemptAmount { get; set; }

        public decimal FinalAmount { get; set; }
        public bool IsFeeWaived { get; set; }
        public string? WaivedReason { get; set; }
        public List<FeePayment> PreviousPayments { get; set; }
    }
}

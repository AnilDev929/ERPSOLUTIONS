using SchoolERP.Models.DTOS;
using SchoolERP.Models.Entities;

namespace SchoolERP.Models.ViewModels
{
    public class StudentFeeDetailViewModel
    {
        public StudentInfoViewModel Student { get; set; }
        public StudentFee Fee { get; set; }
        public List<StudentFeeDto> Installments { get; set; }
        public List<FeePayment> PaymentHis { get; set; }

        public decimal TotalFee { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal LateFine { get; set; }
        public string Status { get; set; }
        public int PaidMonths { get; set; }
        public bool IsInstallment { get; set; }
    }
}

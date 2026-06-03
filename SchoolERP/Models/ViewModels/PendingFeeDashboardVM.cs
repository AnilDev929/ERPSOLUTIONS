using SchoolERP.Models.DTOS;

namespace SchoolERP.Models.ViewModels
{
    public class PendingFeeDashboardVM
    {
        public decimal TotalPending { get; set; }

        public int TotalStudents { get; set; }

        public int TotalClasses { get; set; }
        public int OverdueCount { get; set; }
        public decimal CollectionPercentage { get; set; }

        public List<ClassPendingFeeDto> ClassWisePending { get; set; }

        public List<StudentPendingFeeDto> PendingStudents { get; set; }

    }
}

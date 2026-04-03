namespace ERP_SOLUTIONS.Models.DTOS
{
    public class CreateStudentResultDto
    {
        public int Status { get; set; }
        public string Message { get; set; }

        public string UserName { get; set; }   // ✅ nullable
        public int UserID { get; set; }        // ✅ nullable
        public int StudentID { get; set; }     // ✅ nullable
        public string RollNumber { get; set; } // ✅ nullable
    }
}

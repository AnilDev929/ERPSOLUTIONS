using Microsoft.EntityFrameworkCore;

namespace SchoolERP.Models.DTOS
{
    [Keyless]
    public class CreateStudentResultDto
    {
        public int Status { get; set; }
        public string Message { get; set; }

        public string UserName { get; set; }   
        public int UserID { get; set; }        
        public int StudentID { get; set; }     
        public string RollNumber { get; set; } 
    }
}

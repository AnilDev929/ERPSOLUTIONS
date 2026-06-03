using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.ViewModels
{
    public class StudentMarkRowVM
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }

        [Range(0, 100, ErrorMessage = "Marks must be between 0 and 100")]
        public int? MarksObtained { get; set; }
    }
}

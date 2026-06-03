using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Models.ViewModels
{
    public class ExamTimeSlotVM
    {
        public int Id { get; set; }

        // Exam Type
        [Required(ErrorMessage = "Exam type is required.")]
        [Display(Name = "Exam Type")]
        public int? ExamId { get; set; }

        // Exam Date
        [Required(ErrorMessage = "Exam date is required.")]
        [Display(Name = "Exam Date")]
        [DataType(DataType.Date)]
        public DateTime ExamDate { get; set; }

        // Time Slot
        [Required(ErrorMessage = "Start time is required.")]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        // Room Details
        //[Required(ErrorMessage = "Floor number is required.")]
        [Display(Name = "Floor Name")]
        public string? FloorNumber { get; set; }

        //[Required(ErrorMessage = "Room number is required.")]
        //[StringLength(20)]
        [Display(Name = "Room Number")]
        public string? RoomNumber { get; set; }

        //// Slot Information
        //[Required(ErrorMessage = "Slot name is required.")]
        //[StringLength(100)]
        //[Display(Name = "Slot Name")]
        //public string? SlotName { get; set; }

        // Status
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Audit Fields
        [Display(Name = "Created On")]
        public DateTime? CreatedOn { get; set; } = DateTime.Now;

        [Display(Name = "Updated On")]
        public DateTime? UpdatedOn { get; set; }
    }
}

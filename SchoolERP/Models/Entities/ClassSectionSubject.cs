namespace SchoolERP.Models.Entities
{
    public class ClassSectionSubject
    {
        public int Id { get; set; }

        // 🔗 Link to ClassSection (recommended approach)
        public int ClassSectionId { get; set; }
        public ClassSection ClassSection { get; set; }

        // 📘 Subject mapping
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        // 👨‍🏫 Teacher mapping
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }

        //// 🔥 Optional: extra control fields (recommended)
        //public bool IsActive { get; set; } = true;

        //public DateTime CreatedAt { get; set; } = DateTime.Now;

        //public DateTime? UpdatedAt { get; set; }
    }
}

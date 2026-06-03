namespace SchoolERP.Models.Entities
{
    public class ParentStudent
    {
        public int Id { get; set; }

        public int ParentId { get; set; }

        public int StudentId { get; set; }
        public string RelationshipType { get; set; }

        // Navigation Properties
        public virtual Parent Parent { get; set; }

        public virtual Student Student { get; set; }
    }
}

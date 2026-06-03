namespace SchoolERP.Models.DTOS
{
    public class MappingRequest
    {
        public int ClassId { get; set; }
        public int SectionId { get; set; }
        public List<MappingItem> Mappings { get; set; }
    }
}


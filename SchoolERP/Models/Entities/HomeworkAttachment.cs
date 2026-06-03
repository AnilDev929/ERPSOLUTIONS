namespace SchoolERP.Models.Entities
{
    public class HomeworkAttachment
    {
        public int HomeworkAttachmentId { get; set; }
        public int HomeworkId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}

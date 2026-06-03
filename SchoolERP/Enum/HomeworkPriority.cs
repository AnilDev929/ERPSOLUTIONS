namespace SchoolERP.Enum
{
    //public class HomeworkPriority
    //{
    //}

    public enum HomeworkPriority : byte
    {
        Low = 1,
        Medium = 2,
        High = 3
    }

    public enum HomeworkStatus : byte
    {
        Active = 1,
        Completed = 2,
        Archived = 3
    }

    public enum SubmissionStatus : byte
    {
        Pending = 1,
        Submitted = 2,
        Reviewed = 3,
        Rejected = 4
    }

    public enum HomeworkType : byte
    {
        Normal = 1,
        Project = 2,
        Worksheet = 3,
        Practical = 4
    }
}

namespace hr_crm.Entities
{
    public class LearningCourse
    {
            public int Id { get; set; }

            public int  UserId { get; set; }

            public string? CourseName { get; set; }

            public string? Description { get; set; }

            public string? Role { get; set; }

            public DateTime AssignedDate { get; set; }

            public DateTime DueDate { get; set; }

            public int Progress { get; set; }

            public string? Status { get; set; }
    }
}


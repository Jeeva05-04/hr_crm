namespace hr_crm.DTO
{
    public class LearningCourseDto
    {
            public int UserId { get; set; }

            public string?   CourseName { get; set; }

            public string? Description { get; set; }

            public string? Role { get; set; }

            public DateTime AssignedDate { get; set; }

            public DateTime DueDate { get; set; }
        
           public int Progress { get; set; }

           public string? Status { get; set; }

    }
}


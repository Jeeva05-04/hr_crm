namespace hr_crm.DTO
{
    public class LearningCourseDto
    {
            public int EmployeeId { get; set; }

            public string?   CourseName { get; set; }

            public string? Description { get; set; }

            public string? Role { get; set; }

            public DateTime AssignedDate { get; set; }

            public DateTime DueDate { get; set; }
        
    }
}


namespace hr_crm.Models
{
    public class TodoCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int AssignedTo { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }   // Pending / In Progress / Completed
    }
}


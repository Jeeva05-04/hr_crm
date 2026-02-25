namespace hr_crm.DTO
{
    public class ShiftCreateDto
    {
        public string ShiftName { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public int DepartmentId { get; set; }
    }
}
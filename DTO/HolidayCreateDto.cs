namespace hr_crm.DTO
{
    public class HolidayCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Type { get; set; } = "National";  // National | Company
    }
}

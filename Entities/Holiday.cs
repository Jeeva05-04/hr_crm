namespace hr_crm.Entities
{
    public class Holiday
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;  // National | Company
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

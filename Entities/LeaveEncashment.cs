namespace hr_crm.Entities
{
    public class LeaveEncashment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int EncashedDays { get; set; }
        public decimal AmountPaid { get; set; }
        public int Year { get; set; }
        public DateTime ProcessedDate { get; set; }
        public int ProcessedBy { get; set; }
    }
}

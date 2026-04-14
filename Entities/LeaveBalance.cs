namespace hr_crm.Entities
{
    public class LeaveBalance
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string LeaveType { get; set; } = string.Empty;  // Sick | Casual | Earned
        public int TotalAllowed { get; set; }   // total days allowed per year
        public int UsedDays { get; set; }
        public int Year { get; set; }

        public int RemainingDays => TotalAllowed - UsedDays;
    }
}

namespace hr_crm.Entities
{
    public class OnboardingInvite
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public string? EmployeeEmail { get; set; }
        public string? EmployeeName { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public int? OnboardingId { get; set; }
    }
}

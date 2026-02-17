namespace hr_crm.Models
{
    public class RecruitmentCreateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AppliedPosition { get; set; }
        public int DepartmentId { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; }   // Applied / Interview / Selected / Rejected
        public string Source { get; set; }   // LinkedIn / Referral / Website
    }
}


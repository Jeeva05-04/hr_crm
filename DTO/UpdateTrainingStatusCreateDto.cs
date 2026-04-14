namespace hr_crm.DTO
{
    public class UpdateTrainingStatusCreateDto
    {

        public string Status { get; set; }

        public int Progress { get; set; }

        public DateTime? CompletionDate { get; set; }

        public bool IsCertified { get; set; }

        public int? Score { get; set; }

        public string? Feedback { get; set; }
    }
}



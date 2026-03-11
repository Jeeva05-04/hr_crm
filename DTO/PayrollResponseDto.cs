namespace hr_crm.DTO
{
    public class PayrollResponseDto
    {
        public int PayrollId { get; set; }
        public int UserId { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime Month { get; set; }  
        public int Year { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
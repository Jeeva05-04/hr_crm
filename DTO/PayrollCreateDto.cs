namespace hr_crm.DTO
{
    public class PayrollCreateDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal BasicSalary { get; set; }
    }
}

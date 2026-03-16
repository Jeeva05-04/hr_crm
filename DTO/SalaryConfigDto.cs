namespace hr_crm.DTO
{
    public class SalaryConfigDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal BasicSalary { get; set; }
    }
}

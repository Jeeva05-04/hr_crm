namespace hr_crm.DTO
{
    public class PayrollCreateDto
    {
       
            public int UserId { get; set; }
            public decimal BasicSalary { get; set; }
        public string  UserName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
          
    }
}

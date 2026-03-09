namespace hr_crm.DTO
{
    public class PayrollCreateDto
    {
       
            public int EmployeeId { get; set; }
            public decimal BasicSalary { get; set; }
            public decimal Allowances { get; set; }
            public decimal Deductions { get; set; }
            public DateTime PayrollMonth { get; set; }
        
    }
}

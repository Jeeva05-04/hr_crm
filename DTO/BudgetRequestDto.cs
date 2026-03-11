namespace hr_crm.DTO;

public class BudgetRequestDto
{
    public int DepartmentId { get; set; }

    public decimal RequestedAmount { get; set; }

    public string Reason { get; set; } = string.Empty;
}
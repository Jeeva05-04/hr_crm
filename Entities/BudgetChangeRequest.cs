namespace hr_crm.Entities;

public class BudgetChangeRequest
{
    public int BudgetChangeRequestId { get; set; }

    public int DepartmentId { get; set; }

    public decimal RequestedAmount { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";
    // Pending / ApprovedByHead / ApprovedByFinance / Rejected

    public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
}
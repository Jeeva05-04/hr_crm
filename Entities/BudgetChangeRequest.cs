public class BudgetChangeRequest
{
    public int BudgetChangeRequestId { get; set; }

    public int DepartmentId { get; set; }

    public decimal RequestedAmount { get; set; }

    public string Reason { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime RequestDate { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }
}
using System;
using System.Collections.Generic;

namespace hr_crm.Entities;

public partial class Knowledge
{
    public int BranchId { get; set; }

    public string? RecordType { get; set; }

    public string? Code { get; set; }

    public string Title { get; set; } = null!;

    public string? Category { get; set; }

    public string? SubCategory { get; set; }

    public string? Summary { get; set; }

    public string? ApprovalStatus { get; set; }

    public string? ApporvedBy { get; set; }

    public string? Visibility { get; set; }

    public string? Status { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Branch Branch { get; set; } = null!;
}

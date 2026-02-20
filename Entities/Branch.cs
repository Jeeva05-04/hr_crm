using System;
using System.Collections.Generic;

namespace hr_crm.Entities;

public partial class Branch
{
    public int BranchId { get; set; }

    public string BranchName { get; set; } = null!;

    public string Location { get; set; } = null!;

    public string? Status { get; set; }

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}

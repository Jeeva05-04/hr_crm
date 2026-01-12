using System;
using System.Collections.Generic;

namespace hr_crm.Entities;

public partial class Project
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string Duration { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int ManagerId { get; set; }

    public int DepartmentId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual Employee Manager { get; set; } = null!;
}

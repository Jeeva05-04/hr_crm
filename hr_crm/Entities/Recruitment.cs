using System;
using System.Collections.Generic;

namespace hr_crm.Entities;

public partial class Recruitment
{
    public int CandidateId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string AppliedPosition { get; set; } = null!;

    public int DepartmentId { get; set; }

    public DateOnly ApplicationDate { get; set; }

    public string Status { get; set; } = null!;

    public string Source { get; set; } = null!;

    public virtual Department Department { get; set; } = null!;
}

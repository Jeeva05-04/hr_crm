using System;
using System.Collections.Generic;

namespace hr_crm.Entities;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string EmergencyContact { get; set; } = null!;

    public int DepartmentId { get; set; }

    public string Designation { get; set; } = null!;

    public string Address { get; set; } = null!;

    public DateOnly DateOfJoining { get; set; }

    public decimal Salary { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<TodoTask> TodoTasks { get; set; } = new List<TodoTask>();
}

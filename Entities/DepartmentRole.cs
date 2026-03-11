namespace hr_crm.Entities;

public class DepartmentRole
{
    public int DepartmentRoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string RequiredSkillLevel { get; set; } = string.Empty;

    public string PerformanceLevel { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    // 🔥 Add this navigation property
    public ICollection<UserDepartmentRole> UserDepartmentRoles { get; set; }
        = new List<UserDepartmentRole>();
}
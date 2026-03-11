namespace hr_crm.DTO;

public class DepartmentRoleCreateDto
{
    public string RoleName { get; set; } = string.Empty;

    public string RequiredSkillLevel { get; set; } = string.Empty;

    public string PerformanceLevel { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
}
namespace hr_crm.Entities;

public class UserDepartmentRole
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int DepartmentRoleId { get; set; }

    public DepartmentRole DepartmentRole { get; set; } = null!;
}
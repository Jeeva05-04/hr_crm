using hr_crm.Entities;

public class Shift
{
    public int ShiftId { get; set; }

    public string ShiftName { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public int DepartmentId { get; set; }

    public Department Department { get; set; }
}
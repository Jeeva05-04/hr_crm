using hr_crm.Entities;

namespace hr_crm.Entities
{
    public class OvertimePolicy
    {
        public int OvertimePolicyId { get; set; }

        public int DepartmentId { get; set; }

        public double StandardDailyHours { get; set; } = 8;

        public double MaxWeeklyOvertimeHours { get; set; }

        public Department Department { get; set; }
    }
}
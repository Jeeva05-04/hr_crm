namespace hr_crm.DTO.Overtime
{
    public class OvertimePolicyResponseDto
    {
        public int OvertimePolicyId { get; set; }
        public int DepartmentId { get; set; }
        public double StandardDailyHours { get; set; }
        public double MaxWeeklyOvertimeHours { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace hr_crm.DTO.Overtime
{
    public class OvertimePolicyCreateDto
    {
        [Required]
        public int DepartmentId { get; set; }

        [Required]
        [Range(1, 24)]
        public double StandardDailyHours { get; set; }

        [Required]
        [Range(1, 100)]
        public double MaxWeeklyOvertimeHours { get; set; }
    }
}
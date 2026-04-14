using System.ComponentModel.DataAnnotations;

namespace hr_crm.DTO.Overtime
{
    public class OvertimePolicyUpdateDto
    {
        [Required]
        public double StandardDailyHours { get; set; }

        [Required]
        public double MaxWeeklyOvertimeHours { get; set; }
    }
}
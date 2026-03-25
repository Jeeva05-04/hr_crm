using System.ComponentModel.DataAnnotations;

namespace hr_crm.DTO
{
    public class LeaveTypeDto
    {
        public int LeaveTypeId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
    }
}

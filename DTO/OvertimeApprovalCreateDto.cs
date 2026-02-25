using System;
using System.ComponentModel.DataAnnotations;

namespace hr_crm.DTO.Overtime
{
    public class OvertimeApprovalCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }
    }
}
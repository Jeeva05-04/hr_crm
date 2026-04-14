using System;
using System.ComponentModel.DataAnnotations;

namespace hr_crm.DTO.Overtime
{
    public class OvertimeApprovalUpdateDto
    {
        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        public bool IsApproved { get; set; }
    }
}
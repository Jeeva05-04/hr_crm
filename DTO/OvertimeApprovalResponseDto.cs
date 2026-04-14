using System;

namespace hr_crm.DTO.Overtime
{
    public class OvertimeApprovalResponseDto
    {
        public int OvertimeApprovalId { get; set; }
        public int UserId { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsApproved { get; set; }
    }
}
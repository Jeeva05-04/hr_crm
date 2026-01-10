using System.ComponentModel.DataAnnotations;

namespace hr_crm.Models
{
    public class ProjectCreateDto
    {
        [Required]
        [StringLength(150)]
        public string ProjectName { get; set; }

        [Required]
        [StringLength(50)]
        public string Duration { get; set; }   // "3 Months", "6 Weeks"

        [Required]
        public string Status { get; set; }     // Planned / Active / Completed

        [Required]
        public int ManagerId { get; set; }     // employee_id

        [Required]
        public int DepartmentId { get; set; }
    }
}


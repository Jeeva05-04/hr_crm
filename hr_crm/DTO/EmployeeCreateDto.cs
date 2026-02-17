namespace hr_crm.Models
{
    public class EmployeeCreateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string EmergencyContact { get; set; }
        public int DepartmentId { get; set; }
        public string Designation { get; set; }
        public string Address { get; set; }
        public decimal Salary { get; set; }

        public string status { get; set; }

    }
}

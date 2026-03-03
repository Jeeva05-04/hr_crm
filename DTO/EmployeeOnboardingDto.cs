public class EmployeeOnboardingDto
{
    // Personal
    public string FullName { get; set; }
    public DateTime DateOfJoining { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; }
    public string MobileNumber { get; set; }

    public string BloodGroup { get; set; }
    public string MaritalStatus { get; set; }

    public string? SpouseName { get; set; }
    public DateTime? SpouseDOB { get; set; }
    public string? ChildrenDetails { get; set; }

    public string FatherName { get; set; }
    public DateTime FatherDOB { get; set; }

    public string MotherName { get; set; }
    public DateTime MotherDOB { get; set; }

    public string PAN { get; set; }
    public string AadharNumber { get; set; }

    public string EmergencyContactName { get; set; }
    public string EmergencyContactRelationship { get; set; }

    public string TemporaryAddress { get; set; }
    public string PermanentAddress { get; set; }

    // Work
    public string PreviousCompanyDetails { get; set; }
    public string OfferedDesignation { get; set; }
    public decimal OfferedSalaryNTH { get; set; }
    public decimal OfferedMonthlyCTC { get; set; }
    public decimal OfferedYearlyCTC { get; set; }
    public string TotalExperience { get; set; }
    public string LastCompanyPFNumber { get; set; }
    public string LastCompanyUAN { get; set; }

    // Bank
    public string BankName { get; set; }
    public string AccountNumber { get; set; }
    public string IFSC { get; set; }
    public string BranchName { get; set; }

    // Office
    public string OfficeEmail { get; set; }
    public string OfficeMobileNumber { get; set; }
    public string LaptopSerialNumber { get; set; }

    // FILES
    public IFormFile? PreviousCompanyPayslip { get; set; }
    public IFormFile? AadharCard { get; set; }
    public IFormFile? PANCard { get; set; }
    public IFormFile? BankStatement { get; set; }
    public IFormFile? BankPassbook { get; set; }
    public List<IFormFile>? ParentAadhar { get; set; }
    public IFormFile? ExperienceLetter { get; set; }
    public IFormFile? AcceptanceLetter { get; set; }
    public IFormFile? HighestQualificationDocument { get; set; }
    public IFormFile? LaptopImage { get; set; }
}
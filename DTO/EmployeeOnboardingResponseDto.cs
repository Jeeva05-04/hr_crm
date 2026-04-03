namespace hr_crm.DTO
{
    public class EmployeeOnboardingResponseDto
    {
        public int EmployeeOnboardingId { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public string? BloodGroup { get; set; }
        public string? MaritalStatus { get; set; }

        public string? SpouseName { get; set; }
        public DateTime? SpouseDOB { get; set; }
        public string? ChildrenDetails { get; set; }

        public string? FatherName { get; set; }
        public DateTime? FatherDOB { get; set; }
        public bool? IsFatherDeceased { get; set; }
        public DateTime? FatherDOD { get; set; }
        public int? FatherAge { get; set; }

        public string? MotherName { get; set; }
        public DateTime? MotherDOB { get; set; }
        public bool? IsMotherDeceased { get; set; }
        public DateTime? MotherDOD { get; set; }
        public int? MotherAge { get; set; }

        public string? PAN { get; set; }
        public string? AadharNumber { get; set; }

        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelationship { get; set; }

        public string? TemporaryAddress { get; set; }
        public string? PermanentAddress { get; set; }

        // Work / offer
        public string? PreviousCompanyDetails { get; set; }
        public string? OfferedDesignation { get; set; }
        public decimal? OfferedSalaryNTH { get; set; }
        public decimal? OfferedMonthlyCTC { get; set; }
        public decimal? OfferedYearlyCTC { get; set; }
        public string? TotalExperience { get; set; }
        public string? LastCompanyPFNumber { get; set; }
        public string? LastCompanyUAN { get; set; }
        public string? PreviousCompanyPayslipPath { get; set; }

        // Bank
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? IFSC { get; set; }
        public string? BranchName { get; set; }

        // Documents paths
        public string? AadharCardPath { get; set; }
        public string? PANCardPath { get; set; }
        public string? BankStatementPath { get; set; }
        public string? BankPassbookPath { get; set; }
        public string? ParentAadharPath { get; set; }
        public string? HighestQualificationDocumentPath { get; set; }
        public string? ExperienceLetterPath { get; set; }
        public string? AcceptanceLetterPath { get; set; }

        // Laptop / office
        public string? OfficeEmail { get; set; }
        public string? OfficeMobileNumber { get; set; }
        public string? LaptopSerialNumber { get; set; }
        public string? LaptopImagePath { get; set; }

        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}

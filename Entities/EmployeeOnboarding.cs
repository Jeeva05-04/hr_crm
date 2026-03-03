public class EmployeeOnboarding
{
    public int EmployeeOnboardingId { get; set; }

    // ===== PAGE 1 - PERSONAL =====
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
    public bool IsFatherDeceased { get; set; }
    public DateTime? FatherDOD { get; set; }
    public int? FatherAge { get; set; }

    public string MotherName { get; set; }
    public DateTime MotherDOB { get; set; }
    public bool IsMotherDeceased { get; set; }
    public DateTime? MotherDOD { get; set; }
    public int? MotherAge { get; set; }

    public string PAN { get; set; }
    public string AadharNumber { get; set; }

    public string EmergencyContactName { get; set; }
    public string EmergencyContactRelationship { get; set; }

    public string TemporaryAddress { get; set; }
    public string PermanentAddress { get; set; }

    // ===== PAGE 3 - BANK =====
    public string BankName { get; set; }
    public string AccountNumber { get; set; }
    public string IFSC { get; set; }
    public string BranchName { get; set; }

    // ===== PAGE 5 - OFFICE =====
    public string OfficeEmail { get; set; }
    public string OfficeMobileNumber { get; set; }
    public string LaptopSerialNumber { get; set; }
    public string? LaptopImagePath { get; set; }

    // SYSTEM
    public string Status { get; set; } = "Submitted";
    public DateTime CreatedDate { get; set; }
}
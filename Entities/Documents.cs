public class EmployeeOnboardingDocuments
{
    public int Id { get; set; }

    public int EmployeeOnboardingId { get; set; }

    public string? AadharCardPath { get; set; }
    public string? PANCardPath { get; set; }
    public string? BankStatementPath { get; set; }
    public string? BankPassbookPath { get; set; }

    public string? ParentAadharPath { get; set; }

    public string? HighestQualificationDocumentPath { get; set; }

    public string? ExperienceLetterPath { get; set; }
    public string? AcceptanceLetterPath { get; set; }
}
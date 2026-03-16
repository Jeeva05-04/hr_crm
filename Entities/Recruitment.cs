using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace hr_crm.Entities;

public partial class Recruitment
{
    [Key]
    public int CandidateId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string AppliedPosition { get; set; } = null!;

    public int DepartmentId { get; set; }

    public DateOnly ApplicationDate { get; set; }

    // Status pipeline: Applied → Screening → InterviewScheduled → Selected → Offered → Onboarded | Rejected
    public string Status { get; set; } = "Applied";

    public string Source { get; set; } = null!;

    // Interview details
    public DateTime? InterviewDate { get; set; }
    public string? InterviewerName { get; set; }
    public string? InterviewType { get; set; }   // Phone / Video / In-Person
    public string? InterviewNotes { get; set; }

    // Salary info
    public decimal? ExpectedSalary { get; set; }
    public decimal? OfferedSalary { get; set; }

    // Resume
    public string? ResumeUrl { get; set; }

    // Onboarding link — set when converted
    public int? OnboardingId { get; set; }

    // Lead assignment — set when HR manager assigns a lead to a recruiter/user
    public int? AssignedToUserId { get; set; }

    public virtual Department Department { get; set; } = null!;
}

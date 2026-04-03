using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hr_crm.Entities
{
    [Table("users")]
    public class AuthUser
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("employee_id")]
        public string? EmployeeId { get; set; }

        [Column("username")]
        public string? UserName { get; set; }

        [Column("emails")]
        public string? Emails { get; set; }

        [Column("department")]
        public string? Department { get; set; }

        [Column("designation")]
        public string? Designation { get; set; }

        [Column("manager_id")]
        public int? ManagerId { get; set; }

        [Column("assigned_team_id")]
        public int? AssignedTeamId { get; set; }

        [Column("assigned_region")]
        public string? AssignedRegion { get; set; }

        [Column("assigned_branch")]
        public string? AssignedBranch { get; set; }

        [Column("account_status")]
        public string? AccountStatus { get; set; }

        [Column("lock_reason")]
        public string? LockReason { get; set; }

        [Column("access_start_date")]
        public DateTime? AccessStartDate { get; set; }

        [Column("access_end_date")]
        public DateTime? AccessEndDate { get; set; }

        [Column("last_activity_at")]
        public DateTime? LastActivityAt { get; set; }

        [Column("last_assigned_lead_at")]
        public DateTime? LastAssignedLeadAt { get; set; }

        [Column("last_closed_ticket_at")]
        public DateTime? LastClosedTicketAt { get; set; }

        [Column("employment_type")]
        public string? EmploymentType { get; set; }

        [Column("work_shift")]
        public string? WorkShift { get; set; }

        [Column("terms_accepted_at")]
        public DateTime? TermsAcceptedAt { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("approved_by")]
        public int? ApprovedBy { get; set; }

        [Column("security_reviewed_by")]
        public int? SecurityReviewedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("created_via")]
        public string? CreatedVia { get; set; }

        [Column("DomainId")]
        public int? DomainId { get; set; }

        [Column("Gender")]
        public string? Gender { get; set; }

        [Column("payroll_amount")]
        public decimal? PayrollAmount { get; set; }
    }
}

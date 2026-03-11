using hr_crm.Entities;
using Microsoft.EntityFrameworkCore;


namespace hr_crm.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Branch> Branches { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        public DbSet<AttendanceTracking> AttendanceTracking { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<UserShift> UserShifts { get; set; }
        public DbSet<OvertimePolicy> OvertimePolicies { get; set; }
        public DbSet<OvertimeApproval> OvertimeApprovals { get; set; }
        public DbSet<OvertimeRecord> OvertimeRecords { get; set; }
        public DbSet<DepartmentRole> DepartmentRoles { get; set; }
        public DbSet<UserDepartmentRole> UserDepartmentRoles { get; set; }
        public DbSet<DepartmentBudget> DepartmentBudgets { get; set; }
        public DbSet<BudgetChangeRequest> BudgetChangeRequests { get; set; }
        public DbSet<BudgetGuideline> BudgetGuidelines { get; set; }
        public DbSet<Knowledge> Knowledges { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TodoTask> TodoTasks { get; set; }
        public DbSet<Recruitment> Recruitments { get; set; }

        public DbSet<EmployeeOnboarding> EmployeeOnboardings { get; set; }
        public DbSet<WorkExperience> WorkExperiences { get; set; }
        public DbSet<EmployeeOnboardingDocuments> EmployeeOnboardingDocuments { get; set; }

        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<DigitalSignature> DigitalSignatures { get; set; }
     
        public DbSet<ExitInterview> ExitInterviews { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<OffBoarding> OffBoardings { get; set; }
        public DbSet<EmployeeTraining> EmployeeTrainings { get; set; }
         public DbSet<LearningCourse> LearningCourses { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeOnboarding>()
       .Property(e => e.IsFatherDeceased)
       .HasConversion<string>();

            modelBuilder.Entity<EmployeeOnboarding>()
                .Property(e => e.IsMotherDeceased)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);



            // ✅ Unique attendance per user per day
            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.UserId, a.AttendanceDate })
                .IsUnique();
        }
    }
}

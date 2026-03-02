using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IDepartmentBudgetService
    {
        Task<List<DepartmentBudget>> GetAllAsync();
        Task<DepartmentBudget?> GetByIdAsync(int id);

        // 🔥 ADD THIS
        Task<List<DepartmentBudget>> GetByDepartmentIdAsync(int departmentId);

        Task<DepartmentBudget> CreateAsync(DepartmentBudget budget);

        Task<bool> ApproveByHeadAsync(int budgetId, int headUserId);
        Task<bool> ApproveByFinanceAsync(int budgetId, int financeUserId, decimal approvedAmount);
    }
}
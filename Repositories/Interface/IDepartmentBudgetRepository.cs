using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IDepartmentBudgetRepository
    {
        Task<List<DepartmentBudget>> GetAllAsync();
        Task<DepartmentBudget?> GetByIdAsync(int id);
        Task<DepartmentBudget> CreateAsync(DepartmentBudget budget);

        Task<bool> ApproveByHeadAsync(int budgetId, int headUserId);
        Task<bool> ApproveByFinanceAsync(int budgetId, int financeUserId, decimal approvedAmount);
    }
}
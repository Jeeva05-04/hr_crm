using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Services
{
    public class DepartmentBudgetService : IDepartmentBudgetService
    {
        private readonly IDepartmentBudgetRepository _repository;

        public DepartmentBudgetService(IDepartmentBudgetRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DepartmentBudget>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<DepartmentBudget?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<DepartmentBudget> CreateAsync(DepartmentBudget budget)
        {
            return await _repository.CreateAsync(budget);
        }

        public async Task<bool> ApproveByHeadAsync(int budgetId, int headUserId)
        {
            return await _repository.ApproveByHeadAsync(budgetId, headUserId);
        }

        public async Task<bool> ApproveByFinanceAsync(int budgetId, int financeUserId, decimal approvedAmount)
        {
            return await _repository.ApproveByFinanceAsync(budgetId, financeUserId, approvedAmount);
        }
    }
}
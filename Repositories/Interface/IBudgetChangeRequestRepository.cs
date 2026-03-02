using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IBudgetChangeRequestRepository
    {
        Task<BudgetChangeRequest> CreateAsync(BudgetChangeRequest request);
        Task<List<BudgetChangeRequest>> GetAllAsync();
        Task<BudgetChangeRequest?> GetByIdAsync(int id);
    }
}
using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IBudgetChangeRequestService
    {
        Task<BudgetChangeRequest> CreateAsync(BudgetChangeRequest request);
        Task<List<BudgetChangeRequest>> GetAllAsync();
        Task<BudgetChangeRequest?> GetByIdAsync(int id);
    }
}
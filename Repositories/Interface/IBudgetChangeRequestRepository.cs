using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IBudgetChangeRequestRepository
    {
        Task<BudgetChangeRequest> CreateAsync(BudgetChangeRequest request);
        Task<List<BudgetChangeRequest>> GetAllAsync();
        Task<BudgetChangeRequest?> GetByIdAsync(int id);
        Task<bool> DeleteAsync(int id);

        Task<bool> ApproveAsync(int requestId, int approverId);
        Task<bool> RejectAsync(int requestId, int approverId);
    }
}
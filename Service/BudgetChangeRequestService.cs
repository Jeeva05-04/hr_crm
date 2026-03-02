using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Services
{
    public class BudgetChangeRequestService : IBudgetChangeRequestService
    {
        private readonly IBudgetChangeRequestRepository _repository;

        public BudgetChangeRequestService(IBudgetChangeRequestRepository repository)
        {
            _repository = repository;
        }

        public Task<BudgetChangeRequest> CreateAsync(BudgetChangeRequest request)
            => _repository.CreateAsync(request);

        public Task<List<BudgetChangeRequest>> GetAllAsync()
            => _repository.GetAllAsync();

        public Task<BudgetChangeRequest?> GetByIdAsync(int id)
            => _repository.GetByIdAsync(id);
    }
}
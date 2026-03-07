using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IEmployeeTrainingRepository
    {

          
             Task<EmployeeTraining> AddAsync(EmployeeTraining training);
            Task<List<EmployeeTraining>> GetByEmployeeIdAsync(int employeeId);
            Task<EmployeeTraining> GetByIdAsync(int id);
             Task<List<EmployeeTraining>> GetAllAsync();
             Task UpdateAsync(EmployeeTraining training);
      
            Task DeleteAsync(EmployeeTraining training);
        
    }
}



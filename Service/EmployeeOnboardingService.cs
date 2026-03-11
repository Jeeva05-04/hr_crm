using AutoMapper;
using hr_crm.Data;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Services
{
    public class EmployeeOnboardingService : IEmployeeOnboardingService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public EmployeeOnboardingService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<EmployeeOnboarding> CreateAsync(EmployeeOnboardingCreateDto dto)
        {
            var onboarding = _mapper.Map<EmployeeOnboarding>(dto);

            // FIX DATE KIND FOR POSTGRESQL
            onboarding.DateOfJoining = DateTime.SpecifyKind(dto.DateOfJoining, DateTimeKind.Utc);
            onboarding.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc);

            onboarding.FatherDOB = DateTime.SpecifyKind(dto.FatherDOB, DateTimeKind.Utc);
            onboarding.MotherDOB = DateTime.SpecifyKind(dto.MotherDOB, DateTimeKind.Utc);

            if (dto.SpouseDOB.HasValue)
                onboarding.SpouseDOB = DateTime.SpecifyKind(dto.SpouseDOB.Value, DateTimeKind.Utc);

            onboarding.CreatedDate = DateTime.UtcNow;

            _context.EmployeeOnboardings.Add(onboarding);
            await _context.SaveChangesAsync();

            return onboarding;
        }

        public async Task<List<EmployeeOnboarding>> GetAllAsync()
        {
            return await _context.EmployeeOnboardings.ToListAsync();
        }

        public async Task<EmployeeOnboarding?> GetByIdAsync(int id)
        {
            return await _context.EmployeeOnboardings
                .FirstOrDefaultAsync(x => x.EmployeeOnboardingId == id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var onboarding = await _context.EmployeeOnboardings
                .FirstOrDefaultAsync(x => x.EmployeeOnboardingId == id);

            if (onboarding == null)
                return false;

            _context.EmployeeOnboardings.Remove(onboarding);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
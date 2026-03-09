using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;


namespace hr_crm.Repositories
{
    public class DigitalSignatureRespository : IDigitalSignatureRepository
    {
        
            private readonly AppDbContext _context;

            public DigitalSignatureRespository(AppDbContext context)
            {
                _context = context;
            }

            public async Task<DigitalSignature> CreateRequestAsync(DigitalSignature signature)
            {
                _context.DigitalSignatures.Add(signature);
                await _context.SaveChangesAsync();
                return signature;
            }

            public async Task<DigitalSignature?> GetByIdAsync(int signatureId)
            {
                return await _context.DigitalSignatures
                    .FirstOrDefaultAsync(s => s.SignatureId == signatureId);
            }

            public async Task<DigitalSignature> UpdateAsync(DigitalSignature signature)
            {
                _context.DigitalSignatures.Update(signature);
                await _context.SaveChangesAsync();
                return signature;
            }

            public async Task<IEnumerable<DigitalSignature>> GetByEmployeeIdAsync(int employeeId)
            {
                return await _context.DigitalSignatures
                    .Where(s => s.EmployeeId == employeeId)
                    .OrderByDescending(s => s.RequestedAt)
                    .ToListAsync();
            }
        public async Task<IEnumerable<DigitalSignature>> GetAllAsync()
        {
            return await _context.DigitalSignatures
                .OrderByDescending(s => s.RequestedAt)
                .ToListAsync();
        }
        public async Task DeleteAsync(DigitalSignature signature)
        {
            _context.DigitalSignatures.Remove(signature);
            await _context.SaveChangesAsync();
        }

    }
}


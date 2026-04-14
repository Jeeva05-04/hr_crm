using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class LeadRepository : ILeadRepository
    {
        private readonly AppDbContext _context;

        public LeadRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Lead>> GetAllAsync()
            => await _context.Leads.OrderByDescending(l => l.CreatedAt).ToListAsync();

        public async Task<List<Lead>> GetByStatusAsync(string status)
            => await _context.Leads.Where(l => l.Status == status).OrderByDescending(l => l.CreatedAt).ToListAsync();

        public async Task<List<Lead>> GetByAssignedUserAsync(int userId)
            => await _context.Leads.Where(l => l.AssignedToUserId == userId).OrderByDescending(l => l.CreatedAt).ToListAsync();

        public async Task<Lead?> GetByIdAsync(int leadId)
            => await _context.Leads.FindAsync(leadId);

        public async Task<Lead> AddAsync(Lead lead)
        {
            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();
            return lead;
        }

        public async Task<Lead> UpdateAsync(Lead lead)
        {
            _context.Leads.Update(lead);
            await _context.SaveChangesAsync();
            return lead;
        }

        public async Task DeleteAsync(int leadId)
        {
            var lead = await _context.Leads.FindAsync(leadId);
            if (lead != null)
            {
                _context.Leads.Remove(lead);
                await _context.SaveChangesAsync();
            }
        }
    }
}

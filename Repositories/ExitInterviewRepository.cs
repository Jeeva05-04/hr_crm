using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{ 

    public class ExitInterviewRepository : IExitInterviewRepository
    {
        private readonly AppDbContext _context;

        public ExitInterviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ExitInterview> ScheduleExitInterview(ExitInterview interview)
        {
            _context.ExitInterviews.Add(interview);
            await _context.SaveChangesAsync();
            return interview;
        }

        public async Task<ExitInterview> SubmitFeedback(ExitInterview interview)
        {
            _context.ExitInterviews.Update(interview);
            await _context.SaveChangesAsync();
            return interview;
        }

        public async Task<ExitInterview> GetByUserId(int userId)
        {
            return await _context.ExitInterviews
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<List<ExitInterview>> GetAll()
        {
            return await _context.ExitInterviews.ToListAsync();
        }
        public async Task<ExitInterview> GetById(int id)
        {
            return await _context.ExitInterviews.FindAsync(id);
        }

        public async Task<ExitInterview> Update(ExitInterview interview)
        {
            _context.ExitInterviews.Update(interview);
            await _context.SaveChangesAsync();
            return interview;
        }
        public async Task Delete(int id)
        {
            var interview = await _context.ExitInterviews.FindAsync(id);

            if (interview != null)
            {
                _context.ExitInterviews.Remove(interview);
                await _context.SaveChangesAsync();
            }
        }
    }
}


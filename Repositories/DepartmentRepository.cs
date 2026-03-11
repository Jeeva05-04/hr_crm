using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _context;

        public DepartmentRepository(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET ALL
        public async Task<List<Department>> GetAllAsync()
        {
            return await _context.Departments
                .Include(d => d.Branch)
                .ToListAsync();
        }

        // ✅ GET BY ID  ← THIS WAS MISSING
        public async Task<Department?> GetByIdAsync(int id)
        {
            return await _context.Departments
                .Include(d => d.Branch)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);
        }

        // ✅ ADD
        public async Task AddAsync(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
        }

        // ✅ UPDATE
        public async Task<bool> UpdateAsync(int id, Department department)
        {
            var existing = await _context.Departments.FindAsync(id);
            if (existing == null)
                return false;

            existing.DepartmentName = department.DepartmentName;
            existing.BranchId = department.BranchId;

            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ DELETE
        public async Task<bool> DeleteAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return false;

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ GET USERS IN A DEPARTMENT
        public async Task<object?> GetUsersInDepartmentAsync(int departmentId)
        {
            var departmentExists = await _context.Departments
                .AnyAsync(d => d.DepartmentId == departmentId);

            if (!departmentExists)
                return null;

            var users = await _context.UserDepartmentRoles
                .Include(ur => ur.DepartmentRole)
                .Where(ur => ur.DepartmentRole.DepartmentId == departmentId)
                .Select(ur => new
                {
                    ur.UserId,
                    ur.DepartmentRole.DepartmentRoleId,
                    ur.DepartmentRole.RoleName,
                    ur.DepartmentRole.RequiredSkillLevel,
                    ur.DepartmentRole.PerformanceLevel
                })
                .ToListAsync();

            return new
            {
                DepartmentId = departmentId,
                TotalUsers = users.Select(u => u.UserId).Distinct().Count(),
                Users = users
            };
        }
    }
}
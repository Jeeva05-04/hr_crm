using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Service
{
    public class LeadService : ILeadService
    {
        private readonly ILeadRepository _repo;
        private readonly NotificationService _notification;

        public LeadService(ILeadRepository repo, NotificationService notification)
        {
            _repo = repo;
            _notification = notification;
        }

        public async Task<List<LeadResponseDto>> GetAllAsync()
        {
            var leads = await _repo.GetAllAsync();
            return leads.Select(MapToResponse).ToList();
        }

        public async Task<List<LeadResponseDto>> GetByStatusAsync(string status)
        {
            var leads = await _repo.GetByStatusAsync(status);
            return leads.Select(MapToResponse).ToList();
        }

        public async Task<List<LeadResponseDto>> GetByAssignedUserAsync(int userId)
        {
            var leads = await _repo.GetByAssignedUserAsync(userId);
            return leads.Select(MapToResponse).ToList();
        }

        public async Task<LeadResponseDto?> GetByIdAsync(int leadId)
        {
            var lead = await _repo.GetByIdAsync(leadId);
            return lead is null ? null : MapToResponse(lead);
        }

        public async Task<LeadResponseDto> CreateAsync(LeadCreateDto dto)
        {
            var lead = new Lead
            {
                LeadName = dto.LeadName,
                Email = dto.Email,
                Phone = dto.Phone,
                Source = dto.Source,
                Notes = dto.Notes,
                Status = "New",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repo.AddAsync(lead);
            return MapToResponse(created);
        }

        // HR assigns a lead to an employee — notification sent to employee
        public async Task<(bool Success, string? Error)> AssignLeadAsync(int leadId, LeadAssignDto dto)
        {
            var lead = await _repo.GetByIdAsync(leadId);
            if (lead is null)
                return (false, "Lead not found.");

            lead.AssignedToUserId = dto.AssignedToUserId;
            lead.AssignedByUserId = dto.AssignedByUserId;
            lead.Status = "Assigned";
            lead.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(lead);

            // Notify the assigned employee
            await _notification.CreateNotification(
                userId: dto.AssignedToUserId,
                title: "New Lead Assigned",
                message: $"HR has assigned a new lead to you: {lead.LeadName} from {lead.Source}. Please follow up.",
                module: "Leads",
                referenceId: leadId
            );

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateStatusAsync(int leadId, LeadUpdateStatusDto dto)
        {
            var lead = await _repo.GetByIdAsync(leadId);
            if (lead is null)
                return (false, "Lead not found.");

            var validStatuses = new[] { "New", "Contacted", "Qualified", "Assigned", "Converted", "Closed" };
            if (!validStatuses.Contains(dto.Status))
                return (false, $"Invalid status. Allowed: {string.Join(", ", validStatuses)}");

            lead.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.Notes))
                lead.Notes = dto.Notes;
            lead.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(lead);
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int leadId)
        {
            var lead = await _repo.GetByIdAsync(leadId);
            if (lead is null)
                return (false, "Lead not found.");

            await _repo.DeleteAsync(leadId);
            return (true, null);
        }

        private static LeadResponseDto MapToResponse(Lead l) => new()
        {
            LeadId = l.LeadId,
            LeadName = l.LeadName,
            Email = l.Email,
            Phone = l.Phone,
            Source = l.Source,
            Status = l.Status,
            Notes = l.Notes,
            AssignedToUserId = l.AssignedToUserId,
            AssignedByUserId = l.AssignedByUserId,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        };
    }
}

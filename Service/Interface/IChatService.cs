using hr_crm.Data;
using hr_crm.DTO;
using hr_crm.Entities;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Service.Interface
{
    public interface IChatService
    {
        Task<ChatMessageDto> SendMessageAsync(SendChatMessageDto dto, int sentByUserId, string? userName);
        Task<List<ChatMessageDto>> GetMessagesAsync(int currentUserId, int pageSize = 100, int pageNumber = 1);
        Task<ChatPresenceDto> EnsurePresenceAsync(int userId, string? userName);
        Task<ChatPresenceDto> UpdatePresenceAsync(int userId, string? userName, UpdateChatPresenceDto dto);
    }
}

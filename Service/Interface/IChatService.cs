using hr_crm.DTO;

namespace hr_crm.Service.Interface
{
    public interface IChatService
    {
        Task<(ChatConversationDto? Result, string? Error)> CreateDirectConversationAsync(int currentUserId, string? currentUserName, int otherUserId);
        Task<(ChatConversationDto? Result, string? Error)> CreateGroupConversationAsync(int currentUserId, string? currentUserName, CreateGroupConversationDto dto);
        Task<(bool Success, string? Error)> AddGroupMembersAsync(int currentUserId, string? currentUserName, int conversationId, AddGroupMembersDto dto);
        Task<List<ChatConversationDto>> GetConversationsAsync(int currentUserId);
        Task<(List<ChatMessageDto>? Result, string? Error)> GetConversationMessagesAsync(int currentUserId, int conversationId, int pageSize = 100, int pageNumber = 1);
        Task<ChatMessageDto> SendMessageAsync(SendChatMessageDto dto, int sentByUserId, string? userName);
        Task<List<ChatMessageDto>> GetMessagesAsync(int currentUserId, int pageSize = 100, int pageNumber = 1);
        Task<List<EmployeeContactDto>> GetEmployeeContactsAsync();
        Task<ChatPresenceDto> EnsurePresenceAsync(int userId, string? userName);
        Task<ChatPresenceDto> UpdatePresenceAsync(int userId, string? userName, UpdateChatPresenceDto dto);
    }
}

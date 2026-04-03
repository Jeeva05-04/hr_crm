using hr_crm.Data;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Service
{
    public class ChatService : IChatService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public ChatService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<ChatMessageDto> SendMessageAsync(SendChatMessageDto dto, int sentByUserId, string? userName)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            if (string.IsNullOrWhiteSpace(dto.Content))
                throw new InvalidOperationException("Message content cannot be empty");

            var message = new ChatMessage
            {
                SentByUserId = sentByUserId,
                SentByUserName = userName,
                ReceiverUserId = dto.ReceiverUserId, // Null = everyone
                Content = dto.Content,
                SentAt = DateTime.UtcNow
            };

            db.ChatMessages.Add(message);
            await db.SaveChangesAsync();

            return new ChatMessageDto
            {
                ChatMessageId = message.ChatMessageId,
                SentByUserId = message.SentByUserId,
                SentByUserName = message.SentByUserName,
                ReceiverUserId = message.ReceiverUserId,
                Content = message.Content,
                SentAt = message.SentAt
            };
        }

        public async Task<List<ChatMessageDto>> GetMessagesAsync(int currentUserId, int pageSize = 100, int pageNumber = 1)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            var skip = (pageNumber - 1) * pageSize;

            var messages = await db.ChatMessages
                .Where(m => !m.IsDeleted &&
                            (m.ReceiverUserId == null ||
                             m.SentByUserId == currentUserId ||
                             m.ReceiverUserId == currentUserId))
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return messages
                .OrderBy(m => m.SentAt)
                .Select(m => new ChatMessageDto
                {
                    ChatMessageId = m.ChatMessageId,
                    SentByUserId = m.SentByUserId,
                    SentByUserName = m.SentByUserName,
                    ReceiverUserId = m.ReceiverUserId,
                    Content = m.Content,
                    SentAt = m.SentAt
                })
                .ToList();
        }

        public async Task<ChatPresenceDto> EnsurePresenceAsync(int userId, string? userName)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            var presence = await db.ChatUserPresences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (presence == null)
            {
                presence = new ChatUserPresence
                {
                    UserId = userId,
                    UserName = userName,
                    Status = "Available",
                    UpdatedAt = DateTime.UtcNow
                };

                db.ChatUserPresences.Add(presence);
                await db.SaveChangesAsync();
            }
            else if (!string.IsNullOrWhiteSpace(userName) && !string.Equals(presence.UserName, userName, StringComparison.Ordinal))
            {
                presence.UserName = userName;
                presence.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            return MapPresence(presence);
        }

        public async Task<ChatPresenceDto> UpdatePresenceAsync(int userId, string? userName, UpdateChatPresenceDto dto)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            var normalizedStatus = NormalizeStatus(dto.Status);
            var presence = await db.ChatUserPresences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (presence == null)
            {
                presence = new ChatUserPresence
                {
                    UserId = userId
                };

                db.ChatUserPresences.Add(presence);
            }

            presence.UserName = string.IsNullOrWhiteSpace(userName) ? presence.UserName : userName;
            presence.Status = normalizedStatus;
            presence.StatusMessage = string.IsNullOrWhiteSpace(dto.StatusMessage) ? null : dto.StatusMessage.Trim();
            presence.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return MapPresence(presence);
        }

        private static ChatPresenceDto MapPresence(ChatUserPresence presence)
        {
            return new ChatPresenceDto
            {
                UserId = presence.UserId,
                UserName = presence.UserName,
                Status = presence.Status,
                StatusMessage = presence.StatusMessage,
                UpdatedAt = presence.UpdatedAt
            };
        }

        private static string NormalizeStatus(string? status)
        {
            var normalized = (status ?? "Available").Trim().ToLowerInvariant();
            return normalized switch
            {
                "available" => "Available",
                "away" => "Away",
                "busy" => "Busy",
                "invisible" => "Invisible",
                _ => throw new InvalidOperationException("Status must be Available, Away, Busy, or Invisible")
            };
        }

        private static async Task EnsureChatTablesAsync(AppDbContext db)
        {
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ChatMessages"" (
                    ""ChatMessageId"" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""SentByUserId"" integer NOT NULL,
                    ""SentByUserName"" text NULL,
                    ""ReceiverUserId"" integer NULL,
                    ""Content"" text NOT NULL,
                    ""SentAt"" timestamp with time zone NOT NULL,
                    ""IsDeleted"" boolean NOT NULL DEFAULT FALSE
                );");

            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ChatUserPresences"" (
                    ""ChatUserPresenceId"" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""UserId"" integer NOT NULL,
                    ""UserName"" text NULL,
                    ""Status"" text NOT NULL,
                    ""StatusMessage"" text NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL
                );");

            await db.Database.ExecuteSqlRawAsync(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ChatUserPresences_UserId""
                ON ""ChatUserPresences"" (""UserId"");");
        }
    }
}

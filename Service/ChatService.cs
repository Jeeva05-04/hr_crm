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

        public async Task<(ChatConversationDto? Result, string? Error)> CreateDirectConversationAsync(int currentUserId, string? currentUserName, int otherUserId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            if (otherUserId <= 0)
                return (null, "No employee found for the given user ID.");

            if (otherUserId == currentUserId)
                return (null, "You cannot create a direct conversation with yourself.");

            if (!await EmployeeExistsAsync(db, otherUserId))
                return (null, $"No employee found with user ID {otherUserId}.");

            var existingConversationIds = await db.ChatConversationMembers
                .Where(m => m.UserId == currentUserId || m.UserId == otherUserId)
                .GroupBy(m => m.ChatConversationId)
                .Where(g => g.Count() == 2 &&
                            g.Select(x => x.UserId).Distinct().Count() == 2 &&
                            g.Any(x => x.UserId == currentUserId) &&
                            g.Any(x => x.UserId == otherUserId))
                .Select(g => g.Key)
                .ToListAsync();

            if (existingConversationIds.Count > 0)
            {
                var existing = await db.ChatConversations
                    .Where(c => existingConversationIds.Contains(c.ChatConversationId) &&
                                c.ConversationType == "Direct" &&
                                !c.IsDeleted)
                    .OrderBy(c => c.ChatConversationId)
                    .FirstOrDefaultAsync();

                if (existing != null)
                    return (await MapConversationAsync(db, existing, currentUserId), null);
            }

            var otherUserName = await ResolveEmployeeNameAsync(db, otherUserId);
            var conversation = new ChatConversation
            {
                ConversationType = "Direct",
                CreatedByUserId = currentUserId,
                CreatedByUserName = currentUserName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.ChatConversations.Add(conversation);
            await db.SaveChangesAsync();

            db.ChatConversationMembers.AddRange(
                new ChatConversationMember
                {
                    ChatConversationId = conversation.ChatConversationId,
                    UserId = currentUserId,
                    UserName = currentUserName,
                    IsAdmin = true,
                    JoinedAt = DateTime.UtcNow
                },
                new ChatConversationMember
                {
                    ChatConversationId = conversation.ChatConversationId,
                    UserId = otherUserId,
                    UserName = otherUserName,
                    IsAdmin = false,
                    JoinedAt = DateTime.UtcNow
                });

            await db.SaveChangesAsync();
            return (await MapConversationAsync(db, conversation, currentUserId), null);
        }

        public async Task<(ChatConversationDto? Result, string? Error)> CreateGroupConversationAsync(int currentUserId, string? currentUserName, CreateGroupConversationDto dto)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            var groupName = dto.Name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(groupName))
                return (null, "Group name is required.");

            var memberIds = dto.MemberUserIds
                .Append(currentUserId)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (memberIds.Count < 2)
                return (null, "A group must have at least two employees.");

            var invalidIds = new List<int>();
            foreach (var memberUserId in memberIds)
            {
                if (!await EmployeeExistsAsync(db, memberUserId))
                    invalidIds.Add(memberUserId);
            }

            if (invalidIds.Count > 0)
                return (null, $"No employee found for these user IDs: {string.Join(", ", invalidIds)}.");

            var createdAt = DateTime.UtcNow;
            var conversation = new ChatConversation
            {
                ConversationType = "Group",
                Name = groupName,
                CreatedByUserId = currentUserId,
                CreatedByUserName = currentUserName,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            };

            db.ChatConversations.Add(conversation);
            await db.SaveChangesAsync();

            var members = new List<ChatConversationMember>();
            foreach (var memberUserId in memberIds)
            {
                members.Add(new ChatConversationMember
                {
                    ChatConversationId = conversation.ChatConversationId,
                    UserId = memberUserId,
                    UserName = memberUserId == currentUserId ? currentUserName : await ResolveEmployeeNameAsync(db, memberUserId),
                    IsAdmin = memberUserId == currentUserId,
                    JoinedAt = createdAt
                });
            }

            db.ChatConversationMembers.AddRange(members);
            await db.SaveChangesAsync();

            return (await MapConversationAsync(db, conversation, currentUserId), null);
        }

        public async Task<(bool Success, string? Error)> AddGroupMembersAsync(int currentUserId, string? currentUserName, int conversationId, AddGroupMembersDto dto)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            var conversation = await db.ChatConversations
                .FirstOrDefaultAsync(c => c.ChatConversationId == conversationId && !c.IsDeleted);

            if (conversation == null)
                return (false, "Conversation not found.");

            if (conversation.ConversationType != "Group")
                return (false, "Members can only be added to group conversations.");

            var currentMembership = await db.ChatConversationMembers
                .FirstOrDefaultAsync(m => m.ChatConversationId == conversationId &&
                                          m.UserId == currentUserId &&
                                          m.IsActive);

            if (currentMembership == null)
                return (false, "You are not a member of this conversation.");

            if (!currentMembership.IsAdmin)
                return (false, "Only a group admin can add members.");

            var candidateIds = dto.MemberUserIds
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (candidateIds.Count == 0)
                return (false, "At least one employee user ID is required.");

            var invalidIds = new List<int>();
            foreach (var candidateId in candidateIds)
            {
                if (!await EmployeeExistsAsync(db, candidateId))
                    invalidIds.Add(candidateId);
            }

            if (invalidIds.Count > 0)
                return (false, $"No employee found for these user IDs: {string.Join(", ", invalidIds)}.");

            var existingMemberIds = await db.ChatConversationMembers
                .Where(m => m.ChatConversationId == conversationId && m.IsActive)
                .Select(m => m.UserId)
                .ToListAsync();

            var membersToAdd = candidateIds
                .Where(x => !existingMemberIds.Contains(x))
                .ToList();

            if (membersToAdd.Count == 0)
                return (true, null);

            foreach (var memberUserId in membersToAdd)
            {
                db.ChatConversationMembers.Add(new ChatConversationMember
                {
                    ChatConversationId = conversationId,
                    UserId = memberUserId,
                    UserName = memberUserId == currentUserId ? currentUserName : await ResolveEmployeeNameAsync(db, memberUserId),
                    IsAdmin = false,
                    JoinedAt = DateTime.UtcNow
                });
            }

            conversation.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteGroupConversationAsync(int currentUserId, int conversationId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            var conversation = await db.ChatConversations
                .FirstOrDefaultAsync(c => c.ChatConversationId == conversationId && !c.IsDeleted);

            if (conversation == null)
                return (false, "Conversation not found.");

            if (conversation.ConversationType != "Group")
                return (false, "Only group conversations can be deleted.");

            var currentMembership = await db.ChatConversationMembers
                .FirstOrDefaultAsync(m => m.ChatConversationId == conversationId &&
                                          m.UserId == currentUserId &&
                                          m.IsActive);

            if (currentMembership == null)
                return (false, "You are not a member of this conversation.");

            if (!currentMembership.IsAdmin)
                return (false, "Only a group admin can delete this conversation.");

            var messages = await db.ChatMessages
                .Where(m => m.ChatConversationId == conversationId)
                .ToListAsync();

            var members = await db.ChatConversationMembers
                .Where(m => m.ChatConversationId == conversationId)
                .ToListAsync();

            db.ChatMessages.RemoveRange(messages);
            db.ChatConversationMembers.RemoveRange(members);
            db.ChatConversations.Remove(conversation);

            await db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<ChatConversationDto>> GetConversationsAsync(int currentUserId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            var conversationIds = await db.ChatConversationMembers
                .Where(m => m.UserId == currentUserId && m.IsActive)
                .Select(m => m.ChatConversationId)
                .ToListAsync();

            var conversations = await db.ChatConversations
                .Where(c => conversationIds.Contains(c.ChatConversationId) && !c.IsDeleted)
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                .ToListAsync();

            var result = new List<ChatConversationDto>();
            foreach (var conversation in conversations)
                result.Add(await MapConversationAsync(db, conversation, currentUserId));

            return result
                .OrderByDescending(c => c.LastMessage?.SentAt ?? c.UpdatedAt ?? c.CreatedAt)
                .ToList();
        }

        public async Task<(List<ChatMessageDto>? Result, string? Error)> GetConversationMessagesAsync(int currentUserId, int conversationId, int pageSize = 100, int pageNumber = 1)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            if (!await IsConversationMemberAsync(db, conversationId, currentUserId))
                return (null, "Conversation not found or access denied.");

            var skip = (pageNumber - 1) * pageSize;
            var messages = await db.ChatMessages
                .Where(m => !m.IsDeleted && m.ChatConversationId == conversationId)
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return (messages
                .OrderBy(m => m.SentAt)
                .Select(MapMessage)
                .ToList(), null);
        }

        public async Task<ChatMessageDto> SendMessageAsync(SendChatMessageDto dto, int sentByUserId, string? userName)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            if (string.IsNullOrWhiteSpace(dto.Content))
                throw new InvalidOperationException("Message content cannot be empty");

            int conversationId;
            if (dto.ConversationId.HasValue && dto.ConversationId.Value > 0)
            {
                conversationId = dto.ConversationId.Value;
            }
            else if (dto.ReceiverUserId.HasValue)
            {
                var (conversation, error) = await CreateDirectConversationAsync(sentByUserId, userName, dto.ReceiverUserId.Value);
                if (conversation == null)
                    throw new InvalidOperationException(error ?? "Unable to create direct conversation.");

                conversationId = conversation.ChatConversationId;
            }
            else
            {
                throw new InvalidOperationException("ConversationId or ReceiverUserId is required.");
            }

            var conversationEntity = await db.ChatConversations
                .FirstOrDefaultAsync(c => c.ChatConversationId == conversationId && !c.IsDeleted);

            if (conversationEntity == null)
                throw new InvalidOperationException("Conversation not found.");

            if (!await IsConversationMemberAsync(db, conversationId, sentByUserId))
                throw new InvalidOperationException("You are not a member of this conversation.");

            var message = new ChatMessage
            {
                ChatConversationId = conversationId,
                SentByUserId = sentByUserId,
                SentByUserName = userName,
                ReceiverUserId = dto.ReceiverUserId,
                Content = dto.Content.Trim(),
                SentAt = DateTime.UtcNow
            };

            db.ChatMessages.Add(message);
            conversationEntity.UpdatedAt = message.SentAt;
            await db.SaveChangesAsync();

            return MapMessage(message);
        }

        public async Task<List<ChatMessageDto>> GetMessagesAsync(int currentUserId, int pageSize = 100, int pageNumber = 1)
        {
            var conversations = await GetConversationsAsync(currentUserId);
            var conversationIds = conversations.Select(c => c.ChatConversationId).ToList();

            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            var skip = (pageNumber - 1) * pageSize;
            var messages = await db.ChatMessages
                .Where(m => !m.IsDeleted &&
                            ((m.ChatConversationId.HasValue && conversationIds.Contains(m.ChatConversationId.Value)) ||
                             (!m.ChatConversationId.HasValue &&
                              (m.ReceiverUserId == null ||
                               m.SentByUserId == currentUserId ||
                               m.ReceiverUserId == currentUserId))))
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return messages
                .OrderBy(m => m.SentAt)
                .Select(MapMessage)
                .ToList();
        }

        public async Task<List<EmployeeContactDto>> GetEmployeeContactsAsync()
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            await EnsureChatTablesAsync(db);

            var directory = await GetEmployeeDirectoryAsync(db);
            return directory
                .OrderBy(x => x.Key)
                .Select(x => new EmployeeContactDto
                {
                    UserId = x.Key,
                    UserName = x.Value
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

        private static ChatMessageDto MapMessage(ChatMessage message)
        {
            return new ChatMessageDto
            {
                ChatMessageId = message.ChatMessageId,
                ChatConversationId = message.ChatConversationId,
                SentByUserId = message.SentByUserId,
                SentByUserName = message.SentByUserName,
                ReceiverUserId = message.ReceiverUserId,
                Content = message.Content,
                SentAt = message.SentAt
            };
        }

        private async Task<ChatConversationDto> MapConversationAsync(AppDbContext db, ChatConversation conversation, int currentUserId)
        {
            var members = await db.ChatConversationMembers
                .Where(m => m.ChatConversationId == conversation.ChatConversationId && m.IsActive)
                .OrderBy(m => m.JoinedAt)
                .ToListAsync();

            var lastMessage = await db.ChatMessages
                .Where(m => !m.IsDeleted && m.ChatConversationId == conversation.ChatConversationId)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            var displayName = conversation.Name ?? string.Empty;
            if (conversation.ConversationType == "Direct")
            {
                var otherMember = members.FirstOrDefault(m => m.UserId != currentUserId);
                displayName = otherMember?.UserName ?? $"User{otherMember?.UserId ?? 0}";
            }
            else if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = $"Group {conversation.ChatConversationId}";
            }

            return new ChatConversationDto
            {
                ChatConversationId = conversation.ChatConversationId,
                ConversationType = conversation.ConversationType,
                Name = conversation.Name,
                CreatedByUserId = conversation.CreatedByUserId,
                CreatedByUserName = conversation.CreatedByUserName,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                DisplayName = displayName,
                Members = members.Select(m => new ChatConversationMemberDto
                {
                    UserId = m.UserId,
                    UserName = m.UserName,
                    IsAdmin = m.IsAdmin,
                    JoinedAt = m.JoinedAt
                }).ToList(),
                LastMessage = lastMessage == null ? null : MapMessage(lastMessage)
            };
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

        private static async Task<bool> IsConversationMemberAsync(AppDbContext db, int conversationId, int userId)
        {
            return await db.ChatConversationMembers
                .AnyAsync(m => m.ChatConversationId == conversationId &&
                               m.UserId == userId &&
                               m.IsActive);
        }

        private static async Task<bool> EmployeeExistsAsync(AppDbContext db, int userId)
        {
            if (userId <= 0) return false;

            var directory = await GetEmployeeDirectoryAsync(db);
            return directory.ContainsKey(userId);
        }

        private static async Task<string?> ResolveEmployeeNameAsync(AppDbContext db, int userId)
        {
            var authUserName = await db.AuthUsers
                .Where(x => x.UserId == userId &&
                            x.DeletedAt == null &&
                            !string.IsNullOrWhiteSpace(x.UserName))
                .Select(x => x.UserName)
                .FirstOrDefaultAsync();
            if (IsMeaningfulDisplayName(authUserName))
                return authUserName;

            var onboardingName = await db.EmployeeOnboardings
                .Where(x => (x.ConvertedEmployeeId == userId || x.EmployeeOnboardingId == userId) &&
                            !string.IsNullOrWhiteSpace(x.FullName))
                .OrderByDescending(x => x.ConvertedAt ?? x.CreatedDate)
                .Select(x => x.FullName)
                .FirstOrDefaultAsync();
            if (IsMeaningfulDisplayName(onboardingName))
                return onboardingName;

            var payrollName = await db.Payrolls
                .Where(x => x.UserId == userId && !string.IsNullOrWhiteSpace(x.UserName))
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.UserName)
                .FirstOrDefaultAsync();
            if (IsMeaningfulDisplayName(payrollName))
                return payrollName;

            var salaryName = await db.SalaryConfigurations
                .Where(x => x.UserId == userId && !string.IsNullOrWhiteSpace(x.UserName))
                .Select(x => x.UserName)
                .FirstOrDefaultAsync();
            if (IsMeaningfulDisplayName(salaryName))
                return salaryName;

            var presenceName = await db.ChatUserPresences
                .Where(x => x.UserId == userId && !string.IsNullOrWhiteSpace(x.UserName))
                .Select(x => x.UserName)
                .FirstOrDefaultAsync();
            if (IsMeaningfulDisplayName(presenceName))
                return presenceName;

            return authUserName
                ?? onboardingName
                ?? payrollName
                ?? salaryName
                ?? presenceName;
        }

        private static async Task<Dictionary<int, string>> GetEmployeeDirectoryAsync(AppDbContext db)
        {
            var ids = new HashSet<int>();

            await AddIdsAsync(db.UserDepartmentRoles.Select(x => x.UserId), ids);
            await AddIdsAsync(db.UserShifts.Select(x => x.UserId), ids);
            await AddIdsAsync(db.Attendances.Select(x => x.UserId), ids);
            await AddIdsAsync(db.Payrolls.Select(x => x.UserId), ids);
            await AddIdsAsync(db.SalaryConfigurations.Select(x => x.UserId), ids);
            await AddIdsAsync(db.EmployeeTrainings.Select(x => x.UserId), ids);
            await AddIdsAsync(db.Leaves.Select(x => x.UserId), ids);
            await AddIdsAsync(db.ChatConversationMembers.Select(x => x.UserId), ids);
            await AddIdsAsync(db.ChatUserPresences.Select(x => x.UserId), ids);
            await AddIdsAsync(db.AuthUsers.Where(x => x.DeletedAt == null).Select(x => x.UserId), ids);
            await AddIdsAsync(db.EmployeeOnboardings.Where(x => x.ConvertedEmployeeId.HasValue).Select(x => x.ConvertedEmployeeId!.Value), ids);

            var directory = new Dictionary<int, string>();
            foreach (var userId in ids)
                directory[userId] = await ResolveEmployeeNameAsync(db, userId) ?? $"User{userId}";

            return directory;
        }

        private static async Task AddIdsAsync(IQueryable<int> query, HashSet<int> ids)
        {
            foreach (var id in await query.Distinct().ToListAsync())
            {
                if (id > 0)
                    ids.Add(id);
            }
        }

        private static bool IsMeaningfulDisplayName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var trimmed = value.Trim();
            if (trimmed.Length <= 1)
                return false;

            if (trimmed.StartsWith("User", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(trimmed[4..], out _))
                return false;

            return true;
        }

        private static async Task EnsureChatTablesAsync(AppDbContext db)
        {
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ChatConversations"" (
                    ""ChatConversationId"" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""ConversationType"" text NOT NULL,
                    ""Name"" text NULL,
                    ""CreatedByUserId"" integer NOT NULL,
                    ""CreatedByUserName"" text NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NULL,
                    ""IsDeleted"" boolean NOT NULL DEFAULT FALSE
                );");

            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ChatConversationMembers"" (
                    ""ChatConversationMemberId"" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""ChatConversationId"" integer NOT NULL,
                    ""UserId"" integer NOT NULL,
                    ""UserName"" text NULL,
                    ""IsAdmin"" boolean NOT NULL DEFAULT FALSE,
                    ""JoinedAt"" timestamp with time zone NOT NULL,
                    ""IsActive"" boolean NOT NULL DEFAULT TRUE
                );");

            await db.Database.ExecuteSqlRawAsync(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_ChatConversationMembers_ConversationUser""
                ON ""ChatConversationMembers"" (""ChatConversationId"", ""UserId"");");

            await db.Database.ExecuteSqlRawAsync(@"
                CREATE INDEX IF NOT EXISTS ""IX_ChatConversationMembers_UserId""
                ON ""ChatConversationMembers"" (""UserId"");");

            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ChatMessages"" (
                    ""ChatMessageId"" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""ChatConversationId"" integer NULL,
                    ""SentByUserId"" integer NOT NULL,
                    ""SentByUserName"" text NULL,
                    ""ReceiverUserId"" integer NULL,
                    ""Content"" text NOT NULL,
                    ""SentAt"" timestamp with time zone NOT NULL,
                    ""IsDeleted"" boolean NOT NULL DEFAULT FALSE
                );");

            await db.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE ""ChatMessages""
                ADD COLUMN IF NOT EXISTS ""ChatConversationId"" integer NULL;");

            await db.Database.ExecuteSqlRawAsync(@"
                CREATE INDEX IF NOT EXISTS ""IX_ChatMessages_ConversationId_SentAt""
                ON ""ChatMessages"" (""ChatConversationId"", ""SentAt"");");

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

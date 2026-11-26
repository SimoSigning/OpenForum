using EntityContext;
using EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Repository.Interfaces.Repositories;

namespace Repository.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ForumContext context;

        public ChatRepository(ForumContext forumContext)
        {
            context = forumContext;
        }

        // -------------------------
        // Chat rooms
        // -------------------------
        public async Task<ChatRoom?> GetRoomByIdAsync(int roomId)
        {
            return await context.ChatRooms.FirstOrDefaultAsync(r => r.Id == roomId);
        }

        public async Task<ChatRoom?> GetRoomByTopicIdAsync(int topicId)
        {
            return await context.ChatRooms.FirstOrDefaultAsync(r => r.TopicId == topicId);
        }

        public async Task<IEnumerable<ChatRoom>> GetRoomsForUserAsync(string userId)
        {
            return await context.ChatRooms
                .AsNoTracking()
                .Include(r => r.Members)
                .Where(r => r.RoomType == ChatRoomType.Private && r.Members.Any(m => m.UserId == userId))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatRoom>> GetMiscellaneousRoomsAsync()
        {
            return await context.ChatRooms
                .AsNoTracking()
                .Include(r => r.Members)
                .Where(r => r.RoomType == ChatRoomType.Miscellaneous)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task CreateRoomWithMembersAsync(ChatRoom room, IEnumerable<ChatRoomMember> members)
        {
            List<ChatRoomMember> memberList = members.ToList();

            bool ownsTransaction = context.Database.CurrentTransaction == null;

            await using IDbContextTransaction? transaction = ownsTransaction ? await context.Database.BeginTransactionAsync() : null;

            try
            {
                context.ChatRooms.Add(room);

                foreach (ChatRoomMember member in memberList)
                {
                    room.Members.Add(member);
                }

                await context.SaveChangesAsync();

                if (ownsTransaction)
                {
                    await transaction!.CommitAsync();
                }
            }
            catch
            {
                try
                {
                    if (ownsTransaction && transaction != null)
                    {
                        await transaction.RollbackAsync();
                    }
                }
                finally
                {
                    // Remove the failed room and its members
                    // from EF Core's change tracker.
                    foreach (ChatRoomMember member in memberList)
                    {
                        context.Entry(member).State = EntityState.Detached;
                    }

                    context.Entry(room).State = EntityState.Detached;
                }

                throw;
            }
        }

        public async Task<bool> LeaveRoomAsync(int roomId, string userId)
        {
            await using IDbContextTransaction transaction =
                await context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);

            try
            {
                ChatRoom? room = await context.ChatRooms
                    .FirstOrDefaultAsync(r => r.Id == roomId);

                // The owner may already have deleted this room.
                // Treat that as successful cleanup.
                if (room == null)
                {
                    await transaction.CommitAsync();
                    return true;
                }

                ChatRoomMember? member = await context.ChatRoomMembers
                    .FirstOrDefaultAsync(m =>
                        m.ChatRoomId == roomId &&
                        m.UserId == userId);

                // The user has already left.
                if (member == null)
                {
                    await transaction.CommitAsync();
                    return false;
                }

                List<ChatRoomMember> remainingMembers =
                    await context.ChatRoomMembers
                        .Where(m => m.ChatRoomId == roomId && m.UserId != userId)
                        .ToListAsync();

                bool roomDeleted = false;

                // Delete a private room when its final member leaves.
                if (room.RoomType == ChatRoomType.Private &&
                    remainingMembers.Count == 0)
                {
                    context.ChatRooms.Remove(room);
                    roomDeleted = true;
                }
                else
                {
                    // Transfer ownership when the current owner leaves.
                    if (room.RoomType == ChatRoomType.Private &&
                        room.OwnerUserId == userId)
                    {
                        ChatRoomMember newOwner = remainingMembers
                            .OrderBy(m => m.JoinedAt)
                            .ThenBy(m => m.UserId, StringComparer.Ordinal)
                            .First();

                        room.OwnerUserId = newOwner.UserId;
                    }

                    context.ChatRoomMembers.Remove(member);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return roomDeleted;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        //handling topics in chatrepository.. But its fine for now
        public async Task<string?> GetTopicTitleAsync(int topicId)
        {
            return await context.Topics
                .Where(t => t.Id == topicId)
                .Select(t => t.Title)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> TopicExistsAsync(int topicId)
        {
            return await context.Topics.AnyAsync(t => t.Id == topicId);
        }

        public async Task<int> GetUnseenPrivateRoomCountAsync(string userId)
        {
            return await context.ChatRoomMembers.CountAsync(m => m.UserId == userId && m.ChatRoom.RoomType == ChatRoomType.Private && m.LastVisitedAt == null);
        }

        public async Task MarkRoomAsVisitedAsync(int roomId, string userId)
        {
            await context.ChatRoomMembers
                .Where(m => m.ChatRoomId == roomId && m.UserId == userId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.LastVisitedAt, DateTime.UtcNow));
        }

        public async Task DeletePrivateRoomAsync(int roomId, string userId)
        {
            ChatRoom? room = await context.ChatRooms
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
                throw new KeyNotFoundException("Chat room not found.");

            if (room.RoomType != ChatRoomType.Private)
                throw new InvalidOperationException("Only private chatrooms can be deleted.");

            if (room.OwnerUserId != userId)
                throw new UnauthorizedAccessException("Only the chatroom owner can delete this chatroom.");

            context.ChatRooms.Remove(room);

            await context.SaveChangesAsync();
        }

        // -------------------------
        // Membership
        // -------------------------
        public async Task<ChatRoomMember?> GetMembershipAsync(int roomId, string userId)
        {
            return await context.ChatRoomMembers.FirstOrDefaultAsync(m => m.ChatRoomId == roomId && m.UserId == userId);
        }

        public async Task<IEnumerable<ChatRoomMember>> GetMembersAsync(int roomId)
        {
            return await context.ChatRoomMembers
                .AsNoTracking()
                .Where(m => m.ChatRoomId == roomId)
                .OrderBy(m => m.JoinedAt)
                .ToListAsync();
        }

        public async Task AddMemberAsync(ChatRoomMember member)
        {
            context.ChatRoomMembers.Add(member);

            await context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(ChatRoomMember member)
        {
            context.ChatRoomMembers.Remove(member);

            await context.SaveChangesAsync();
        }

        // -------------------------
        // Messages
        // -------------------------
        public async Task<IEnumerable<ChatMessage>> GetLatestMessagesAsync(int roomId, int count)
        {
            return await context.ChatMessages
                .AsNoTracking()
                .Where(m => m.ChatRoomId == roomId)
                .OrderByDescending(m => m.Id)
                .Take(count)
                .OrderBy(m => m.Id)
                .ToListAsync();
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            await using IDbContextTransaction? transaction = await context.Database.BeginTransactionAsync();

            try
            {
                context.ChatMessages.Add(message);

                await context.SaveChangesAsync();

                List<long> oldMessageIds = await context.ChatMessages
                    .Where(m => m.ChatRoomId == message.ChatRoomId)
                    .OrderByDescending(m => m.Id)
                    .Skip(100)
                    .Select(m => m.Id)
                    .ToListAsync();

                if (oldMessageIds.Count > 0)
                {
                    await context.ChatMessages.Where(m => oldMessageIds.Contains(m.Id)).ExecuteDeleteAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
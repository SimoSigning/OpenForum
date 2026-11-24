using EntityContext;
using EntityModels;
using Microsoft.EntityFrameworkCore;
using Repository.interfaces;

namespace Repository
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

        public async Task<IEnumerable<ChatRoom>> GetRoomsForUserAsync(
            string userId)
        {
            return await context.ChatRooms
                .AsNoTracking()
                .Include(r => r.Members)
                .Where(r =>
                    r.RoomType == ChatRoomType.Private &&
                    r.Members.Any(m => m.UserId == userId))
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

        public async Task AddRoomAsync(ChatRoom room)
        {
            context.ChatRooms.Add(room);

            await context.SaveChangesAsync();
        }

        public async Task DeleteRoomAsync(ChatRoom room)
        {
            context.ChatRooms.Remove(room);

            await context.SaveChangesAsync();
        }

        public async Task CreateRoomWithMembersAsync(
            ChatRoom room,
            IEnumerable<ChatRoomMember> members)
        {
            bool ownsTransaction =
                context.Database.CurrentTransaction == null;

            await using var transaction = ownsTransaction
                ? await context.Database.BeginTransactionAsync()
                : null;

            try
            {
                context.ChatRooms.Add(room);

                foreach (ChatRoomMember member in members)
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
                if (ownsTransaction && transaction != null)
                {
                    await transaction.RollbackAsync();
                }

                throw;
            }
        }


        public async Task LeaveRoomAsync(
            int roomId,
            string userId,
            string? newOwnerUserId,
            bool deleteRoom)
        {
            await using var transaction =
                await context.Database.BeginTransactionAsync();

            try
            {
                ChatRoom? room = await context.ChatRooms
                    .FirstOrDefaultAsync(r => r.Id == roomId);

                if (room == null)
                    throw new KeyNotFoundException("Chat room not found.");

                ChatRoomMember? member = await context.ChatRoomMembers
                    .FirstOrDefaultAsync(m =>
                        m.ChatRoomId == roomId &&
                        m.UserId == userId);

                if (member == null)
                    throw new InvalidOperationException(
                        "You are not a member of this chat room.");

                if (deleteRoom)
                {
                    context.ChatRooms.Remove(room);
                }
                else
                {
                    if (room.RoomType == ChatRoomType.Private &&
                        room.OwnerUserId == userId)
                    {
                        if (string.IsNullOrWhiteSpace(newOwnerUserId))
                            throw new InvalidOperationException(
                                "A new owner must be specified.");

                        bool newOwnerIsMember =
                            await context.ChatRoomMembers.AnyAsync(m =>
                                m.ChatRoomId == roomId &&
                                m.UserId == newOwnerUserId);

                        if (!newOwnerIsMember)
                            throw new InvalidOperationException(
                                "The new owner must be a remaining room member.");

                        room.OwnerUserId = newOwnerUserId;
                    }

                    context.ChatRoomMembers.Remove(member);
                }

                await context.SaveChangesAsync();

                await transaction.CommitAsync();
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
            return await context.Topics
                .AnyAsync(t => t.Id == topicId);
        }

        // -------------------------
        // Membership
        // -------------------------

        public async Task<ChatRoomMember?> GetMembershipAsync(int roomId, string userId)
        {
            return await context.ChatRoomMembers
                .FirstOrDefaultAsync(m =>
                    m.ChatRoomId == roomId &&
                    m.UserId == userId);
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
            await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

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
                    await context.ChatMessages
                        .Where(m => oldMessageIds.Contains(m.Id))
                        .ExecuteDeleteAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // -------------------------
        // Ownership
        // -------------------------

        public async Task UpdateRoomAsync(ChatRoom room)
        {
            context.ChatRooms.Update(room);

            await context.SaveChangesAsync();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models; // Message

namespace UrDoggy.Data.Repositories
{
    
    public class MessageRepositpry
    {
        private readonly ApplicationDbContext _context;
        public MessageRepositpry(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<int>> GetChatPartnerIds(int me)
        {
            return await _context.Messages
                .AsNoTracking()
                .Where(m => m.SenderId == me || m.ReceiverId == me)
                .Select(m => m.SenderId == me ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Message?> GetLastMessage(int userA, int userB)
        {
            return await _context.Messages
                .AsNoTracking()
                .Where(m => 
                    (m.SenderId == userA && m.ReceiverId == userB) 
                    || (m.SenderId == userB && m.ReceiverId == userA))
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Message>> GetThread(int userA, int userB)
        {
            return await _context.Messages
                .AsNoTracking()
                .Where(m=> 
                    (m.SenderId == userA && m.ReceiverId == userB)
                    || (m.SenderId == userB && m.ReceiverId == userA))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task SendMessage (int fromUser, int toUser, string content)
        {
            var message = new Message
            {
                SenderId = fromUser,
                ReceiverId = toUser,
                Content = content,
                SentAt = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMessage (int userId, int MessageId)
        {
            var message = await _context.Messages
                            .FirstOrDefaultAsync(m => 
                            m.Id == MessageId &&
                            m.SenderId == userId);
            if (message != null)
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAsRead(int currentUser, int SenderUser)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m =>
                m.ReceiverId == currentUser && m.SenderId == SenderUser);
            if (message != null)
            {
                message.IsRead = true;
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();
            }
        }
    }
}

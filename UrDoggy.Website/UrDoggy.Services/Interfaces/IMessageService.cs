using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Services.Interfaces
{
    public interface IMessageService
    {
        Task<List<int>> GetChatPartnerIds(int userId);
        Task<Message> GetLastMessage(int userA, int userB);
        Task<List<Message>> GetThread(int userA, int userB, int pageNumber = 1, int pageSize = 20);
        Task<Message> SendMessage(int fromUser, int toUser, string content);
        Task DeleteMessage(int userId, int messageId);
        Task MarkAsRead(int currentUser, int senderUser);
        Task<int> GetUnreadCount(int userId, int partnerId);
        Task<List<ConversationDto>> GetConversations(int userId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;
using UrDoggy.Data.Repositories;
using UrDoggy.Services.Interfaces;

namespace UrDoggy.Services.Service
{
    public class MessageService : IMessageService
    {
        private readonly MessageRepositpry _messageRepository;
        private readonly UserRepository _userRepository;

        public MessageService(MessageRepositpry messageRepository, UserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public async Task<List<int>> GetChatPartnerIds(int userId)
        {
            return await _messageRepository.GetChatPartnerIds(userId);
        }

        public async Task<Message> GetLastMessage(int userA, int userB)
        {
            return await _messageRepository.GetLastMessage(userA, userB);
        }

        public async Task<List<Message>> GetThread(int userA, int userB, int pageNumber = 1, int pageSize = 20)
        {
            var allMessages = await _messageRepository.GetThread(userA, userB);
            return allMessages
                .OrderByDescending(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<Message> SendMessage(int fromUser, int toUser, string content)
        {
            await _messageRepository.SendMessage(fromUser, toUser, content);

            // Lấy message vừa gửi để trả về
            var lastMessage = await _messageRepository.GetLastMessage(fromUser, toUser);
            return lastMessage;
        }

        public async Task DeleteMessage(int userId, int messageId)
        {
            await _messageRepository.DeleteMessage(userId, messageId);
        }

        public async Task MarkAsRead(int currentUser, int senderUser)
        {
            await _messageRepository.MarkAsRead(currentUser, senderUser);
        }

        public async Task<int> GetUnreadCount(int userId, int partnerId)
        {
            var messages = await _messageRepository.GetThread(userId, partnerId);
            return messages.Count(m => m.ReceiverId == userId && !m.IsRead);
        }

        public async Task<List<ConversationDto>> GetConversations(int userId)
        {
            var partnerIds = await GetChatPartnerIds(userId);
            var conversations = new List<ConversationDto>();

            foreach (var partnerId in partnerIds)
            {
                var lastMessage = await GetLastMessage(userId, partnerId);
                var partner = await _userRepository.GetByI(partnerId);

                if (lastMessage != null && partner != null)
                {
                    conversations.Add(new ConversationDto
                    {
                        OtherId = partnerId,
                        OtherUsername = partner.UserName,
                        OtherPicture = partner.ProfilePicture,
                        Preview = lastMessage.Content,
                        PreviewSenderId = lastMessage.SenderId,
                        Time = lastMessage.SentAt
                    });
                }
            }

            return conversations.OrderByDescending(c => c.Time).ToList();
        }
    }
}

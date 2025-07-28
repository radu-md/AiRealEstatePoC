using System;
using System.Linq;
using Xunit;
using AiRealEstate.Core.Services;
using AiRealEstate.Core.Models;

namespace AiRealEstate.Tests.CoreTests
{
    public class ConversationStateServiceTests
    {
        private readonly ConversationStateService _service = new ConversationStateService();
        private const string SessionId = "session1";

        [Fact]
        public void GetHistory_NewSession_ShouldReturnEmptyList()
        {
            var history = _service.GetHistory(SessionId);
            Assert.NotNull(history);
            Assert.Empty(history);
        }

        [Fact]
        public void AddMessage_ShouldAddCleanedMessageToHistory()
        {
            var rawContent = "  Hello World  ";
            var message = new ChatMessage { Role = "user", Content = rawContent };

            _service.AddMessage(SessionId, message);
            var history = _service.GetHistory(SessionId);

            Assert.Single(history);
            var stored = history.First();
            Assert.Equal("user", stored.Role);
            Assert.Equal("Hello World", stored.Content);
        }

        [Fact]
        public void AddMessage_WhitespaceOnlyContent_ShouldStoreEmptyContent()
        {
            var message = new ChatMessage { Role = "assistant", Content = "   \t\n" };
            _service.AddMessage(SessionId, message);
            var history = _service.GetHistory(SessionId);

            Assert.Single(history);
            Assert.Equal(string.Empty, history.First().Content);
        }

        [Fact]
        public void AddMessage_MoreThanLimit_ShouldKeepLast10Messages()
        {
            // Add 12 messages
            for (int i = 1; i <= 12; i++)
            {
                var msg = new ChatMessage { Role = "user", Content = $"msg{i}" };
                _service.AddMessage(SessionId, msg);
            }
            var history = _service.GetHistory(SessionId);
            // Only last 10 should be kept (msg3 to msg12)
            Assert.Equal(10, history.Count);
            Assert.Equal("msg3", history.First().Content);
            Assert.Equal("msg12", history.Last().Content);
        }

        [Fact]
        public void Clear_ShouldRemoveAllHistory()
        {
            var msg = new ChatMessage { Role = "user", Content = "test" };
            _service.AddMessage(SessionId, msg);
            Assert.Single(_service.GetHistory(SessionId));

            _service.Clear(SessionId);
            var historyAfterClear = _service.GetHistory(SessionId);
            Assert.Empty(historyAfterClear);
        }
    }
}
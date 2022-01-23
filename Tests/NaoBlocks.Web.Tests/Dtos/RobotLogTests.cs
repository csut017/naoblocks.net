using Transfer = NaoBlocks.Web.Dtos;
using Data = NaoBlocks.Engine.Data;
using Xunit;
using System;
using System.Linq;
using NaoBlocks.Common;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class RobotLogTests
    {
        [Fact]
        public void FromModelConvertsEntity()
        {
            var past = DateTime.Now.AddHours(-1);
            var now = DateTime.Now;
            var entity = new Data.RobotLog
            {
                WhenLastUpdated = now,
                WhenAdded = past
            };
            var dto = Transfer.RobotLog.FromModel(entity, false, null);
            Assert.Equal(now, dto.WhenLastUpdated);
            Assert.Equal(past, dto.WhenAdded);
            Assert.Null(dto.Lines);
            Assert.Null(dto.UserName);
            Assert.Equal(-1, dto.ConversationId);
        }

        [Fact]
        public void FromModelWithConversation()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotLog
            {
                WhenLastUpdated = now,
                WhenAdded = now,
                Conversation = new Data.Conversation
                {
                    ConversationId = 10,
                    SourceName = "Moana"
                }
            };
            var dto = Transfer.RobotLog.FromModel(entity, false, null);
            Assert.Equal(now, dto.WhenLastUpdated);
            Assert.Equal(now, dto.WhenAdded);
            Assert.Null(dto.Lines);
            Assert.Equal("Moana", dto.UserName);
            Assert.Equal(10, dto.ConversationId);
        }

        [Fact]
        public void FromModelWithAssociatedConversation()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotLog
            {
                WhenLastUpdated = now,
                WhenAdded = now
            };
            var conversation = new Data.Conversation
            {
                ConversationId = 5,
                SourceName = "Mia"
            };
            var dto = Transfer.RobotLog.FromModel(entity, false, conversation);
            Assert.Equal(now, dto.WhenLastUpdated);
            Assert.Equal(now, dto.WhenAdded);
            Assert.Null(dto.Lines);
            Assert.Equal("Mia", dto.UserName);
            Assert.Equal(5, dto.ConversationId);
        }

        [Fact]
        public void FromModelWithBothConversations()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotLog
            {
                WhenLastUpdated = now,
                WhenAdded = now,
                Conversation = new Data.Conversation
                {
                    ConversationId = 10,
                    SourceName = "Moana"
                }
            };
            var conversation = new Data.Conversation
            {
                ConversationId = 5,
                SourceName = "Mia"
            };
            var dto = Transfer.RobotLog.FromModel(entity, false, conversation);
            Assert.Equal(now, dto.WhenLastUpdated);
            Assert.Equal(now, dto.WhenAdded);
            Assert.Null(dto.Lines);
            Assert.Equal("Mia", dto.UserName);
            Assert.Equal(5, dto.ConversationId);
        }

        [Fact]
        public void FromModelConvertsEntityWithLines()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotLog
            {
                WhenLastUpdated = now,
                WhenAdded = now
            };
            entity.Lines.Add(new Data.RobotLogLine());
            var dto = Transfer.RobotLog.FromModel(entity, true, null);
            Assert.Equal(now, dto.WhenLastUpdated);
            Assert.Equal(now, dto.WhenAdded);
            Assert.NotEmpty(dto.Lines);
            Assert.Null(dto.UserName);
            Assert.Equal(-1, dto.ConversationId);
        }
    }
}

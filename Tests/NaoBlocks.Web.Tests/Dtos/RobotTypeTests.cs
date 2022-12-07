﻿using NaoBlocks.Common;
using System;
using System.Linq;
using Xunit;
using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class RobotTypeTests
    {
        [Fact]
        public void FromModelConvertsEntity()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotType
            {
                Name = "karetao",
                IsDefault = true,
                WhenAdded = now
            };
            var dto = Transfer.RobotType.FromModel(entity);
            Assert.Equal("karetao", dto.Name);
            Assert.True(dto.IsDefault);
        }

        [Fact]
        public void FromModelConvertsEntityAndTemplates()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotType
            {
                Name = "karetao",
                IsDefault = true,
                WhenAdded = now
            };
            entity.LoggingTemplates.Add(
                new Data.LoggingTemplate { Category = "action", Text = "Action", MessageType = ClientMessageType.RobotAction });
            var dto = Transfer.RobotType.FromModel(entity, Transfer.DetailsType.Standard);
            Assert.Equal(
                new[] { "action->Action[RobotAction]" },
                dto.Templates?.Select(lt => $"{lt.Category}->{lt.Text}[{lt.MessageType}]").ToArray());
        }

        [Fact]
        public void FromModelConvertsEntityAndToolboxes()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotType
            {
                Name = "karetao",
                IsDefault = true,
                WhenAdded = now
            };
            entity.Toolboxes.Add(new Data.Toolbox { Name = "blocks" });
            var dto = Transfer.RobotType.FromModel(entity, Transfer.DetailsType.Standard);
            Assert.Equal(
                new[] { "blocks" },
                dto.Toolboxes?.Select(t => t.Name).ToArray());
        }

        [Fact]
        public void FromModelConvertsEntityAndValues()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotType
            {
                Name = "karetao",
                IsDefault = true,
                WhenAdded = now
            };
            entity.CustomValues.Add(new Data.NamedValue { Name = "One", Value = "Tahi" });
            var dto = Transfer.RobotType.FromModel(entity, Transfer.DetailsType.Standard);
            Assert.Equal(
                new[] { "One->Tahi" },
                dto.CustomValues?.Select(cv => $"{cv.Name}->{cv.Value}").ToArray());
        }

        [Fact]
        public void FromModelConvertsFullDetails()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotType
            {
                Name = "karetao",
                IsDefault = true,
                WhenAdded = now,
                Message = "This is a test message"
            };
            entity.CustomValues.Add(new Data.NamedValue { Name = "One", Value = "Tahi" });
            entity.LoggingTemplates.Add(
               new Data.LoggingTemplate { Category = "action", Text = "Action", MessageType = ClientMessageType.RobotAction });
            entity.Toolboxes.Add(new Data.Toolbox { Name = "blocks" });
            var dto = Transfer.RobotType.FromModel(entity, Transfer.DetailsType.Parse | Transfer.DetailsType.Standard);
            Assert.Equal(
                "This is a test message",
                dto.Parse?.Message);
            Assert.Equal(
               new[] { "One->Tahi" },
               dto.CustomValues?.Select(cv => $"{cv.Name}->{cv.Value}").ToArray());
            Assert.Equal(
               new[] { "blocks" },
               dto.Toolboxes?.Select(t => t.Name).ToArray());
            Assert.Equal(
               new[] { "action->Action[RobotAction]" },
               dto.Templates?.Select(lt => $"{lt.Category}->{lt.Text}[{lt.MessageType}]").ToArray());
        }

        [Fact]
        public void FromModelConvertsParseDetails()
        {
            var now = DateTime.Now;
            var entity = new Data.RobotType
            {
                Name = "karetao",
                IsDefault = true,
                WhenAdded = now,
                Message = "This is a test message",
                IsDuplicate = true
            };
            var dto = Transfer.RobotType.FromModel(entity, Transfer.DetailsType.Parse);
            Assert.Equal(
                "This is a test message",
                dto.Parse?.Message);
            Assert.Equal(
                true,
                dto?.Parse?.Details["duplicate"]);
        }
    }
}
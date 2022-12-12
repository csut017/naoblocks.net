﻿using System;
using Xunit;
using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class StudentTests
    {
        [Fact]
        public void FromModelConvertsEntity()
        {
            var now = DateTime.Now;
            var entity = new Data.User
            {
                Name = "Moana",
                Role = Data.UserRole.Teacher,
                WhenAdded = now
            };
            var dto = Transfer.Student.FromModel(entity);
            Assert.Equal("Moana", dto.Name);
            Assert.Null(dto.Role);
            Assert.Equal(now, dto.WhenAdded);
        }

        [Fact]
        public void FromModelConvertsEntityWithDetails()
        {
            var now = DateTime.Now;
            var entity = new Data.User
            {
                Name = "Moana",
                Role = Data.UserRole.Teacher,
                WhenAdded = now,
                Settings = new Data.UserSettings()
            };
            var dto = Transfer.Student.FromModel(entity, Transfer.DetailsType.Standard);
            Assert.Same(dto.Settings, entity.Settings);
        }

        [Fact]
        public void FromModelIncludesStudentDetails()
        {
            var now = DateTime.Now;
            var entity = new Data.User
            {
                Name = "Moana",
                Role = Data.UserRole.Student,
                WhenAdded = now,
                StudentDetails = new Data.StudentDetails
                {
                    Age = 6,
                    Gender = "Male"
                }
            };
            var dto = Transfer.Student.FromModel(entity);
            Assert.Equal("Moana", dto.Name);
            Assert.Equal(now, dto.WhenAdded);
            Assert.Equal(6, dto.Age);
            Assert.Equal("Male", dto.Gender);
        }
    }
}
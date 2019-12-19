using NaoBlocks.Core.Models;
using Xunit;

using Data = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class TeacherTests
    {
        [Fact]
        public void FromModelHandlesNull()
        {
            var dto = Data.Teacher.FromModel(null);
            Assert.Null(dto);
        }

        [Fact]
        public void FromModelSetsProperties()
        {
            var user = new User
            {
                Name = "The User"
            };
            var dto = Data.Teacher.FromModel(user);
            Assert.Equal(user.Name, dto.Name);
        }
    }
}
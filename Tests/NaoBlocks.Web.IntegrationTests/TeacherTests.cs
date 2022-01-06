using Microsoft.AspNetCore.Mvc.Testing;
using NaoBlocks.Engine.Data;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.IntegrationTests
{
    public class TeacherTests
        : IntegrationSecurityTestHelper
    {
        public TeacherTests(WebApplicationFactory<Program> factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData("version")]
        [InlineData("whoami")]
        [InlineData("clients/robot")]
        [InlineData("clients/2/logs", HttpStatusCode.NotFound)]
        public async Task GetCanAccessAuthorizedApi(string url, HttpStatusCode expectedCode = HttpStatusCode.OK)
        {
            await RunSecurityTest(UserRole.Teacher, url, expectedCode);
        }

        [Theory]
        [InlineData("users")]
        [InlineData("users/mia")]
        public async Task GetFailsWithAuthorizedApi(string url)
        {
            await RunSecurityTest(UserRole.Teacher, url, HttpStatusCode.Forbidden);
        }
    }
}

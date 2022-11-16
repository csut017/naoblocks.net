using NaoBlocks.Engine.Data;
using System.Net;
using Xunit;

namespace NaoBlocks.Web.IntegrationTests
{
    public abstract class SecurityTestsBase
        : TheoryData<UserRole, string, HttpStatusCode, bool>
    {
        public void AddUrlsToCheck(UserRole role, HttpStatusCode expectedCode, params string[] urls)
        {
            foreach (var url in urls)
            {
                Add(role, url, expectedCode, true);
            }
        }

        public void AddNonJsonUrlsToCheck(UserRole role, HttpStatusCode expectedCode, params string[] urls)
        {
            foreach (var url in urls)
            {
                Add(role, url, expectedCode, false);
            }
        }
    }
}

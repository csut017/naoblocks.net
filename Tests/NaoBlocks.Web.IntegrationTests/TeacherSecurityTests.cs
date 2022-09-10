using NaoBlocks.Engine.Data;
using System.Net;

namespace NaoBlocks.Web.IntegrationTests
{
    public class TeacherSecurityTests
        : SecurityTestsBase
    {
        public TeacherSecurityTests()
        {
            // Allowed teacher requests
            AddUrlsToCheck(
                UserRole.Teacher,
                HttpStatusCode.OK,
                "/api/v1/Clients/robot",
                "/api/v1/users/mia/Programs",
                "/api/v1/Programs",
                "/api/v1/Robots",
                "/api/v1/robots/types",
                "/api/v1/Session",
                "/api/v1/Session/settings",
                "/api/v1/Students",
                "/api/v1/system/addresses",
                "/api/v1/system/config",
                "/api/v1/whoami",
                "/api/v1/version");

            // Allowed teacher requests that fail due to a lack of data (should add proper tests at some point)
            AddUrlsToCheck(
                UserRole.Teacher,
                HttpStatusCode.NotFound,
                "/api/v1/Clients/6/logs",
                "/api/v1/robots/types/export/package/1",
                "/api/v1/robots/types/karetao/blocksets",
                "/api/v1/users/mia/Programs/3",
                "/api/v1/robots/types/karetao",
                "/api/v1/Robots/karetao",
                "/api/v1/Students/mia",
                "/api/v1/robots/karetao/Logs/1",
                "/api/v1/Code/mia/1",
                "/api/v1/Students/1/logs/export",
                "/api/v1/Students/1/snapshots/export",
                "/api/v1/Programs/3");

            // Add the non-JSON requests
            AddNonJsonUrlsToCheck(
                UserRole.Teacher,
                HttpStatusCode.OK,
                "/api/v1/Robots/export/list",
                "/api/v1/Students/export",
                "/api/v1/system/addresses/connect.txt",
                "/api/v1/Students/mia/export",
                "/api/v1/Students/mia/export.txt");

            // Forbidden teacher requests
            AddUrlsToCheck(
                UserRole.Teacher,
                HttpStatusCode.Forbidden,
                "/api/v1/Teachers/mia",
                "/api/v1/Teachers",
                "/api/v1/Users/mia",
                "/api/v1/Users");
        }
    }
}
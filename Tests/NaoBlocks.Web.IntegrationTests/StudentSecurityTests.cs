﻿using NaoBlocks.Engine.Data;
using System.Net;

namespace NaoBlocks.Web.IntegrationTests
{
    public class StudentSecurityTests
        : SecurityTestsBase
    {
        public StudentSecurityTests()
        {
            // Allowed student requests
            AddUrlsToCheck(
                UserRole.Student,
                HttpStatusCode.OK,
                "/api/v1/users/mia/Programs",
                "/api/v1/Programs",
                "/api/v1/Robots",
                "/api/v1/robots/types",
                "/api/v1/Session",
                "/api/v1/Session/settings",
                "/api/v1/system/addresses",
                "/api/v1/system/config",
                "/api/v1/whoami",
                "/api/v1/version");

            // Allowed student requests that fail due to a lack of data (should add proper tests at some point)
            AddUrlsToCheck(
                UserRole.Student,
                HttpStatusCode.NotFound,
                "/api/v1/robots/types/karetao/blocksets",
                "/api/v1/users/mia/Programs/3",
                "/api/v1/robots/types/karetao",
                "/api/v1/Robots/karetao",
                "/api/v1/Programs/3");

            // Add the non-JSON requests
            AddNonJsonUrlsToCheck(
                UserRole.Student,
                HttpStatusCode.OK,
                "/api/v1/system/addresses/connect.txt");

            // Forbidden student requests
            AddUrlsToCheck(
                UserRole.Student,
                HttpStatusCode.Forbidden,
                "/api/v1/Clients/robot",
                "/api/v1/Clients/6/logs",
                "/api/v1/Code/mia/1",
                "/api/v1/robots/karetao/Logs/1",
                "/api/v1/robots/types/1/export/logs",
                "/api/v1/robots/types/1/export/logs.csv",
                "/api/v1/robots/types/1/export/package",
                "/api/v1/robots/types/1/export/package.zip",
                "/api/v1/Students",
                "/api/v1/Students/export",
                "/api/v1/Students/mia",
                "/api/v1/Students/mia/export",
                "/api/v1/Students/1/logs/export",
                "/api/v1/Students/1/snapshots/export",
                "/api/v1/Teachers/mia",
                "/api/v1/Teachers",
                "/api/v1/Users/mia",
                "/api/v1/Users");
        }
    }
}
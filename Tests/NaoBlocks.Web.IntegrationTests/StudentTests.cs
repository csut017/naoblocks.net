﻿using Microsoft.AspNetCore.Mvc.Testing;
using NaoBlocks.Engine.Data;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.IntegrationTests
{
    public class StudentTests
        : IntegrationSecurityTestHelper
    {
        public StudentTests(WebApplicationFactory<Program> factory)
            : base(factory) 
        {
        }


        [Theory]
        [InlineData("version")]
        [InlineData("whoami")]
        public async Task GetCanAccessAuthorizedApi(string url, HttpStatusCode expectedCode = HttpStatusCode.OK)
        {
            await RunSecurityTest(UserRole.Student, url, expectedCode);
        }

        [Theory]
        [InlineData("clients/robot")]
        [InlineData("clients/2/logs")]
        [InlineData("users")]
        [InlineData("users/mia")]
        public async Task GetFailsWithAuthorizedApi(string url)
        {
            await RunSecurityTest(UserRole.Student, url, HttpStatusCode.Forbidden);
        }
    }
}

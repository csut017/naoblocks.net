using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Core.Models;
using Raven.Client.Documents.Session;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Tests.Controllers
{
    public static class Utils
    {
        public static void InitialiseUser(Mock<IAsyncDocumentSession> sessionMock, ControllerBase controller, User user)
        {
            var identity = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                });
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            sessionMock.Setup(s => s.LoadAsync<User>(user.Id, CancellationToken.None))
                .Returns(Task.FromResult(user));
        }
    }
}
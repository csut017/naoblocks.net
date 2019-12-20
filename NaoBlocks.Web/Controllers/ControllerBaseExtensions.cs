using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Core.Models;
using Raven.Client.Documents.Session;
using System;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Controllers
{
    internal static class ControllerBaseExtensions
    {
        public static async Task<User?> LoadUser(this ControllerBase? controller, IAsyncDocumentSession? session)
        {
            if (controller is null) throw new ArgumentNullException(nameof(controller));
            if (session is null) throw new ArgumentNullException(nameof(session));

            var userId = controller.User?.Identity?.Name;
            if (userId == null) return null;

            var user = await session.LoadAsync<User>(userId).ConfigureAwait(false);
            return user;
        }
    }
}
using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands.Helpers
{
    static class UserHelper
    {
        public static async Task<User?> ValidateAndRetrieveUser(this CommandBase command, IAsyncDocumentSession? session, string? name, UserRole? role, List<CommandError> errors)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var roleName = role?.ToString() ?? "User";
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(command.GenerateError($"{roleName} name is required"));
            }

            User? person = null;
            if (!errors.Any())
            {
                person = role == null
                    ? await session.Query<User>()
                                .FirstOrDefaultAsync(u => u.Name == name)
                                .ConfigureAwait(false)
                    : await session.Query<User>()
                                .FirstOrDefaultAsync(u => u.Name == name && u.Role == role)
                                .ConfigureAwait(false);
                if (person == null) errors.Add(command.GenerateError($"{roleName} {name} does not exist"));
            }

            return person;
        }
    }
}

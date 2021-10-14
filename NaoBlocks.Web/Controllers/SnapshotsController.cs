using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Common;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Commands = NaoBlocks.Core.Commands;
using Generators = NaoBlocks.Core.Generators;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class SnapshotsController : ControllerBase
    {
        private readonly ILogger<SnapshotsController> _logger;
        private readonly Commands.ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;

        public SnapshotsController(ILogger<SnapshotsController> logger, Commands.ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        [HttpPost]
        public async Task<ActionResult<ExecutionResult<Dtos.Snapshot>>> Post(Dtos.Snapshot value)
        {
            if (value == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing snapshot details"
                });
            }

            var currentUser = await this.LoadUser(session);
            if (currentUser == null) return Unauthorized();

            this._logger.LogInformation($"Adding new snapshot for '{currentUser.Name}'");
            var command = new Commands.StoreSnapshot
            {
                Source = value.Source,
                UserId = currentUser.Id,
                State = value.State
            };
            if (value.Values != null)
            {
                foreach (var subValue in value.Values)
                {
                    command.Values.Add(new NamedValue
                    {
                        Name = subValue.Name,
                        Value = subValue.Value
                    });
                }
            }

            return await this.commandManager.ExecuteForHttp(command, v => Dtos.Snapshot.FromModel(v) ?? new Dtos.Snapshot());
        }
    }
}
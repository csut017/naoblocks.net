using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaoBlocks.Client;
using NaoBlocks.Common;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// This controller allows for polling from a remote robot (e.g. the MBot2).
    /// </summary>
    /// <remarks>
    /// Some robots do not have the ability to run a web socket connection (either no library or insufficient resources.)
    /// </remarks>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RemoteController : Controller
    {
        private readonly ILogger<RemoteController> logger;
        private readonly IRemoteStore remoteStore;

        public RemoteController(ILogger<RemoteController> logger, IRemoteStore remoteStore)
        {
            this.logger = logger;
            this.remoteStore = remoteStore;
        }

        /// <summary>
        /// Checks if there is any messages waiting and returns them.
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="robot">The name of the robot.</param>
        /// <returns>The message details if successful, "FAIL" otherwise.</returns>
        [HttpGet("{robot}/check")]
        public Task<IActionResult> Check(string robot)
        {
            this.logger.LogInformation($"Checking for messages for {robot}");
            var response = this.remoteStore.CheckRemote(robot);
            return Task.FromResult((IActionResult)Content(response));
        }

        /// <summary>
        /// Start the internal connection. This method will make the robot internally visible.
        /// </summary>
        /// <param name="robot">The name of the robot.</param>
        /// <returns>"OK" if successful, "FAIL" otherwise.</returns>
        [HttpGet("{robot}/start")]
        public async Task<IActionResult> Start(string robot, string? password)
        {
            this.logger.LogInformation($"Starting remote {robot}");
            var response = await this.remoteStore.StartAsync(robot, password ?? string.Empty);
            return Content(response);
        }

        /// <summary>
        /// Stops the internal connection and cleans up all the internal state.
        /// </summary>
        /// <param name="robot">The name of the robot.</param>
        /// <returns>"OK" if successful, "FAIL" otherwise.</returns>
        [HttpGet("{robot}/stop")]
        public async Task<IActionResult> Stop(string robot)
        {
            this.logger.LogInformation($"Stopping remote {robot}");
            var response = await this.remoteStore.StopAsync(robot);
            return Content(response);

        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Dtos;
using NaoBlocks.Web.Helpers;

using Data = NaoBlocks.Engine.Data;


namespace NaoBlocks.Web.Controllers
{
    /// <summary>
    /// A controller for working with UI files.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UiController: ControllerBase
    {
        private readonly ILogger<UiController> logger;
        private readonly IExecutionEngine executionEngine;
        private readonly UiManager uiManager;

        /// <summary>
        /// Initialises a new <see cref="UiController"/> instance.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="executionEngine">The execution engine for processing commands and queries.</param>
        /// <param name="uiManager">A <see cref="UiManager"/> instance for parsing incoming data.</param>
        public UiController(ILogger<UiController> logger, IExecutionEngine executionEngine, UiManager uiManager)
        {
            this.logger = logger;
            this.executionEngine = executionEngine;
            this.uiManager = uiManager;
        }

        /// <summary>
        /// Retrieves a component for a UI.
        /// </summary>
        /// <param name="name">The name of the UI to retrieve the component for.</param>
        /// <param name="component">The name of the component.</param>
        /// <returns>Either a 404 (not found) or the component.</returns>
        [HttpGet("{name}/{component}")]
        public async Task<ActionResult> GetComponent(string name, string component)
        {
            this.logger.LogDebug($"Retrieving component {component} for {name}");

            var definition = await this.executionEngine
                .Query<UIDefinitionData>()
                .RetrieveByNameAsync(name)
                .ConfigureAwait(false);
            if (definition == null)
            {
                return NotFound();
            }

            var stream = await definition.Definition!.GenerateAsync(component);
            return File(stream, ContentTypes.Txt);
        }
    }
}

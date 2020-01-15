using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class TutorialsController : ControllerBase
    {
        private readonly ILogger<TutorialsController> _logger;
        private readonly ICommandManager commandManager;
        private readonly IAsyncDocumentSession session;

        public TutorialsController(ILogger<TutorialsController> logger, ICommandManager commandManager, IAsyncDocumentSession session)
        {
            this._logger = logger;
            this.commandManager = commandManager;
            this.session = session;
        }

        [HttpDelete("{category}/{id}")]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<Dtos.ExecutionResult>> Delete(string category, string id)
        {
            this._logger.LogInformation($"Deleting tutorial '{id}' in '{ category}'");
            var command = new DeleteTutorialCommand
            {
                Name = id,
                Category = category
            };
            return await this.commandManager.ExecuteForHttp(command);
        }

        [HttpGet("{category}/{id}")]
        public async Task<ActionResult<Dtos.Tutorial>> GetTutorial(string category, string id)
        {
            this._logger.LogDebug($"Retrieving tutorial '{id}' in '{category}'");
            var queryable = this.session.Query<Tutorial>();
            var tutorial = await queryable.FirstOrDefaultAsync(t => t.Category == category && t.Name == id);
            if (tutorial == null)
            {
                return NotFound();
            }

            this._logger.LogDebug("Retrieved tutorial");
            return Dtos.Tutorial.FromModel(tutorial);
        }

        [HttpGet]
        public async Task<Dtos.ListResult<Dtos.Tutorial>> GetTutorials(int? page, int? size)
        {
            return await GenerateList(this.session.Query<Tutorial>(), page, size);
        }

        [HttpGet("{category}")]
        public async Task<Dtos.ListResult<Dtos.Tutorial>> GetTutorialsForCategory(string category, int? page, int? size)
        {
            IRavenQueryable<Tutorial> query = this.session.Query<Tutorial>()
                .Where(t => t.Category == category);
            return await GenerateList(query, page, size);
        }

        [HttpPost]
        [Authorize(Policy = "Teacher")]
        public async Task<ActionResult<Dtos.ExecutionResult<Dtos.Tutorial>>> Post(Dtos.Tutorial tutorial)
        {
            if (tutorial == null)
            {
                return this.BadRequest(new
                {
                    Error = "Missing tutorial details"
                });
            }

            this._logger.LogInformation($"Adding new tutorial '{tutorial.Name}' in '{tutorial.Category}'");
            var command = new AddTutorialCommand
            {
                Category = tutorial.Category,
                Name = tutorial.Name,
                Order = tutorial.Order
            };
            if (tutorial.Exercises != null)
            {
                foreach (var exercise in tutorial.Exercises)
                {
                    command.Exercises.Add(Dtos.TutorialExercise.FromDto(exercise));
                }
            }

            return await this.commandManager.ExecuteForHttp(command, t => Dtos.Tutorial.FromModel(t));
        }

        private async Task<Dtos.ListResult<Dtos.Tutorial>> GenerateList(IRavenQueryable<Tutorial> query, int? page, int? size)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;

            this._logger.LogDebug($"Retrieving tutorials: page {pageNum} with size {pageSize}");
            var tutorials = await query.Statistics(out QueryStatistics stats)
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .Skip(pageNum * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var count = tutorials.Count;
            this._logger.LogDebug($"Retrieved {count} tutorials");
            var result = new Dtos.ListResult<Dtos.Tutorial>
            {
                Count = stats.TotalResults,
                Page = pageNum,
                Items = tutorials.Select(t => Dtos.Tutorial.FromModel(t, false))
            };
            return result;
        }

        //[HttpPut("{category}/{id}")]
        //[Authorize(Policy = "Teacher")]
        //public async Task<ActionResult<Dtos.ExecutionResult>> Put(string category, string id? id, Dtos.Tutorial? tutorial)
        //{
        //    if ((tutorial == null) || string.IsNullOrEmpty(id))
        //    {
        //        return this.BadRequest(new
        //        {
        //            Error = "Missing tutorial details"
        //        });
        //    }

        //    this._logger.LogInformation($"Updating tutorial '{id}'");
        //    var command = new UpdateTutorialCommand
        //    {
        //        CurrentMachineName = id,
        //        MachineName = tutorial.MachineName,
        //        FriendlyName = tutorial.FriendlyName,
        //        Password = tutorial.Password
        //    };
        //    return await this.commandManager.ExecuteForHttp(command);
        //}
    }
}
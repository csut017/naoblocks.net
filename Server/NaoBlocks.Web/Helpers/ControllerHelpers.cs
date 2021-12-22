using Microsoft.AspNetCore.Mvc;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Extension methods to assist with validating controller data.
    /// </summary>
    public static class ControllerHelpers
    {
        /// <summary>
        /// Attempts to validate the paging arguments.
        /// </summary>
        /// <param name="controller">The controller to use for retrieving the user.</param>
        /// <param name="page">The incoming page number.</param>
        /// <param name="size">The incoming page size.</param>
        /// <returns>The validated page number and size.</returns>
        public static (int, int) ValidatePageArguments(this ControllerBase controller, int? page, int? size)
        {
            var pageSize = size ?? 25;
            var pageNum = page ?? 0;
            if (pageSize > 100) pageSize = 100;
            if (pageSize < 0) pageSize = 25;

            return (pageNum, pageSize);
        }

        /// <summary>
        /// Retrieves the current user details.
        /// </summary>
        /// <param name="controller">The controller to use for retrieving the user.</param>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        /// <returns>A <see cref="User"/> instance, if found, otherwise null.</returns>
        public static async Task<User?> LoadUserAsync(this ControllerBase controller, IExecutionEngine engine)
        {
            var userId = controller.User?.Identity?.Name;
            if (userId == null) return null;

            var user = await engine.Query<UserData>()
                .RetrieveByNameAsync(userId)
                .ConfigureAwait(false);
            return user;
        }

        /// <summary>
        /// Attempt to convert an incoming format string into a valid <see cref="ReportFormat"/>.
        /// </summary>
        /// <param name="controller">The controller to use for retrieving the user.</param>
        /// <param name="format">The incoming format string.</param>
        /// <param name="output">The parsed <see cref="ReportFormat"/>.</param>
        /// <param name="defaultFormat">The default <see cref="ReportFormat"/> to use.</param>
        /// <returns>True if the conversion worked (or was missing), false otherwise.</returns>
        public static bool TryConvertFormat(this ControllerBase controller, string? format, out ReportFormat output, ReportFormat defaultFormat = ReportFormat.Excel)
        {
            output = defaultFormat;
            if (!string.IsNullOrEmpty(format))
            {
                if (!Enum.TryParse(format, true, out output))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to generate a report.
        /// </summary>
        /// <typeparam name="TGenerator">The report generator to use.</typeparam>
        /// <param name="controller">The controller attempting to generate the report.</param>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        /// <param name="format">The output format.</param>
        /// <param name="generate">An optional generation function.</param>
        /// <returns>A <see cref="ActionResult"/> containing the result of the validation and generation.</returns>
        public static async Task<ActionResult> GenerateReport<TGenerator>(
            this ControllerBase controller,
            IExecutionEngine engine,
            string? format,
            Func<TGenerator, ReportFormat, Task<Tuple<Stream, string>>>? generate = null)
           where TGenerator : ReportGenerator, new()
        {
            if (!controller.TryConvertFormat(format, out var reportFormat))
            {
                return controller.BadRequest(new
                {
                    Error = $"Unknown report format {format}"
                });
            }

            var generator = engine.Generator<TGenerator>();
            if (!generator.IsFormatAvailable(reportFormat))
            {
                return controller.BadRequest(new
                {
                    Error = $"Report format {format} is not available"
                });
            }

            var data = generate == null
                ? await generator.GenerateAsync(reportFormat)
                : await generate(generator, reportFormat);
            var contentType = ContentTypes.Convert(reportFormat);
            return controller.File(data.Item1, contentType, data.Item2);
        }

        /// <summary>
        /// Attempts to generate a report for a user.
        /// </summary>
        /// <typeparam name="TGenerator">The report generator to use.</typeparam>
        /// <param name="controller">The controller attempting to generate the report.</param>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        /// <param name="format">The output format.</param>
        /// <param name="name">The user's name.</param>
        /// <param name="generate">An optional generation function.</param>
        /// <returns>A <see cref="ActionResult"/> containing the result of the validation and generation.</returns>
        public static async Task<ActionResult> GenerateUserReport<TGenerator>(
            this ControllerBase controller,
            IExecutionEngine engine,
            string? format,
            string? name,
            Func<TGenerator, ReportFormat, Task<Tuple<Stream, string>>>? generate = null)
           where TGenerator : ReportGenerator, new()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return controller.BadRequest(new
                {
                    Error = "Missing student details"
                });
            }

            var student = await engine
                .Query<UserData>()
                .RetrieveByNameAsync(name);
            if (student == null)
            {
                return controller.NotFound();
            }

            return await controller.GenerateReport(
                engine,
                format,
                generate != null 
                    ? generate
                    : async (generator, reportFormat) => await generator.GenerateAsync(reportFormat, student));
        }
    }
}

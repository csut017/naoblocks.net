namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Helper method for configuring the Angular UI.
    /// </summary>
    public static class AngularUISetup
    {
        /// <summary>
        /// Adds the middleware to run the Angular UI.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> to add the middleware to.</param>
        public static void UseAngularUI(this WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Angular>>();
            app.Use(async (context, next) =>
            {                
                var url = context.Request.Path.Value ?? String.Empty;
                if (!(url.StartsWith("/api", StringComparison.Ordinal) 
                    || url.Contains(".", StringComparison.Ordinal)
                    || string.Equals(url, "/health", StringComparison.InvariantCultureIgnoreCase)))
                {
                    logger.LogInformation("Redirecting to default");
                    context.Request.Path = "/";
                }
                await next.Invoke();
            });

            var options = new DefaultFilesOptions();
            options.DefaultFileNames.Clear();
            options.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(options);
            app.UseStaticFiles();
        }

        private class Angular { }
    }
}

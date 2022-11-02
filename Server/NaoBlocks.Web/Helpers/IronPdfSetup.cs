namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Helper class to initialise the IronPDF library.
    /// </summary>
    public static class IronPdfSetup
    {
        /// <summary>
        /// Initialise IronPDF.
        /// </summary>
        /// <param name="builder">The <see cref="WebApplicationBuilder"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        public static void Initialise(WebApplicationBuilder builder, ILogger logger)
        {
            var ironPdfLicense = builder.Configuration["ironPdf:license"];
            if (!string.IsNullOrEmpty(ironPdfLicense))
            {
                logger.LogInformation("Initialising IronPDF");
                IronPdf.License.LicenseKey = ironPdfLicense;
                if (IronPdf.License.IsLicensed)
                {
                    logger.LogInformation("IronPDF has been initialised");
                }
                else
                {
                    logger.LogWarning("Unable to initialise IronPDF: {error}", "License key is invalid");
                }
            }
            else
            {
                logger.LogWarning("Unable to initialise IronPDF: {error}", "License key not set");
            }
        }
    }
}
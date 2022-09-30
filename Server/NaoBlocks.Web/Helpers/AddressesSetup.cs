using NaoBlocks.Communications;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Configures the IP addresses to use for the server.
    /// </summary>
    public static class AddressesSetup
    {
        /// <summary>
        /// Configures the endpoints to listen on using the available addresses.
        /// </summary>
        /// <param name="builder">The builder to add the endpoints to.</param>
        public static void UseLocalAddresses(this WebApplicationBuilder builder)
        {
            var settings = builder.Configuration.Get<Configuration.Addresses>();
            if (settings != null)
            {
                builder.WebHost.UseKestrel(opts =>
                {
                    if (settings.UseHttp)
                    {
                        foreach (var address in ClientAddressList.RetrieveAddresses())
                        {
                            opts.Listen(address, settings.HttpPort);
                        }
                    }
                    if (settings.UseHttps)
                    {
                        foreach (var address in ClientAddressList.RetrieveAddresses())
                        {
                            opts.Listen(address, settings.HttpsPort, conf => conf.UseHttps());
                        }
                    }
                });
            }
        }
    }
}
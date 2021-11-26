using NaoBlocks.Web.Helpers;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NaoBlocks.Web
{
    /// <summary>
    /// Main entry point for the web API application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Initialises the application.
        /// </summary>
        /// <param name="args">Any command line arguments to check.</param>
        /// <returns>An <see cref="IHostBuilder"/> instance.</returns>
        public IHostBuilder CreateHostBuilder(string[] args)
        {
            var availableInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .OrderByDescending(c => c.Speed)
                .Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up);
            ClientAddressList.Add("http://localhost:5000", "https://localhost:5001");
            foreach (var availableInterface in availableInterfaces)
            {
                var props = availableInterface.GetIPProperties();
                var ip4Addresses = props.UnicastAddresses
                    .Where(c => c.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(c => c.Address)
                    .ToArray();
                foreach (var ip4Address in ip4Addresses)
                {
                    ClientAddressList.Add($"http://{ip4Address}:5000");
                }
            }

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(ClientAddressList.Get().ToArray());
                    webBuilder.UseStartup<Startup>();
                });
        }

        /// <summary>
        /// Executes the application.
        /// </summary>
        /// <param name="args">Any command line arguments to check.</param>
        public static void Main(string[] args)
        {
            var app = new Program();
            app.CreateHostBuilder(args).Build().Run();
        }
    }
}
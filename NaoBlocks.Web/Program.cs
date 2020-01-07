using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

[assembly: AssemblyInformationalVersion("1.0.0 [alpha 2]")]

namespace NaoBlocks.Web
{
    [SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Main application entry point")]
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
    }
}
﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NaoBlocks.Web.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;

[assembly: AssemblyInformationalVersion("1.0.0 [alpha 3]")]

namespace NaoBlocks.Web
{
    [SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Main application entry point")]
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
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

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
    }
}
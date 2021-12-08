using NaoBlocks.Web.Helpers;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Xunit;

namespace NaoBlocks.Web.Tests
{
    [Collection("ClientAddressList tests")]
    public class ProgramTests
    {
        public ProgramTests()
        {
            ClientAddressList.Clear();
        }

        [Fact]
        public void CreateHostBuilderAddsLocalAddresses()
        {
            var app = new Program();
            app.CreateHostBuilder(new[] { "/useLocal" });
            var addresses = ClientAddressList.Get();
            Assert.Contains("http://localhost:5000", addresses);
            Assert.Contains("https://localhost:5001", addresses);
        }

        [Fact]
        public void CreateHostBuilderSkipsLocalAddresses()
        {
            var app = new Program();
            app.CreateHostBuilder(Array.Empty<string>());
            var addresses = ClientAddressList.Get();
            Assert.DoesNotContain("http://localhost:5000", addresses);
            Assert.DoesNotContain("https://localhost:5001", addresses);
        }

        [Fact]
        public void CreateHostBuilderAddsMachineIP()
        {
            var app = new Program();
            app.CreateHostBuilder(Array.Empty<string>());
            var addresses = ClientAddressList.Get();

            var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
                .OrderByDescending(c => c.Speed)
                .FirstOrDefault(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up);
            Assert.NotNull(networkInterface);

            var props = networkInterface!.GetIPProperties();
            var ip4Address = props.UnicastAddresses
                .Where(c => c.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(c => c.Address)
                .First();

            Assert.Contains($"http://{ip4Address}:5000", addresses);
        }

        [Theory]
        [InlineData("/data=test", "data", "test")]
        [InlineData("/data", "data", "")]
        [InlineData("--data=test", "data", "test")]
        [InlineData("--data", "data", "")]
        [InlineData("data=test", "data=test", "")]
        public void ParseArgsChecksArguments(string input, string key, string value)
        {
            var args = Program.ParseArgs(new[] { input });
            Assert.Equal(
                new[] { $"{key}=>{value}" },
                args.Select(kv => $"{kv.Key}=>{kv.Value}").ToArray());
        }

        [Fact]
        public void ParseArgsHandlesDuplicates()
        {
            var args = Program.ParseArgs(new[] {
                "/test=one",
                "/test=two",
                "/test"
            });
            Assert.Equal(
                new[] { $"test=>one,two" },
                args.Select(kv => $"{kv.Key}=>{kv.Value}").ToArray());
        }
    }
}

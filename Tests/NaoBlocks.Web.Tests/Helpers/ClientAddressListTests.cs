using NaoBlocks.Web.Helpers;
using System.Linq;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    [Collection("ClientAddressList tests")]
    public class ClientAddressListTests
    {
        [Fact]
        public void RetrieveAddressesWorks()
        {
            var addresses = ClientAddressList.RetrieveAddresses().ToArray();
            Assert.NotEmpty(addresses);
        }
    }
}
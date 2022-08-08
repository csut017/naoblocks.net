using NaoBlocks.Web.Helpers;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    [Collection("ClientAddressList tests")]
    public class ClientAddressListTests
    {
        [Fact]
        public void RetrieveAddressesWorks()
        {
            Assert.NotEmpty(ClientAddressList.RetrieveAddresses());
        }
    }
}
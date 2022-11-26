using Xunit;

namespace NaoBlocks.Communications.Tests
{
    public class ClientAddressListTests
    {
        [Fact]
        public void RetrieveAddressesRetrievesNetworkAddress()
        {
            // Act
            var addresses = ClientAddressList.RetrieveAddresses().ToArray();

            // Assert
            Assert.NotEmpty(addresses);
        }
    }
}
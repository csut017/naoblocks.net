using NaoBlocks.Web.Helpers;
using System.Linq;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    [Collection("ClientAddressList tests")]
    public class ClientAddressListTests
    {
        [Fact]
        public void AddAddsToList()
        {
            ClientAddressList.Clear();
            ClientAddressList.Add("Testing");
            Assert.Equal(new[] { "Testing" }, ClientAddressList.Get().ToArray());
        }
    }
}

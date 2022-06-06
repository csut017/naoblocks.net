using NaoBlocks.Web.Resources;
using Xunit;

namespace NaoBlocks.Web.Tests.Resources
{
    public class ManagerTests
    {
        [Fact]
        public void AngularUITemplateRetrievesValue()
        {
            var value = Manager.AngularUITemplate;
            Assert.NotEmpty(value);
        }

        [Fact]
        public void NaoToolboxRetrievesValue()
        {
            var value = Manager.NaoToolbox;
            Assert.NotEmpty(value);
        }

        [Fact]
        public void TangiblesUITemplateRetrievesValue()
        {
            var value = Manager.TangiblesUITemplate;
            Assert.NotEmpty(value);
        }
    }
}
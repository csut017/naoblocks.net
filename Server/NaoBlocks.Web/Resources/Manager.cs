using NaoBlocks.Engine;

namespace NaoBlocks.Web.Resources
{
    /// <summary>
    /// Manager for retrieving resources.
    /// </summary>
    public class Manager
    {
        /// <summary>
        /// Gets the default Angular UI definition.
        /// </summary>
        public static string AngularUITemplate
        {
            get
            {
                return TemplateGenerator.ReadEmbededResource<Manager>("angular-ui.json");
            }
        }

        /// <summary>
        /// Gets the default Nao robot toolbox.
        /// </summary>
        public static string NaoToolbox
        {
            get
            {
                return TemplateGenerator.ReadEmbededResource<Manager>("nao-toolbox.xml");
            }
        }

        /// <summary>
        /// Gets the default tangibles UI definition.
        /// </summary>
        public static string TangiblesUITemplate
        {
            get
            {
                return TemplateGenerator.ReadEmbededResource<Manager>("tangibles-ui.json");
            }
        }
    }
}
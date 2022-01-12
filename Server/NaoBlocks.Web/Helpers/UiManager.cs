using NaoBlocks.Engine;
using Newtonsoft.Json;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Manager for working with <see cref="IUIDefinition"/> data.
    /// </summary>
    public class UiManager
    {
        private readonly Dictionary<string, Type> definitions = new();

        /// <summary>
        /// Registers a new <see cref="IUIDefinition"/> type.
        /// </summary>
        /// <typeparam name="TUi">The <see cref="IUIDefinition"/> type.</typeparam>
        /// <param name="name">The name of the definition.</param>
        public void Register<TUi>(string name)
            where TUi : IUIDefinition
        {
            this.definitions[name] = typeof(TUi);
        }

        /// <summary>
        /// Attempts to parse a JSON string into a <see cref="IUIDefinition"/> instance.
        /// </summary>
        /// <param name="name">The name of the definition.</param>
        /// <param name="json">The JSON data.</param>
        /// <returns>The <see cref="IUIDefinition"/> instance.</returns>
        public IUIDefinition Parse(string name, string json)
        {
            if (!this.definitions.TryGetValue(name, out var type)) throw new ApplicationException($"UI {name} does not exist");

            try
            {
                var definition = JsonConvert.DeserializeObject(json, type) as IUIDefinition;
                return definition!;
            }
            catch (JsonReaderException error)
            {
                throw new ApplicationException("Unable to parse definition", error);
            }
        }
    }
}

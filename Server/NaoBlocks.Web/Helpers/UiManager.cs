using NaoBlocks.Engine;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Manager for working with <see cref="IUIDefinition"/> data.
    /// </summary>
    public class UiManager
    {
        private readonly Dictionary<string, Type> definitions = new();

        /// <summary>
        /// Lists all the registered definitions.
        /// </summary>
        /// <returns>The definitions.</returns>
        public IEnumerable<Dtos.UIDefinition> ListRegistered()
        {
            return this.definitions.Select(def =>
            {
                var nameAttrib = def.Value.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault() as DisplayNameAttribute;
                var descriptionAttrib = def.Value.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
                var definition = new Dtos.UIDefinition
                {
                    Description = descriptionAttrib?.Description ?? string.Empty,
                    Name = nameAttrib?.DisplayName ?? "<Unknown>",
                    Key = def.Key
                };
                return definition;
            }).ToArray();
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
    }
}
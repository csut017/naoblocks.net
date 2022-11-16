using System.Reflection;
using System.Text.RegularExpressions;

namespace NaoBlocks.Engine
{
    /// <summary>
    /// Helper class to generate output from embedded resources.
    /// </summary>
    public class TemplateGenerator
    {
        /// <summary>
        /// Reads an embedded resource.
        /// </summary>
        /// <typeparam name="TNamespace">A type in the same namespace and assembly as the embedded resource.</typeparam>
        /// <param name="name">The name of the resource to retrieve.</param>
        /// <returns>A string containing the contents of the embedded resource.</returns>
        /// <exception cref="ArgumentException">Thrown if the name is empty.</exception>
        /// <exception cref="ApplicationException">Thrown if the resource cannot be found.</exception>
        public static string ReadEmbededResource<TNamespace>(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException(null, nameof(name));

            var namespaceType = typeof(TNamespace);
            var assembly = Assembly.GetAssembly(namespaceType);
            var resourceName = $"{namespaceType.Namespace}.{name}";

            using var resourceStream = assembly!.GetManifestResourceStream(resourceName);
            if (resourceStream == null) throw new ApplicationException($"Unable to find resource {name}");

            using var streamReader = new StreamReader(resourceStream);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// Generate some string content from a template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="content">A set of content generators.</param>
        /// <returns>The updated template.</returns>
        public static string Build(string template, Dictionary<string, Func<string>> content)
        {
            var regex = new Regex(@"\<\[\[[^\]]*\]\]\>", RegexOptions.None);
            var matches = new Dictionary<string, string>();
            var output = regex.Replace(template, match =>
            {
                var key = match.Value[3..^3];       // Need to remove matcher characters (<[[ and ]]>)
                if (matches.TryGetValue(key, out var value)) return value;
                if (!content.TryGetValue(key, out var generator)) return string.Empty;
                value = generator();
                matches.Add(key, value);
                return value;
            });

            return output;
        }

        /// <summary>
        /// Reads an embedded resource.
        /// </summary>
        /// <typeparam name="TNamespace">A type in the same namespace and assembly as the embedded resource.</typeparam>
        /// <param name="name">The name of the resource to retrieve.</param>
        /// <param name="content">A set of content generators.</param>
        /// <returns>The updated template.</returns>
        /// <exception cref="ArgumentException">Thrown if the name is empty.</exception>
        /// <exception cref="ApplicationException">Thrown if the template cannot be found.</exception>
        public static string BuildFromTemplate<TNamespace>(string name, Dictionary<string, Func<string>> content)
        {
            if (!name.Contains('.')) name += ".template";
            var template = ReadEmbededResource<TNamespace>(name);
            var output = Build(template, content);
            return output;
        }
    }
}

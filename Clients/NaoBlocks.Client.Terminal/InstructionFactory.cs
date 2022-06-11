using System.Reflection;

namespace NaoBlocks.Client.Terminal
{
    /// <summary>
    /// Factory class for initialising <see cref="InstructionBase"/> instances.
    /// </summary>
    public class InstructionFactory
    {
        private readonly Dictionary<string, Tuple<InstructionAttribute, Type>> instructions = new();
        private bool isInitialised;

        /// <summary>
        /// Initialises this factory.
        /// </summary>
        /// <typeparam name="TInstruction">An instruction in the assembly to load.</typeparam>
        public void Initialise<TInstruction>()
        {
            this.Initialise(typeof(TInstruction));
        }

        /// <summary>
        /// Initialises this factory.
        /// </summary>
        /// <param name="instructionSource">The instruction source.</param>
        public void Initialise(Type instructionSource)
        {
            if (this.isInitialised) throw new InstructionFactoryException("Factory has already been initialised");
            this.isInitialised = true;

            foreach (var type in instructionSource.Assembly.GetExportedTypes())
            {
                var attib = type.GetCustomAttribute<InstructionAttribute>();
                if (attib == null) continue;

                this.instructions.Add(attib.Name.ToLowerInvariant(), new Tuple<InstructionAttribute, Type>(attib, type));
            }
        }

        /// <summary>
        /// Retrieves all the instructions.
        /// </summary>
        /// <returns>An enumerable of all the instructions.</returns>
        public IEnumerable<InstructionBase> List()
        {
            foreach (var instruction in this.instructions.Keys.OrderBy(key => key))
            {
                InstructionBase? instance = null;
                try
                {
                    instance = this.Retrieve(instruction);
                }
                catch
                {
                    // We have a wierd end case where some of the commands will fail to initialise (mainly
                    // due to unit tests), so we'll just say we'll ignore broken instructions
                }
                if (instance != null) yield return instance;
            }
        }

        /// <summary>
        /// Retrieves an instance of an instruction.
        /// </summary>
        /// <param name="name">The name of the instruction.</param>
        /// <returns>A new <see cref="InstructionBase"/> instance if the name is valid; null otherwise.</returns>
        public InstructionBase? Retrieve(string name)
        {
            if (!this.isInitialised) throw new InstructionFactoryException("Factory has not been initialised");
            if (!this.instructions.TryGetValue(name.ToLowerInvariant(), out var instruction)) return null;

            InstructionBase? instance = null;
            Exception? innerError = null;

            try
            {
                instance = Activator.CreateInstance(instruction.Item2) as InstructionBase;
            }
            catch (Exception error)
            {
                innerError = error;
            }

            if (instance == null)
            {
                throw new InstructionFactoryException($"Unable to initialise an instance of {instruction.Item2.FullName}", innerError);
            }

            instance.Name = instruction.Item1.Name;
            if (!string.IsNullOrWhiteSpace(instruction.Item1.Description)) instance.Description = instruction.Item1.Description;
            return instance;
        }
    }
}
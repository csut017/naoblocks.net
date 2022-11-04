namespace NaoBlocks.Engine
{
    /// <summary>
    /// Marks a commands as non-serialisable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TransientAttribute
        : Attribute
    {
    }
}
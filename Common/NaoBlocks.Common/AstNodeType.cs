namespace NaoBlocks.Common
{
    /// <summary>
    /// Defines the type of an AST node.
    /// </summary>
    /// <remarks>
    /// A NaoLang program is compiled into an Abstract Syntax Tree, which
    /// consists of a connected series of <see cref="AstNode"/> blocks.
    /// </remarks>
    public enum AstNodeType
    {
        Invalid,
        Function,
        Argument,
        Constant,
        Variable,
        Compound,
        Empty
    }
}

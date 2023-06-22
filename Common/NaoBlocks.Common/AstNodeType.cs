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
        /// <summary>
        /// The AST node is invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// The AST node is a function.
        /// </summary>
        Function,

        /// <summary>
        /// The AST node is an argument.
        /// </summary>
        Argument,

        /// <summary>
        /// The AST node is a constant.
        /// </summary>
        Constant,

        /// <summary>
        /// The AST node is a variable.
        /// </summary>
        Variable,

        /// <summary>
        /// The AST node is a compound element.
        /// </summary>
        Compound,

        /// <summary>
        /// The AST node is empty.
        /// </summary>
        Empty
    }
}
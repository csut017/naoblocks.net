using System.Collections.Generic;
using System.Linq;


namespace NaoBlocks.Web.Dtos
{
    public class AstNode
    {
        public static AstNode Empty
        {
            get { return new AstNode (); }
        }

        public IList<AstNode>? Arguments { get; private set; }

        public IList<AstNode>? Children { get; private set; }

        public string SourceId { get; set; } = string.Empty;

        public Token Token { get; set; } = Token.Empty;

        public string Type { get; set; } = string.Empty;

        public static AstNode? FromModel(Common.AstNode? value)
        {
            if (value == null) return null;
            var model = new AstNode
            {
                SourceId = value.SourceId,
                Token = Token.FromModel(value.Token) ?? Token.Empty,
                Type = value.Type.ToString()
            };
            if (value.Arguments != null)
            {
                model.Arguments = value.Arguments.Select(n => FromModel(n) ?? Empty).Where(n => !string.IsNullOrEmpty(n.Type)).ToList();
                if (!model.Arguments.Any()) model.Arguments = null;
            }

            if (value.Children != null)
            {
                model.Children = value.Children.Select(n => FromModel(n) ?? Empty).Where(n => !string.IsNullOrEmpty(n.Type)).ToList();
                if (!model.Children.Any()) model.Children = null;
            }

            return model;
        }
    }
}
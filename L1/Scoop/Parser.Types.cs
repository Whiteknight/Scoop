using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {

        private AstNode ParseType(Tokenizer t)
        {
            return ParseTypeArray(t);
        }

        private AstNode ParseTypeArray(Tokenizer t)
        {
            var type = ParseTypeSubtype(t);
            while (true)
            {
                var lookahead = t.Peek();
                if (lookahead.IsOperator("["))
                {
                    t.Advance();
                    // TODO: Size
                    t.Expect(TokenType.Operator, "]");
                    type = new ArrayTypeNode
                    {
                        ElementType = type,
                        Location = lookahead.Location
                    };
                    continue;
                }

                break;
            }

            return type;
        }

        private AstNode ParseTypeSubtype(Tokenizer t)
        {
            // <generic>
            // <generic> "." <generic>
            AstNode type = ParseTypeGeneric(t);
            while (t.NextIs(TokenType.Operator, ".", true))
            {
                var child = ParseTypeGeneric(t);
                type = new ChildTypeNode
                {
                    Location = type.Location,
                    Parent = type,
                    Child = child
                };
            }

            return type;
        }

        private TypeNode ParseTypeGeneric(Tokenizer t)
        {
            // <identifier>
            // <identifier> "<" <typeName> ">"
            var typeNode = ParseTypeName(t);
            if (typeNode == null)
                return null;

            var lookahead = t.Peek();
            if (lookahead.IsOperator("<"))
            {
                t.Advance();
                typeNode.GenericArguments = new List<AstNode>();
                while (true)
                {
                    var elementType = ParseTypeArray(t);
                    if (elementType == null)
                        return null;
                    typeNode.GenericArguments.Add(elementType);
                    if (!t.NextIs(TokenType.Operator, ",", true))
                        break;
                }

                t.Expect(TokenType.Operator, ">");
            }

            return typeNode;
        }

        private TypeNode ParseTypeName(Tokenizer t)
        {
            var id = t.Peek();
            if (!id.IsType(TokenType.Identifier))
                return null;
            t.Advance();

            return new TypeNode
            {
                Location = id.Location,
                Name = new IdentifierNode(id)
            };
        }
    }
}

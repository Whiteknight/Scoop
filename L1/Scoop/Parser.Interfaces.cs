using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        // Helper method to start parsing at the interface level, mostly to simplify unit tests
        public InterfaceNode ParseInterface(string s) => ParseInterface(new Tokenizer(s), null);

        private InterfaceNode ParseInterface(Tokenizer t, List<AttributeNode> attributes)
        {
            return new InterfaceNode
            {
                Attributes = attributes ?? ParseAttributes(t),
                AccessModifier = new KeywordNode(t.Expect(TokenType.Keyword, "public", "private")),
                Location = t.Expect(TokenType.Keyword, "interface").Location,
                Name = new IdentifierNode(t.Expect(TokenType.Identifier)),
                GenericTypeParameters = ParseGenericTypeParametersList(t),
                Interfaces = t.NextIs(TokenType.Operator, ":", true) ? ParseInheritanceList(t) : null,
                TypeConstraints = ParseTypeConstraints(t),
                Members = ParseInterfaceBody(t)
            };
        }

        private List<AstNode> ParseInheritanceList(Tokenizer t)
        {
            var interfaces = new List<AstNode>();
            var contractType = ParseType(t);
            interfaces.Add(contractType);
            while (t.NextIs(TokenType.Operator, ",", true))
            {
                contractType = ParseType(t);
                interfaces.Add(contractType);
            }

            return interfaces;
        }

        private List<AstNode> ParseInterfaceBody(Tokenizer t)
        {
            // <methodSignature>*
            t.Expect(TokenType.Operator, "{");
            var members = new List<AstNode>();
            while (true)
            {
                if (t.Peek().IsOperator("}"))
                    break;
                var returnType = ParseType(t);
                var nameToken = t.GetNext();
                var genericTypeParams = ParseGenericTypeParametersList(t);
                var parameters = ParseParameterList(t);
                members.Add(new MethodDeclareNode
                {
                    Location = returnType.Location,
                    Name = new IdentifierNode(nameToken),
                    GenericTypeParameters = genericTypeParams,
                    Parameters = parameters,
                    TypeConstraints = ParseTypeConstraints(t),
                    ReturnType = returnType
                });
                t.Expect(TokenType.Operator, ";");
            }
            t.Expect(TokenType.Operator, "}");
            return members;
        }
    }
}

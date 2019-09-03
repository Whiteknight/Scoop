using System.Collections.Generic;
using Scoop.SyntaxTree;
using Scoop.Tokenization;

namespace Scoop
{
    public partial class Parser
    {
        // Helper method to start parsing at the interface level, mostly to simplify unit tests
        public InterfaceNode ParseInterface(string s) => ParseInterface(new Tokenizer(new StringCharacterSequence(s)));

        private InterfaceNode ParseInterface(Tokenizer t)
        {
            var accessModifierToken = t.Expect(TokenType.Keyword, "public", "private");
            t.Expect(TokenType.Keyword, "interface");
            var classNameToken = t.Expect(TokenType.Identifier);
            var genericTypeParams = ParseGenericTypeParametersList(t);
            // TODO: ':' <ContractList>
            t.Expect(TokenType.Operator, "{");
            var memberNodes = ParseInterfaceBody(t);
            t.Expect(TokenType.Operator, "}");
            return new InterfaceNode
            {
                AccessModifier = new KeywordNode(accessModifierToken),
                Name = new IdentifierNode(classNameToken),
                GenericTypeParameters = genericTypeParams,
                Members = memberNodes
            };
        }

        private List<AstNode> ParseInterfaceBody(Tokenizer t)
        {
            // <methodSignature>*
            var members = new List<AstNode>();
            while (true)
            {
                var lookahead = t.Peek();
                if (lookahead.IsOperator("}"))
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
                    ReturnType = returnType
                });
                t.Expect(TokenType.Operator, ";");
            }

            return members;
        }
    }
}

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
            var interfaceNode = new InterfaceNode
            {
                AccessModifier = new KeywordNode(accessModifierToken),
                Name = new IdentifierNode(classNameToken),
                GenericTypeParameters = genericTypeParams,
                
            };
            if (t.NextIs(TokenType.Operator, ":", true))
            {
                interfaceNode.Interfaces = new List<AstNode>();
                var contractType = ParseType(t);
                interfaceNode.Interfaces.Add(contractType);
                while (t.NextIs(TokenType.Operator, ",", true))
                {
                    contractType = ParseType(t);
                    interfaceNode.Interfaces.Add(contractType);
                }
            }
            // TODO: "where" <genericTypeParameter> ":" <typeConstraints>
            t.Expect(TokenType.Operator, "{");
            interfaceNode.Members = ParseInterfaceBody(t);
            t.Expect(TokenType.Operator, "}");
            return interfaceNode;
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

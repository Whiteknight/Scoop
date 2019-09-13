using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;
using static Scoop.Parsers.ScoopParsers;

namespace Scoop
{
    public partial class Parser
    {
        private void InitializeClasses()
        {
            var parameter = Sequence(
                Attributes,
                Optional(new KeywordParser("params")),
                Types,
                _identifiers,
                Optional(
                    Sequence(
                        new OperatorParser("="),
                        Expressions,
                        (op, expr) => expr
                    )
                ),
                (attrs, isparams, type, name, value) => new ParameterNode
                {
                    Location = type.Location,
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    IsParams = isparams is KeywordNode,
                    Type = type,
                    Name = name,
                    DefaultValue = value is EmptyNode ? null : value
                }
            ).Named("parameter");

            ParameterList = Sequence(
                new OperatorParser("("),
                SeparatedList(
                    parameter,
                    new OperatorParser(","),
                    parameters => new ListNode<ParameterNode> { Items = parameters.ToList(), Separator = new OperatorNode(",") }
                ),
                RequireOperator(")"),
                (a, parameters, b) => parameters.WithUnused(a, b)
            ).Named("ParameterList");

            var inheritanceList = Optional(
                Sequence(
                    new OperatorParser(":"),
                    SeparatedList(
                        Types,
                        new OperatorParser(","),
                        types => new ListNode<TypeNode> { Items = types.ToList(), Separator = new OperatorNode(",") }
                    ),
                    (colon, types) => types.WithUnused(colon)
                )
            ).Named("inheritanceList");

            var interfaceMember = Sequence(
                Types,
                _identifiers,
                GenericTypeParameters,
                ParameterList,
                TypeConstraints,
                RequireOperator(";"),
                (ret, name, genParm, parm, cons, s) => new MethodDeclareNode
                {
                    Location = name.Location,
                    ReturnType = ret,
                    Name = name,
                    GenericTypeParameters = genParm.IsNullOrEmpty() ? null : genParm,
                    Parameters = parm,
                    TypeConstraints = cons.IsNullOrEmpty() ? null : cons,
                }.WithUnused(s)
            ).Named("interfaceMember");
            var interfaceBody = First(
                Sequence(
                    new OperatorParser("{"),
                    new OperatorParser("}"),
                    (a, b) => new ListNode<MethodDeclareNode>()
                ),
                Sequence(
                    new OperatorParser("{"),
                    List(
                        interfaceMember,
                        members => new ListNode<MethodDeclareNode> { Items = members.ToList() }
                    ),
                    new OperatorParser("}"),
                    (a, members, b) => members
                )
            ).Named("interfaceBody");
            Interfaces = Sequence(
                Attributes,
                Optional(
                    new KeywordParser("public", "private")
                ),
                new KeywordParser("interface"),
                _identifiers,
                GenericTypeParameters,
                inheritanceList,
                TypeConstraints,
                interfaceBody,
                (attrs, vis, i, name, genParm, inh, cons, body) => new InterfaceNode
                {
                    Location = name.Location,
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = vis as KeywordNode,
                    Name = name,
                    GenericTypeParameters = genParm.IsNullOrEmpty() ? null : genParm,
                    Interfaces = inh as ListNode<TypeNode>,
                    TypeConstraints = cons.IsNullOrEmpty() ? null : cons,
                    Members = body
                }
            ).Named("Interfaces");

            Delegates = Sequence(
                // <attributes> <accessModifier>? "delegate" <type> <identifier> <genericParameters>? <parameters> <typeConstraints> ";"
                Deferred(() => Attributes),
                Optional(
                    new KeywordParser("public", "private")
                ),
                new KeywordParser("delegate"),
                Types,
                new IdentifierParser(),
                GenericTypeParameters,
                ParameterList,
                TypeConstraints,
                new OperatorParser(";"),
                (attrs, vis, d, retType, name, gen, param, cons, s) => new DelegateNode
                {
                    Location = d.Location,
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = vis as KeywordNode,
                    ReturnType = retType,
                    Name = name,
                    GenericTypeParameters = gen.IsNullOrEmpty() ? null : gen,
                    Parameters = param,
                    TypeConstraints = cons.IsNullOrEmpty() ? null : cons
                }
            ).Named("Delegates");

            var enumMember = Sequence(
                Attributes,
                new IdentifierParser(),
                Optional(
                    Sequence(
                        new OperatorParser("="),
                        Expressions,
                        (e, expr) => expr
                    )
                ),
                (attrs, name, value) => new EnumMemberNode
                {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    Location = name.Location,
                    Name = name,
                    Value = value is EmptyNode ? null : value
                }
            );
            Enums = Sequence(
                Attributes,
                Optional(
                    new KeywordParser("public", "private")
                ),
                new KeywordParser("enum"),
                new IdentifierParser(),
                new OperatorParser("{"),
                SeparatedList(
                    enumMember,
                    new OperatorParser(","),
                    members => new ListNode<EnumMemberNode> { Items = members.ToList(), Separator = new OperatorNode(",") }
                ),
                new OperatorParser("}"),
                (attrs, vis, e, name, x, members, y) => new EnumNode
                {
                    Location = e.Location,
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = vis as KeywordNode,
                    Name = name,
                    Members = members
                }
            );

            var constants = Sequence(
                Optional(
                    new KeywordParser("public", "private")
                ),
                new KeywordParser("const"),
                Types,
                    _identifiers,
                new OperatorParser("="),
                Expressions,
                new OperatorParser(";"),
                (vis, c, type, name, e, expr, s) => new ConstNode
                {
                    AccessModifier = vis as KeywordNode,
                    Location = name.Location,
                    Type = type,
                    Name = name,
                    Value = expr
                }
            ).Named("constants");

            var exprMethodBody = Sequence(
                new OperatorParser("=>"),
                First(
                    Sequence(
                        new OperatorParser("{"),
                        new OperatorParser("}"),
                        (a, b) => new ListNode<AstNode>()
                    ),
                    Sequence(
                        new OperatorParser("{"),
                        List(
                            Statements,
                            stmts => new ListNode<AstNode> { Items = stmts.ToList() }
                        ),
                        new OperatorParser("}"),
                        (a, stmts, b) => stmts
                    ),
                    Sequence(
                        Expressions,
                        new OperatorParser(";"),
                        (expr, s) => new ListNode<AstNode> { new ReturnNode { Expression = expr } }
                    )
                ),
                (lambda, body) => body
            ).Named("exprMethodBody");
            NormalMethodBody = First(
                Sequence(
                    new OperatorParser("{"),
                    new OperatorParser("}"),
                    (a, b) => new ListNode<AstNode>()
                ),
                Sequence(
                    new OperatorParser("{"),
                    List(
                        Statements,
                        stmts => new ListNode<AstNode> { Items = stmts.ToList() }
                    ),
                    new OperatorParser("}"),
                    (a, body, b) => body
                )
            ).Named("NormalMethodBody");
            var methodBody = First(
                exprMethodBody,
                NormalMethodBody
            ).Named("methodBody");

            var thisArgs = Optional(
                Sequence(
                    new OperatorParser(":"),
                    new IdentifierParser("this"),
                    ArgumentLists,
                    (a, b, args) => args
                )
            ).Named("thisArgs");
            var constructors = Sequence(
                Attributes,
                Optional(
                    new KeywordParser("public", "private")
                ),
                _identifiers,
                ParameterList,
                thisArgs,
                methodBody,
                (attrs, vis, name, param, targs, body) => new ConstructorNode
                {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    Location = name.Location,
                    AccessModifier = vis as KeywordNode,
                    ClassName = name,
                    Parameters = param,
                    ThisArgs = targs as ListNode<AstNode>,
                    Statements = body
                }
            ).Named("constructors");

            var methods = Sequence(
                // <accessModifier>? "async"? <type> <ident> <genericTypeParameters>? <parameterList> <typeConstraints>? <methodBody>
                Attributes,
                Optional(
                    new KeywordParser("public", "private")
                ),
                Optional(
                    new KeywordParser("async")
                ),
                Types,
                _identifiers,
                GenericTypeParameters,
                ParameterList,
                TypeConstraints,
                methodBody,
                (attrs, vis, isAsync, retType, name, genParam, param, cons, body) => new MethodNode
                {
                    Location = name.Location,
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = vis as KeywordNode,
                    Modifiers = isAsync is EmptyNode ? null : new ListNode<KeywordNode> { isAsync as KeywordNode },
                    ReturnType = retType,
                    Name = name,
                    GenericTypeParameters = genParam.IsNullOrEmpty() ? null : genParam,
                    Parameters = param,
                    TypeConstraints = cons.IsNullOrEmpty() ? null : cons,
                    Statements = body
                }
            ).Named("methods");
            var fields = Sequence(
                Attributes,
                Types,
                _identifiers,
                new OperatorParser(";"),
                (attrs, type, name, s) => new FieldNode
                {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    Location = name.Location,
                    Type = type,
                    Name = name
                }
            ).Named("fields");

            ClassMembers = First<AstNode>(
                Token(TokenType.CSharpLiteral, cs => new CSharpNode(cs)),
                Deferred(() => Classes),
                Interfaces,
                Enums,
                Delegates,
                constants,
                constructors,
                methods,
                fields
            ).Named("ClassMembers");

            var classBody = First(
                Sequence(
                    new OperatorParser("{"),
                    new OperatorParser("}"),
                    (a, b) => new ListNode<AstNode>()
                ).Named("classBody.EmptyBrackets"),
                Sequence(
                    new OperatorParser("{"),
                    List(
                        ClassMembers,
                        members => new ListNode<AstNode> { Items = members.ToList() }
                    ),
                    new OperatorParser("}"),
                    (a, members, b) => members
                ).Named("classBody.body")
            ).Named("classBody");

            Classes = Sequence(
                Attributes,
                _accessModifiers,
                Optional(
                    new KeywordParser("partial")
                ).Named("Classes.Optional.Partial"),
                new KeywordParser("class", "struct").Named("Class.class|struct"),
                _identifiers,
                GenericTypeParameters,
                inheritanceList,
                TypeConstraints,
                classBody,
                (attrs, vis, isPartial, obj, name, genParm, contracts, cons, body) => new ClassNode {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    AccessModifier = vis as KeywordNode,
                    Modifiers = isPartial is KeywordNode k ? new ListNode<KeywordNode> { k } : null,
                    Type = obj,
                    Name = name,
                    GenericTypeParameters = genParm.IsNullOrEmpty() ? null : genParm,
                    Interfaces = contracts as ListNode<TypeNode>,
                    TypeConstraints = cons.IsNullOrEmpty() ? null : cons,
                    Members = body
                }
            ).Named("Classes");
        }

        // Helper method to start parsing at the class level, mostly to simplify unit tests
        public ClassNode ParseClass(string s) => Classes.Parse(new Tokenizer(s)).GetResult();

        public AstNode ParseClassMember(string s) => ClassMembers.Parse(new Tokenizer(s)).GetResult();

        public InterfaceNode ParseInterface(string s) => Interfaces.Parse(new Tokenizer(s)).GetResult();
    }
}

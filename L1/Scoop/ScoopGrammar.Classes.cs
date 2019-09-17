using System.Linq;
using Scoop.Parsers;
using Scoop.SyntaxTree;
using Scoop.Tokenization;
using static Scoop.Parsers.ScoopParsers;

namespace Scoop
{
    public partial class ScoopGrammar
    {
        private void InitializeClasses()
        {
            var inheritanceList = Optional(
                // ":" <commaSeparatedType+>
                Sequence(
                    new OperatorParser(":"),
                    Required(
                        SeparatedList(
                            Types,
                            new OperatorParser(","),
                            types => new ListNode<TypeNode> { Items = types.ToList(), Separator = new OperatorNode(",") },
                            atLeastOne: true
                        ),
                        Errors.MissingType
                    ),
                    (colon, types) => types.WithUnused(colon))
            ).Named("inheritanceList");

            var interfaceMember = Sequence(
                Types,
                _requiredIdentifier,
                GenericTypeParameters,
                ParameterList,
                TypeConstraints,
                _requiredSemicolon,
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
                    (a, b) => new ListNode<MethodDeclareNode>().WithUnused(a, b)
                ),
                Sequence(
                    new OperatorParser("{"),
                    List(
                        interfaceMember,
                        members => new ListNode<MethodDeclareNode> { Items = members.ToList() }
                    ),
                    _requiredCloseBracket,
                    (a, members, b) => members.WithUnused(a, b)
                ),
                Error<ListNode<MethodDeclareNode>>(false, Errors.MissingOpenBracket)
            ).Named("interfaceBody");

            Interfaces = Sequence(
                Attributes,
                _accessModifiers,
                new KeywordParser("interface"),
                _requiredIdentifier,
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
                }.WithUnused(i)
            ).Named("Interfaces");

            var constants = Sequence(
                _accessModifiers,
                new KeywordParser("const"),
                _requiredType,
                _requiredIdentifier,
                _requiredEquals,
                _requiredExpression,
                _requiredSemicolon,
                (vis, c, type, name, e, expr, s) => new ConstNode
                {
                    AccessModifier = vis as KeywordNode,
                    Location = name.Location,
                    Type = type,
                    Name = name,
                    Value = expr
                }.WithUnused(c, e, s)
            ).Named("constants");

            var exprMethodBody = Sequence(
                new OperatorParser("=>"),
                First(
                    Sequence(
                        new OperatorParser("{"),
                        new OperatorParser("}"),
                        (a, b) => new ListNode<AstNode>().WithUnused(a, b)
                    ),
                    Sequence(
                        new OperatorParser("{"),
                        List(
                            Statements,
                            stmts => new ListNode<AstNode> { Items = stmts.ToList() }
                        ),
                        _requiredCloseBracket,
                        (a, stmts, b) => stmts.WithUnused(a, b)
                    ),
                    Sequence(
                        Expressions,
                        _requiredSemicolon,
                        (expr, s) => new ListNode<AstNode> { new ReturnNode { Expression = expr } }.WithUnused(s)
                    ),
                    Error< ListNode<AstNode>>(false, Errors.MissingOpenBracket)
                ),
                (lambda, body) => body.WithUnused(lambda)
            ).Named("exprMethodBody");

            NormalMethodBody = First(
                Sequence(
                    new OperatorParser("{"),
                    new OperatorParser("}"),
                    (a, b) => new ListNode<AstNode>().WithUnused(a, b)
                ),
                Sequence(
                    new OperatorParser("{"),
                    List(
                        Statements,
                        stmts => new ListNode<AstNode> { Items = stmts.ToList() }
                    ),
                    _requiredCloseBracket,
                    (a, body, b) => body.WithUnused(a, b)
                )
            ).Named("NormalMethodBody");

            var methodBody = First(
                exprMethodBody,
                NormalMethodBody,
                Error<ListNode<AstNode>>(false, Errors.MissingOpenBracket)
            ).Named("methodBody");

            var constructors = Sequence(
                Attributes,
                _accessModifiers,
                _identifiers,
                ParameterList,
                Optional(
                    Sequence(
                        new OperatorParser(":"),
                        Required(new IdentifierParser("this"), Errors.MissingThis),
                        ArgumentLists,
                        (a, b, args) => args.WithUnused(a, b)
                    )
                ).Named("thisArgs"),
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
                _accessModifiers,
                // TODO: If we see "async" it must be a method and we should require everything else. No backtracking after that
                Optional(new KeywordParser("async")),
                Types,
                _identifiers,
                GenericTypeParameters,
                // TODO: Once we see the parameter list, it must be a method and we should not backtrack
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
                _requiredSemicolon,
                (attrs, type, name, s) => new FieldNode
                {
                    Attributes = attrs.IsNullOrEmpty() ? null : attrs,
                    Location = name.Location,
                    Type = type,
                    Name = name
                }.WithUnused(s)
            ).Named("fields");

            ClassMembers = First<AstNode>(
                Token(TokenType.CSharpLiteral, cs => new CSharpNode(cs)),
                Deferred(() => Classes),
                Interfaces,
                Enums,
                Delegates,
                constants,
                fields,
                methods,
                constructors
            ).Named("ClassMembers");

            var classBody = First(
                Sequence(
                    new OperatorParser("{"),
                    new OperatorParser("}"),
                    (a, b) => new ListNode<AstNode>().WithUnused(a, b)
                ).Named("classBody.EmptyBrackets"),
                Sequence(
                    new OperatorParser("{"),
                    List(
                        ClassMembers,
                        members => new ListNode<AstNode> { Items = members.ToList() }
                    ),
                    _requiredCloseBracket,
                    (a, members, b) => members.WithUnused(a, b)
                ).Named("classBody.body"),
                Error< ListNode<AstNode>>(true, Errors.MissingOpenBracket)
            ).Named("classBody");

            Classes = Sequence(
                Attributes,
                _accessModifiers,
                Optional(new KeywordParser("partial")),
                new KeywordParser("class", "struct"),
                _requiredIdentifier,
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

        private void InitializeParameters()
        {
            var parameter = Sequence(
                // <attributes> "params"? <type> <ident> ("=" <expr>)?
                Attributes,
                Optional(new KeywordParser("params")),
                Types,
                _requiredIdentifier,
                Optional(
                    Sequence(
                        new OperatorParser("="),
                        _requiredExpression,
                        (op, expr) => expr.WithUnused(op)
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

            ParameterList = First(
                //("(" ")") | ("(" <commaSeparatedParameterList> ")")
                Sequence(
                    new OperatorParser("("),
                    new OperatorParser(")"),
                    (a, b) => ListNode<ParameterNode>.Default().WithUnused(a, b)
                ),
                Sequence(
                    _requiredOpenParen,
                    SeparatedList(
                        parameter,
                        new OperatorParser(","),
                        parameters => new ListNode<ParameterNode> { Items = parameters.ToList(), Separator = new OperatorNode(",") }
                    ),
                    _requiredCloseParen,
                    (a, parameters, b) => parameters.WithUnused(a, b)
                ),
                Error<ListNode<ParameterNode>>(false, Errors.MissingParameterList)
            ).Named("ParameterList");
        }

        // Helper method to start parsing at the class level, mostly to simplify unit tests
        public ClassNode ParseClass(string s) => Classes.Parse(new Tokenizer(s)).GetResult();

        public AstNode ParseClassMember(string s) => ClassMembers.Parse(new Tokenizer(s)).GetResult();

        public InterfaceNode ParseInterface(string s) => Interfaces.Parse(new Tokenizer(s)).GetResult();
    }
}

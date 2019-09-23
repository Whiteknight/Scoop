using Scoop.Parsers;
using Scoop.SyntaxTree;

namespace Scoop.Grammar
{
    public interface IScoopGrammar
    {
        IParser<CompilationUnitNode> CompilationUnits { get; }
        IParser<TypeNode> Types { get; }
        IParser<AstNode> Expressions { get; }
        IParser<AstNode> Statements { get; }
        IParser<ListNode<AttributeNode>> Attributes { get; }
        IParser<DelegateNode> Delegates { get; }
        IParser<EnumNode> Enums { get; }
        IParser<ClassNode> Classes { get; }
        IParser<AstNode> ClassMembers { get; }
        IParser<InterfaceNode> Interfaces { get; }
    }
}
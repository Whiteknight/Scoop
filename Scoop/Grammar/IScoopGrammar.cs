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
        IParser<ListNode<AttributeNode>> Attributes { get; set; }
        IParser<DelegateNode> Delegates { get; set; }
        IParser<EnumNode> Enums { get; set; }
        IParser<ClassNode> Classes { get; set; }
        IParser<AstNode> ClassMembers { get; set; }
        IParser<InterfaceNode> Interfaces { get; set; }
    }
}
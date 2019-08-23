using System;
using System.Collections.Generic;
using System.IO;

namespace Scoop.SyntaxTree
{
    public class CompilationUnitNode : AstNode
    {
        // A compilation unit is a single file, which typically will contain some
        // using statements, maybe a namespace declaration, and maybe some classes

        public CompilationUnitNode(string relativeFilePath)
        {
            FileName = Path.GetFileName(relativeFilePath);
            RelativeFilePath = Path.GetFullPath(relativeFilePath);
            var defaultNamespace = new DottedIdentifierNode(RelativeFilePath.Split(Path.PathSeparator));
            AddNamespace(new NamespaceNode { Name = defaultNamespace });
        }

        public CompilationUnitNode()
        {
        }

        public string FileName { get; set;  }

        // Used to get a default namespace
        public string RelativeFilePath { get; set; }

        
        public List<UsingDirectiveNode> UsingDirectives { get; set; }
        public List<NamespaceNode> Namespaces { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitCompilationUnit(this);

        public void AddNamespace(NamespaceNode ns)
        {
            if (Namespaces == null)
                Namespaces = new List<NamespaceNode>();
            Namespaces.Add(ns);
        }


        public void AddUsingDirective(UsingDirectiveNode usingDirective)
        {
            if (UsingDirectives == null)
                UsingDirectives = new List<UsingDirectiveNode>();
            UsingDirectives.Add(usingDirective);
        }
    }
}
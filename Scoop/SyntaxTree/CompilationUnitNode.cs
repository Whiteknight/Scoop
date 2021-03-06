﻿using System.IO;
using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class CompilationUnitNode : AstNode
    {
        // A compilation unit is a single file, which typically will contain some
        // using statements, maybe a namespace declaration, and maybe some classes

        public CompilationUnitNode()
        {
        }

        public CompilationUnitNode(string relativeFilePath)
        {
            FileName = Path.GetFileName(relativeFilePath);
            RelativeFilePath = Path.GetFullPath(relativeFilePath);
        }

        public string FileName { get; set;  }

        // Used to get a default namespace
        public string RelativeFilePath { get; set; }

        public ListNode<AstNode> Members { get; set; }

        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitCompilationUnit(this);
    }
}
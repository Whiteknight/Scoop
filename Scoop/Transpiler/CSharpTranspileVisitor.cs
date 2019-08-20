﻿using System.Text;
using Scoop.SyntaxTree;

namespace Scoop.Transpiler
{
    public class CSharpTranspileVisitor : AstNodeVisitor
    {
        private readonly StringBuilder _sb;
        private int _indent;

        public CSharpTranspileVisitor(StringBuilder sb)
        {
            _sb = sb;
            _indent = 0;
        }

        public static string ToString(AstNode n)
        {
            var sb = new StringBuilder();
            new CSharpTranspileVisitor(sb).Visit(n);
            return sb.ToString();
        }

        public void AppendLineAndIndent(string s = "")
        {
            AppendLine(s);
            WriteIndent();
        }

        public void AppendLine(string s = "") => _sb.AppendLine(s);
        public void Append(string s) => _sb.Append(s);
        public void WriteIndent()
        {
            if (_indent <= 0)
                return;
            _sb.Append(new string(' ', _indent * 4));
        }
        public void IncreaseIndent() => _indent++;
        public void DecreaseIndent() => _indent--;

        public override AstNode VisitCompilationUnit(CompilationUnitNode n)
        {
            foreach (var ud in n.UsingDirectives.OrEmptyIfNull())
            {
                Visit(ud);
                AppendLine();
            }

            foreach (var ns in n.Namespaces.OrEmptyIfNull())
            {
                Visit(ns);
                AppendLine();
            }

            return n;
        }

        public override AstNode VisitClass(ClassNode n)
        {
            Visit(n.AccessModifier);
            Append(" class ");
            Visit(n.Name);
            Append(" ");
            if (!n.Interfaces.IsNullOrEmpty())
            {
                Append(": ");
                Visit(n.Interfaces[0]);
                for (int i = 1; i < n.Interfaces.Count; i++)
                {
                    Append(", ");
                    Visit(n.Interfaces[i]);
                }
            }

            AppendLineAndIndent();
            Append("{");
            IncreaseIndent();

            if (!n.Members.IsNullOrEmpty())
            {
                AppendLineAndIndent();
                Visit(n.Members[0]);
                for (int i = 1; i < n.Members.Count; i++)
                {
                    AppendLineAndIndent();
                    Visit(n.Members[i]);
                }
            }

            DecreaseIndent();
            AppendLineAndIndent();
            Append("}");
            return n;
        }

        public override AstNode VisitConstructor(ConstructorNode n)
        {
            Visit(n.AccessModifier);
            Append(" ");
            Visit(n.ClassName);
            Append("(");
            if (!n.Parameters.IsNullOrEmpty())
            {
                Visit(n.Parameters[0]);
                for (int i = 1; i < n.Parameters.Count; i++)
                {
                    Append(", ");
                    Visit(n.Parameters[i]);
                }
            }
            AppendLineAndIndent(")");
            Append("{");
            IncreaseIndent();

            foreach (var s in n.Statements.OrEmptyIfNull())
                Visit(s);

            DecreaseIndent();
            AppendLineAndIndent();
            Append("}");
            return n;
        }

        public override AstNode VisitDottedIdentifier(DottedIdentifierNode n)
        {
            Append(n.Id);
            return n;
        }

        public override AstNode VisitIdentifier(IdentifierNode n)
        {
            Append(n.Id);
            return n;
        }

        public override AstNode VisitInterface(InterfaceNode n)
        {
            Visit(n.AccessModifier);
            Append(" interface ");
            Visit(n.Name);
            Append(" ");
            if (!n.Interfaces.IsNullOrEmpty())
            {
                Append(": ");
                Visit(n.Interfaces[0]);
                for (int i = 1; i < n.Interfaces.Count; i++)
                {
                    Append(", ");
                    Visit(n.Interfaces[i]);
                }
            }

            AppendLine("{");
            IncreaseIndent();
            AppendLineAndIndent();

            foreach (var m in n.Members.OrEmptyIfNull())
            {
                Visit(m);
                AppendLineAndIndent();
            }

            DecreaseIndent();
            AppendLineAndIndent();
            AppendLine("}");
            return n;
        }

        public override AstNode VisitKeyword(KeywordNode n)
        {
            Append(n.Keyword);
            return n;
        }

        public override AstNode VisitMethod(MethodNode n)
        {
            Visit(n.AccessModifier);
            Append(" ");
            Visit(n.ReturnType);
            Append(" ");
            Visit(n.Name);
            Append("(");
            if (!n.Parameters.IsNullOrEmpty())
            {
                Visit(n.Parameters[0]);
                for (int i = 1; i < n.Parameters.Count; i++)
                {
                    Append(", ");
                    Visit(n.Parameters[i]);
                }
            }

            AppendLineAndIndent(")");
            Append("{");
            IncreaseIndent();

            foreach (var s in n.Statements.OrEmptyIfNull())
                Visit(s);

            DecreaseIndent();
            AppendLineAndIndent();
            Append("}");
            return n;
        }

        public override AstNode VisitNamespace(NamespaceNode n)
        {
            Append("namespace ");
            Visit(n.Name);
            AppendLine();
            Append("{");
            IncreaseIndent();
            AppendLineAndIndent();
            if (!n.Declarations.IsNullOrEmpty())
            {
                Visit(n.Declarations[0]);
                for (int i = 1; i < n.Declarations.Count; i++)
                {
                    AppendLineAndIndent();
                    Visit(n.Declarations[i]);
                }
            }

            DecreaseIndent();
            AppendLineAndIndent();
            Append("}");
            return n;
        }

        public override AstNode VisitUsingDirective(UsingDirectiveNode n)
        {
            Append("using ");
            if (n.Alias != null)
            {
                Visit(n.Alias);
                Append(" = ");
            }

            Visit(n.Namespace);
            Append(";");
            return n;
        }
    }
}

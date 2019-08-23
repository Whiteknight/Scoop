﻿using System.Globalization;
using Scoop.SyntaxTree;
using System.IO;
using System.Linq;
using System.Text;

namespace Scoop.Transpiler
{
    public class CSharpTranspileVisitor : AstNodeVisitor
    {
        private readonly TextWriter _tw;
        private int _indent;

        public CSharpTranspileVisitor(StringBuilder sb)
        {
            _tw = new StringWriter(sb);
            _indent = 0;
        }

        public CSharpTranspileVisitor(TextWriter writer)
        {
            _tw = writer;
            _indent = 0;
        }

        public static string ToString(AstNode n)
        {
            var sb = new StringBuilder();
            new CSharpTranspileVisitor(sb).Visit(n);
            return sb.ToString();
        }

        public void Append(string s) => _tw.Write(s);

        public void AppendLine(string s = "") => _tw.WriteLine(s);

        public void AppendLineAndIndent(string s = "")
        {
            AppendLine(s);
            AppendIndent();
        }

        public void DecreaseIndent() => _indent--;
        public void IncreaseIndent() => _indent++;
        public void AppendIndent()
        {
            if (_indent <= 0)
                return;
            _tw.Write(new string(' ', _indent * 4));
        }

        public override AstNode VisitArrayType(ArrayTypeNode n)
        {
            Visit(n.ElementType);
            Append("[]");
            return n;
        }

        public override AstNode VisitCast(CastNode n)
        {
            Append("(");
            Visit(n.Type);
            Append(")");
            Visit(n.Right);
            return n;
        }

        public override AstNode VisitChar(CharNode n)
        {
            Append("'");
            var asInt = (int) n.Value;
            if (asInt > 127 || char.IsControl(n.Value) || n.Value == '\n' || n.Value == '\r' || n.Value == '\v')
                Append($"\\x{asInt:X}");
            else
                Append(n.Value.ToString());
            Append("'");
            return n;
        }

        public override AstNode VisitChildType(ChildTypeNode n)
        {
            Visit(n.Parent);
            Append(".");
            Visit(n.Child);
            return n;
        }

        public override AstNode VisitClass(ClassNode n)
        {
            Visit(n.AccessModifier);
            Append(" sealed class ");
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

        public override AstNode VisitCompilationUnit(CompilationUnitNode n)
        {
            if (!string.IsNullOrEmpty(n.FileName))
                AppendLineAndIndent($"// Source File: {n.FileName}");
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
            // TODO: ": this(...)"
            Append("{");
            IncreaseIndent();
            AppendLineAndIndent();
            AppendLineAndIndent("// Do not allow concrete inheritance");
            AppendLineAndIndent("System.Diagnostics.Debug.Assert(GetType().BaseType == typeof(object));");

            foreach (var s in n.Statements.OrEmptyIfNull())
            {
                Visit(s);
                AppendLineAndIndent($"#line {s.Location.Line} \"{s.Location.FileName}\"");
                AppendLineAndIndent();
            }

            DecreaseIndent();
            AppendLineAndIndent();
            Append("}");
            return n;
        }

        public override AstNode VisitDecimal(DecimalNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            return n;
        }

        public override AstNode VisitDottedIdentifier(DottedIdentifierNode n)
        {
            Append(n.Id);
            return n;
        }

        public override AstNode VisitDouble(DoubleNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            return n;
        }

        public override AstNode VisitFloat(FloatNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            return n;
        }

        public override AstNode VisitIdentifier(IdentifierNode n)
        {
            Append(n.Id);
            return n;
        }

        public override AstNode VisitInfixOperation(InfixOperationNode n)
        {
            Visit(n.Left);
            Append(" ");
            Visit(n.Operator);
            Append(" ");
            Visit(n.Right);
            return n;
        }

        public override AstNode VisitInteger(IntegerNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
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

        public override AstNode VisitInvoke(InvokeNode n)
        {
            Visit(n.Instance);
            Append("(");
            if (n.Arguments != null && n.Arguments.Any())
            {
                Visit(n.Arguments[0]);
                for (int i = 1; i < n.Arguments.Count; i++)
                {
                    Append(", ");
                    Visit(n.Arguments[i]);
                }
            }

            Append(")");
            return n;
        }

        public override AstNode VisitKeyword(KeywordNode n)
        {
            Append(n.Keyword);
            return n;
        }

        public override AstNode VisitLong(LongNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            return n;
        }

        public override AstNode VisitMemberAccess(MemberAccessNode n)
        {
            Visit(n.Instance);
            Append(".");
            Visit(n.MemberName);
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
            {
                AppendLineAndIndent();
                Visit(s);
                Append(";");
            }

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

        public override AstNode VisitNew(NewNode n)
        {
            Append("new ");
            Visit(n.Type);
            Append("(");
            if (n.Arguments != null && n.Arguments.Any())
            {
                Visit(n.Arguments[0]);
                for (int i = 1; i < n.Arguments.Count; i++)
                {
                    Append(", ");
                    Visit(n.Arguments[i]);
                }
            }
            Append(")");
            return n;
        }


        public override AstNode VisitOperator(OperatorNode n)
        {
            Append(n.Operator);
            return n;
        }

        public override AstNode VisitParenthesis<TNode>(ParenthesisNode<TNode> n)
        {
            Append("(");
            Visit(n.Expression);
            Append(")");
            return n;
        }

        public override AstNode VisitPostfixOperation(PostfixOperationNode n)
        {
            Visit(n.Left);
            Visit(n.Operator);
            return n;
        }

        public override AstNode VisitPrefixOperation(PrefixOperationNode n)
        {
            Visit(n.Operator);
            Visit(n.Right);
            return n;
        }

        public override AstNode VisitReturn(ReturnNode n)
        {
            Append("return ");
            Visit(n.Expression);
            return n;
        }

        public override AstNode VisitString(StringNode n)
        {
            if (n.Interpolated)
                Append("$");
            if (n.Literal)
                Append("@");
            Append("\"");
            Append(n.Value);
            Append("\"");
            return n;
        }

        public override AstNode VisitType(TypeNode n)
        {
            Visit(n.Name);
            if (n.GenericArguments != null && n.GenericArguments.Any())
            {
                Append("<");
                Visit(n.GenericArguments[0]);
                for (int i = 1; i < n.GenericArguments.Count; i++)
                {
                    Append(", ");
                    Visit(n.GenericArguments[i]);
                }
                Append(">");
            }

            return n;
        }

        public override AstNode VisitUInteger(UIntegerNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            Append("U");
            return n;
        }

        public override AstNode VisitULong(ULongNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            Append("UL");
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

        public override AstNode VisitVariableDeclare(VariableDeclareNode n)
        {
            Append("var ");
            Visit(n.Name);
            return n;
        }   
    }
}
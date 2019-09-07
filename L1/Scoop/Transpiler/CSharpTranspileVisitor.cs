using System.Globalization;
using Scoop.SyntaxTree;
using System.Linq;

namespace Scoop.Transpiler
{
    public partial class CSharpTranspileVisitor : IAstNodeVisitorImplementation
    {
        public AstNode VisitArrayInitializer(ArrayInitializerNode n)
        {
            Append("[");
            Visit(n.Key);
            Append("] = ");
            Visit(n.Value);
            return n;
        }

        public AstNode VisitArrayType(ArrayTypeNode n)
        {
            Visit(n.ElementType);
            Append("[]");
            return n;
        }

        public AstNode VisitAttribute(AttributeNode n)
        {
            Append("[");
            if (n.Target != null)
            {
                Visit(n.Target);
                Append(": ");
            }

            Visit(n.Type);
            if (!n.Arguments.IsNullOrEmpty())
            {
                Append("(");
                Visit(n.Arguments.First());
                foreach (var a in n.Arguments.Skip(1))
                {
                    Append(", ");
                    Visit(a);
                }
                Append(")");
            }
            Append("]");
            return n;
        }

        public AstNode VisitCast(CastNode n)
        {
            Append("(");
            Visit(n.Type);
            Append(")");
            Visit(n.Right);
            return n;
        }

        public AstNode VisitConst(ConstNode n)
        {
            if (n.AccessModifier != null)
            {
                Visit(n.AccessModifier);
                Append(" ");
            }
            Append("const ");
            Visit(n.Type);
            Append(" ");
            Visit(n.Name);
            Append(" = ");
            Visit(n.Value);
            return n;
        }

        public AstNode VisitChar(CharNode n)
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

        public AstNode VisitChildType(ChildTypeNode n)
        {
            Visit(n.Parent);
            Append(".");
            Visit(n.Child);
            return n;
        }

        public AstNode VisitClass(ClassNode n)
        {
            if (!n.Attributes.IsNullOrEmpty())
            {
                foreach (var a in n.Attributes)
                    Visit(a);
            }

            // Sometimes C# treats explicit "private" as an error, so never print it here.
            if (n.AccessModifier?.Keyword != "private")
            {
                Visit(n.AccessModifier);
                Append(" ");
            }
            Append("sealed");
            if (n.Modifiers != null)
            {
                for (int i = 0; i < n.Modifiers.Count; i++)
                {
                    Append(" ");
                    Visit(n.Modifiers[i]);
                }
            }

            Append(" ");
            Visit(n.Type);
            Append(" ");
            Visit(n.Name);
            if (!n.GenericTypeParameters.IsNullOrEmpty())
            {
                Append("<");
                Visit(n.GenericTypeParameters[0]);
                for (int i = 1; i < n.GenericTypeParameters.Count; i++)
                {
                    Append(", ");
                    Visit(n.GenericTypeParameters[i]);
                }

                Append(">");
            }
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

            if (!n.GenericTypeParameters.IsNullOrEmpty() && !n.TypeConstraints.IsNullOrEmpty())
            {
                IncreaseIndent();
                AppendLineAndIndent();
                Visit(n.TypeConstraints.First());
                foreach (var tc in n.TypeConstraints.Skip(1))
                {
                    AppendLineAndIndent();
                    Visit(tc);
                }

                DecreaseIndent();
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

        public AstNode VisitCompilationUnit(CompilationUnitNode n)
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

        public AstNode VisitConditional(ConditionalNode n)
        {
            Visit(n.Condition);
            Append(" ? ");
            Visit(n.IfTrue);
            Append(" : ");
            Visit(n.IfFalse);
            return n;
        }

        public AstNode VisitConstructor(ConstructorNode n)
        {
            if (!n.Attributes.IsNullOrEmpty())
            {
                foreach (var a in n.Attributes)
                    Visit(a);
            }
            Visit(n.AccessModifier ?? new KeywordNode("private"));
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

            Append(")");
            if (!n.ThisArgs.IsNullOrEmpty())
            {
                IncreaseIndent();
                AppendLineAndIndent();
                Append(": this(");
                Visit(n.ThisArgs.First());
                foreach (var a in n.ThisArgs.Skip(1))
                {
                    Append(", ");
                    Visit(a);
                }
                Append(")");
                DecreaseIndent();
            }

            AppendLineAndIndent();
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

        public AstNode VisitCSharp(CSharpNode n)
        {
            AppendLineAndIndent(n.Code);
            return n;
        }

        public AstNode VisitDecimal(DecimalNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            return n;
        }

        public AstNode VisitDottedIdentifier(DottedIdentifierNode n)
        {
            Append(n.Id);
            return n;
        }

        public AstNode VisitDouble(DoubleNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            return n;
        }

        public AstNode VisitEnum(EnumNode n)
        {
            if (!n.Attributes.IsNullOrEmpty())
            {
                foreach (var a in n.Attributes)
                    Visit(a);
            }
            if (n.AccessModifier != null)
            {
                Visit(n.AccessModifier);
                Append(" ");
            }

            Append("enum ");
            Visit(n.Name);
            Append("{");
            IncreaseIndent();
            if (n.Members.Any())
            {
                Visit(n.Members.First());
                foreach (var m in n.Members.Skip(1))
                {
                    AppendLineAndIndent(",");
                    Visit(m);
                }
            }
            DecreaseIndent();
            AppendLineAndIndent();
            Append("}");
            return n;
        }

        public AstNode VisitEnumMember(EnumMemberNode n)
        {
            if (!n.Attributes.IsNullOrEmpty())
            {
                foreach (var a in n.Attributes)
                    Visit(a);
            }
            Visit(n.Name);
            if (n.Value != null)
            {
                Append(" = ");
                Visit(n.Value);
            }

            return n;
        }

        public AstNode VisitField(FieldNode n)
        {
            Append("private readonly ");
            Visit(n.Type);
            Append(" ");
            Visit(n.Name);
            return n;
        }

        public AstNode VisitFloat(FloatNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            return n;
        }

        public AstNode VisitIdentifier(IdentifierNode n)
        {
            Append(n.Id);
            return n;
        }

        public AstNode VisitIndex(IndexNode n)
        {
            Visit(n.Instance);
            Append("[");
            Visit(n.Arguments[0]);
            for (int i = 1; i < n.Arguments.Count; i++)
            {
                Append(", ");
                Visit(n.Arguments[i]);
            }
            Append("]");
            return n;
        }

        public AstNode VisitInfixOperation(InfixOperationNode n)
        {
            Visit(n.Left);
            Append(" ");
            Visit(n.Operator);
            Append(" ");
            Visit(n.Right);
            return n;
        }

        public AstNode VisitInteger(IntegerNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            return n;
        }

        public AstNode VisitInterface(InterfaceNode n)
        {
            if (!n.Attributes.IsNullOrEmpty())
            {
                foreach (var a in n.Attributes)
                    Visit(a);
            }
            Visit(n.AccessModifier ?? new KeywordNode("private"));
            Append(" interface ");
            Visit(n.Name);
            if (!n.GenericTypeParameters.IsNullOrEmpty())
            {
                Append("<");
                Visit(n.GenericTypeParameters[0]);
                for (int i = 1; i < n.GenericTypeParameters.Count; i++)
                {
                    Append(", ");
                    Visit(n.GenericTypeParameters[i]);
                }

                Append(">");
            }
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
            if (!n.GenericTypeParameters.IsNullOrEmpty() && !n.TypeConstraints.IsNullOrEmpty())
            {
                IncreaseIndent();
                AppendLineAndIndent();
                Visit(n.TypeConstraints.First());
                foreach (var tc in n.TypeConstraints.Skip(1))
                {
                    AppendLineAndIndent();
                    Visit(tc);
                }

                DecreaseIndent();
            }

            AppendLineAndIndent();
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

        public AstNode VisitInvoke(InvokeNode n)
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

        public AstNode VisitKeyValueInitializer(KeyValueInitializerNode n)
        {
            Append("{");
            Visit(n.Key);
            Append(", ");
            Visit(n.Value);
            Append("}");
            return n;
        }

        public AstNode VisitKeyword(KeywordNode n)
        {
            Append(n.Keyword);
            return n;
        }

        public AstNode VisitLambda(LambdaNode n)
        {
            Append("((");
            if (n.Parameters.Any())
            {
                Visit(n.Parameters.First());
                foreach (var p in n.Parameters.Skip(1))
                {
                    Append(", ");
                    Visit(p);
                }
            }

            Append(") => ");
            if (n.Statements == null || !n.Statements.Any())
                Append("{}");
            else if (n.Statements.Count == 1)
                Visit(n.Statements[0]);
            else
            {
                Append("{");
                IncreaseIndent();
                
                foreach (var statement in n.Statements)
                {
                    AppendLineAndIndent();
                    Visit(statement);
                    Append(";");
                }

                DecreaseIndent();
                AppendLineAndIndent();
                Append("}");
            }
            Append(")");

            return n;
        }

        public AstNode VisitList(ListNode n)
        {
            if (n.Any())
            {
                Visit(n[0]);
                for (int i = 1; i < n.Count; i++)
                {
                    Append(", ");
                    Visit(n[i]);
                }
            }

            return n;
        }

        public AstNode VisitLong(LongNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            return n;
        }

        public AstNode VisitMemberAccess(MemberAccessNode n)
        {
            Visit(n.Instance);
            Append(n.IgnoreNulls ? "?." : ".");
            Visit(n.MemberName);
            if (!n.GenericArguments.IsNullOrEmpty())
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

        public AstNode VisitMethod(MethodNode n)
        {
            if (!n.Attributes.IsNullOrEmpty())
            {
                foreach (var a in n.Attributes)
                    Visit(a);
            }
            Visit(n.AccessModifier ?? new KeywordNode("private"));
            Append(" ");
            Visit(n.ReturnType);
            Append(" ");
            Visit(n.Name);
            if (!n.GenericTypeParameters.IsNullOrEmpty())
            {
                Append("<");
                Visit(n.GenericTypeParameters[0]);
                for (int i = 1; i < n.GenericTypeParameters.Count; i++)
                {
                    Append(", ");
                    Visit(n.GenericTypeParameters[i]);
                }

                Append(">");
            }

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

            Append(")");
            if (!n.GenericTypeParameters.IsNullOrEmpty() && !n.TypeConstraints.IsNullOrEmpty())
            {
                IncreaseIndent();
                AppendLineAndIndent();
                Visit(n.TypeConstraints.First());
                foreach (var tc in n.TypeConstraints.Skip(1))
                {
                    AppendLineAndIndent();
                    Visit(tc);
                }

                DecreaseIndent();
            }

            AppendLineAndIndent();
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

        public AstNode VisitMethodDeclare(MethodDeclareNode n)
        {
            Visit(n.ReturnType);
            Append(" ");
            Visit(n.Name);
            if (!n.GenericTypeParameters.IsNullOrEmpty())
            {
                Append("<");
                Visit(n.GenericTypeParameters[0]);
                for (int i = 1; i < n.GenericTypeParameters.Count; i++)
                {
                    Append(", ");
                    Visit(n.GenericTypeParameters[i]);
                }

                Append(">");
            }
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

            Append(")");
            if (!n.GenericTypeParameters.IsNullOrEmpty() && !n.TypeConstraints.IsNullOrEmpty())
            {
                IncreaseIndent();
                AppendLineAndIndent();
                Visit(n.TypeConstraints.First());
                foreach (var tc in n.TypeConstraints.Skip(1))
                {
                    AppendLineAndIndent();
                    Visit(tc);
                }

                DecreaseIndent();
            }

            Append(";");
            return n;
        }

        public AstNode VisitNamespace(NamespaceNode n)
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

        public AstNode VisitNew(NewNode n)
        {
            Append("new");
            // Tries to output one of the following forms:
            // new {+}
            // new MyType(*)
            // new MyType(+) {+}
            // new MyType {+}
            if (n.Type != null)
            {
                Append(" ");
                Visit(n.Type);
                // If we have arguments, output parens
                // If we have no arguments AND we have no initializers, output empty parens
                if (!n.Arguments.IsNullOrEmpty() || n.Initializers.IsNullOrEmpty())
                {
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
                }
            }

            if (!n.Initializers.IsNullOrEmpty())
            {
                Append(" {");
                IncreaseIndent();
                AppendLineAndIndent();
                Visit(n.Initializers[0]);
                for (int i = 1; i < n.Initializers.Count; i++)
                {
                    AppendLineAndIndent(",");
                    Visit(n.Initializers[i]);
                }
                DecreaseIndent();
                AppendLineAndIndent();
                Append("}");
            }

            return n;
        }


        public AstNode VisitOperator(OperatorNode n)
        {
            Append(n.Operator);
            return n;
        }

        public AstNode VisitParameter(ParameterNode n)
        {
            if (!n.Attributes.IsNullOrEmpty())
            {
                foreach (var a in n.Attributes)
                    Visit(a);
            }
            Visit(n.Type);
            Append(" ");
            Visit(n.Name);
            if (n.DefaultValue != null)
            {
                Append(" = ");
                Visit(n.DefaultValue);
            }

            return n;
        }

        public AstNode VisitParenthesis<TNode>(ParenthesisNode<TNode> n)
            where TNode : AstNode
        {
            Append("(");
            Visit(n.Expression);
            Append(")");
            return n;
        }

        public AstNode VisitPostfixOperation(PostfixOperationNode n)
        {
            Visit(n.Left);
            Visit(n.Operator);
            return n;
        }

        public AstNode VisitPrefixOperation(PrefixOperationNode n)
        {
            Visit(n.Operator);
            Visit(n.Right);
            return n;
        }

        public AstNode VisitPropertyInitializer(PropertyInitializerNode n)
        {
            Visit(n.Property);
            Append(" = ");
            Visit(n.Value);
            return n;
        }

        public AstNode VisitReturn(ReturnNode n)
        {
            Append("return ");
            Visit(n.Expression);
            return n;
        }

        public AstNode VisitString(StringNode n)
        {
            Append(n.Value);
            return n;
        }

        public AstNode VisitType(TypeNode n)
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

        public AstNode VisitTypeConstraint(TypeConstraintNode n)
        {
            Append("where ");
            Visit(n.Type);
            Append(" : ");
            Visit(n.Constraints.First());
            foreach (var c in n.Constraints.Skip(1))
            {
                Append(", ");
                Visit(c);
            }

            return n;
        }

        public AstNode VisitUInteger(UIntegerNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            Append("U");
            return n;
        }

        public AstNode VisitULong(ULongNode n)
        {
            Append(n.Value.ToString(CultureInfo.InvariantCulture));
            Append("UL");
            return n;
        }

        public AstNode VisitUsingDirective(UsingDirectiveNode n)
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

        public AstNode VisitUsingStatement(UsingStatementNode n)
        {
            Append("using (");
            Visit(n.Disposable);
            Append(")");
            IncreaseIndent();
            AppendLineAndIndent();
            Visit(n.Statement);
            DecreaseIndent();
            return n;
        }

        public AstNode VisitVariableDeclare(VariableDeclareNode n)
        {
            Append("var ");
            Visit(n.Name);
            return n;
        }   
    }
}
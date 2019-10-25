using System.Linq;
using Scoop.SyntaxTree;

namespace Scoop.Transpiling
{
    public class ConstructorNodeRenderer
    {
        private readonly CSharpTranspileVisitor _visitor;
        private readonly OutputRenderer _renderer;

        public ConstructorNodeRenderer(CSharpTranspileVisitor visitor, OutputRenderer renderer)
        {
            _visitor = visitor;
            _renderer = renderer;
        }

        public void Render(ConstructorNode n)
        {
            if (n.Name == null)
            {
                RenderUnnamedConstructor(n);
                return;
            }

            if (n.Name != null)
            {
                RenderNamedConstructor(n);
                return;
            }

            throw new CSharpTranspileException("Could not render ConstructorNode");
        }

        private void RenderUnnamedConstructor(ConstructorNode n)
        {
            RenderAttributes(n);
            _visitor.Visit(n.AccessModifier ?? new KeywordNode("private"));
            _renderer.Append(" ");
            _visitor.Visit(n.ClassName);
            RenderNormalParameters(n);
            RenderThisArgs(n);

            _renderer.AppendLineAndIndent();
            _renderer.Append("{");
            _renderer.IncreaseIndent();
            _renderer.AppendLineAndIndent();
            RenderStatements(n);

            _renderer.DecreaseIndent();
            _renderer.AppendLineAndIndent();
            _renderer.Append("}");
        }

        private void RenderNamedConstructor(ConstructorNode n)
        {
            // Render the args type
            RenderNamedArgsType(n);

            RenderAttributes(n);
            _visitor.Visit(n.AccessModifier ?? new KeywordNode("private"));
            _renderer.Append(" ");
            _visitor.Visit(n.ClassName);
            _renderer.Append("(__ctorArgs_");
            _visitor.Visit(n.Name);
            _renderer.Append(" __args)");
            RenderThisArgs(n);

            _renderer.AppendLineAndIndent();
            _renderer.Append("{");
            _renderer.IncreaseIndent();
            _renderer.AppendLineAndIndent();

            foreach (var param in n.Parameters)
            {
                _renderer.Append("var ");
                _visitor.Visit(param.Name);
                _renderer.Append(" = __args.");
                _visitor.Visit(param.Name);
                _renderer.AppendLineAndIndent(";");
            }

            RenderStatements(n);

            _renderer.DecreaseIndent();
            _renderer.AppendLineAndIndent();
            _renderer.Append("}");
        }

        private void RenderNamedArgsType(ConstructorNode n)
        {
            _renderer.Append("public class __ctorArgs_");
            _visitor.Visit(n.Name);
            _renderer.AppendLineAndIndent();
            _renderer.Append("{");
            _renderer.IncreaseIndent();
            _renderer.AppendLineAndIndent();

            _renderer.Append("public __ctorArgs_");
            _visitor.Visit(n.Name);
            RenderNormalParameters(n);
            _renderer.AppendLineAndIndent();
            _renderer.Append("{");
            _renderer.IncreaseIndent();
            _renderer.AppendLineAndIndent();

            foreach (var param in n.Parameters)
            {
                _renderer.Append("this.");
                _visitor.Visit(param.Name);
                _renderer.Append(" = ");
                _visitor.Visit(param.Name);
                _renderer.AppendLineAndIndent(";");
            }

            _renderer.DecreaseIndent();
            _renderer.AppendLineAndIndent();
            _renderer.AppendLineAndIndent("}");


            foreach (var param in n.Parameters)
            {
                _renderer.Append("public ");
                _visitor.Visit(param.Type);
                _renderer.Append(" ");
                _visitor.Visit(param.Name);
                _renderer.AppendLineAndIndent(" { get; private set; }");
            }

            _renderer.DecreaseIndent();
            _renderer.AppendLineAndIndent();
            _renderer.AppendLineAndIndent("}");
            _renderer.AppendLineAndIndent();
        }

        private void RenderStatements(ConstructorNode n)
        {
            _renderer.AppendLineAndIndent("// Do not allow concrete inheritance");
            _renderer.AppendLineAndIndent("System.Diagnostics.Debug.Assert(GetType().BaseType == typeof(object));");

            foreach (var s in n.Statements.OrEmptyIfNull())
            {
                _visitor.Visit(s);
                _renderer.AppendLineAndIndent(";");
                _renderer.AppendLineAndIndent($"#line {s.Location.Line} \"{s.Location.FileName}\"");
                _renderer.AppendLineAndIndent();
            }
        }

        private void RenderThisArgs(ConstructorNode n)
        {
            if (!n.ThisArgs.IsNullOrEmpty())
            {
                _renderer.IncreaseIndent();
                _renderer.AppendLineAndIndent();
                _renderer.Append(": this(");
                _visitor.Visit(n.ThisArgs[0]);
                foreach (var a in n.ThisArgs.Skip(1))
                {
                    _renderer.Append(", ");
                    _visitor.Visit(a);
                }

                _renderer.Append(")");
                _renderer.DecreaseIndent();
            }
        }

        private void RenderNormalParameters(ConstructorNode n)
        {
            _renderer.Append("(");
            if (!n.Parameters.IsNullOrEmpty())
            {
                _visitor.Visit(n.Parameters[0]);
                for (var i = 1; i < n.Parameters.Count; i++)
                {
                    _renderer.Append(", ");
                    _visitor.Visit(n.Parameters[i]);
                }
            }

            _renderer.Append(")");
        }

        private void RenderAttributes(ConstructorNode n)
        {
            if (!n.Attributes.IsNullOrEmpty())
            {
                foreach (var a in n.Attributes)
                    _visitor.Visit(a);
            }
        }
    }
}

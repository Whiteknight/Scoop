using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Scoop.SyntaxTree;

namespace Scoop.Transpiler
{
    [Serializable]
    public class CSharpTranspileException : Exception
    {
        public CSharpTranspileException()
        {
        }

        public CSharpTranspileException(string message) : base(message)
        {
        }

        public CSharpTranspileException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CSharpTranspileException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    public class NewNodeRenderer
    {
        private readonly CSharpTranspileVisitor _visitor;
        private readonly OutputRenderer _renderer;

        public NewNodeRenderer(CSharpTranspileVisitor visitor, OutputRenderer renderer)
        {
            _visitor = visitor;
            _renderer = renderer;
        }

        public void Render(NewNode n)
        {

            // Tries to output one of the following forms:
            // new {+}
            if (n.Type == null && n.Name == null && n.Arguments.IsNullOrEmpty() && !n.Initializers.IsNullOrEmpty())
            {
                _renderer.Append("new");
                RenderInitializers(n);
                return;
            }

            // new MyType {+}
            if (n.Type != null && n.Name == null && n.Arguments.IsNullOrEmpty() && !n.Initializers.IsNullOrEmpty())
            {
                _renderer.Append("new");
                _renderer.Append(" ");
                _visitor.Visit(n.Type);
                RenderInitializers(n);
                return;
            }

            // new MyType()
            if (n.Type != null && n.Name == null && n.Arguments.IsNullOrEmpty() && n.Initializers.IsNullOrEmpty())
            {
                _renderer.Append("new");
                _renderer.Append(" ");
                _visitor.Visit(n.Type);
                RenderArguments(n);
                return;
            }

            // new MyType(+)
            if (n.Type != null && n.Name == null && !n.Arguments.IsNullOrEmpty() && n.Initializers.IsNullOrEmpty())
            {
                _renderer.Append("new");
                _renderer.Append(" ");
                _visitor.Visit(n.Type);
                RenderArguments(n);
                return;
            }

            // new MyType(+) {+}
            if (n.Type != null && n.Name == null && !n.Arguments.IsNullOrEmpty() && !n.Initializers.IsNullOrEmpty())
            {
                _renderer.Append("new");
                _renderer.Append(" ");
                _visitor.Visit(n.Type);
                RenderArguments(n);
                RenderInitializers(n);
                return;
            }

            // new MyType:Name(+) -> new MyType(new MyType._NewArgs_Name(*))
            if (n.Type != null && n.Name != null && n.Initializers.IsNullOrEmpty())
            {
                _renderer.Append("new");
                _renderer.Append(" ");
                _visitor.Visit(n.Type);
                _renderer.Append("(new ");
                _visitor.Visit(n.Type);
                _renderer.Append(".__ctorArgs_");
                _visitor.Visit(n.Name);
                RenderArguments(n);
                _renderer.Append(")");
                return;
            }

            // new MyType:Name(+) {+} -> new MyType(new MyType._NewArgs_Name(*)) {+}
            if (n.Type != null && n.Name != null && !n.Arguments.IsNullOrEmpty() && !n.Initializers.IsNullOrEmpty())
            {
                _renderer.Append("new");
                _renderer.Append(" ");
                _visitor.Visit(n.Type);
                _renderer.Append("(new ");
                _visitor.Visit(n.Type);
                _renderer.Append(".__ctorArgs_");
                _visitor.Visit(n.Name);
                RenderArguments(n);
                _renderer.Append(")");
                RenderInitializers(n);
                return;
            }

            throw new CSharpTranspileException("Could not render NewNode");
        }

        private void RenderInitializers(NewNode n)
        {
            if (!n.Initializers.IsNullOrEmpty())
            {
                _renderer.Append(" {");
                _renderer.IncreaseIndent();
                _renderer.AppendLineAndIndent();
                _visitor.Visit(n.Initializers[0]);
                for (var i = 1; i < n.Initializers.Count; i++)
                {
                    _renderer.AppendLineAndIndent(",");
                    _visitor.Visit(n.Initializers[i]);
                }

                _renderer.DecreaseIndent();
                _renderer.AppendLineAndIndent();
                _renderer.Append("}");
            }
        }


        private void RenderArguments(NewNode n)
        {
            _renderer.Append("(");
            if (!n.Arguments.IsNullOrEmpty())
            {
                _visitor.Visit(n.Arguments[0]);
                for (var i = 1; i < n.Arguments.Count; i++)
                {
                    _renderer.Append(", ");
                    _visitor.Visit(n.Arguments[i]);
                }
            }

            _renderer.Append(")");
        }
    }
}

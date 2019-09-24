using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scoop.SyntaxTree.Visiting;

namespace Scoop.SyntaxTree
{
    public class ListNode<TNode> : AstNode, IList<TNode>
        where TNode : AstNode
    {
        public AstNode Separator { get; set; }
        public List<TNode> Items { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitList(this);

        public IEnumerator<TNode> GetEnumerator() => (Items ?? Enumerable.Empty<TNode>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => (Items ?? Enumerable.Empty<TNode>()).GetEnumerator();

        public static ListNode<TNode> Default(string separator = ",")
        {
            return new ListNode<TNode>
            {
                Separator = new OperatorNode(separator),
                Items = new List<TNode>()
            };
        }

        public void Add(TNode item)
        {
            if (Items == null)
                Items = new List<TNode>();
            Items.Add(item);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public bool Contains(TNode item) => Items.Contains(item);

        public void CopyTo(TNode[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public bool Remove(TNode item) => Items.Remove(item);

        public int Count => Items?.Count ?? 0;
        public bool IsReadOnly => false;
        public int IndexOf(TNode item) => Items.IndexOf(item);

        public void Insert(int index, TNode item)
        {
            Items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Items.RemoveAt(index);
        }

        public TNode this[int index]
        {
            get => Items[index];
            set
            {
                if (Items == null)
                    Items = new List<TNode>();
                Items.Insert(index, value);
            }
        }
    }
}

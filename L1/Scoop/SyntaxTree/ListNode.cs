using System.Collections;
using System.Collections.Generic;

namespace Scoop.SyntaxTree
{
    public class ListNode : AstNode, IList<AstNode>
    {

        public List<AstNode> Items { get; set; }
        public override AstNode Accept(IAstNodeVisitorImplementation visitor) => visitor.VisitList(this);

        public IEnumerator<AstNode> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

        public void Add(AstNode item)
        {
            Items.Add(item);
        }

        public void Clear()
        {
            Items.Clear();
        }

        public bool Contains(AstNode item) => Items.Contains(item);

        public void CopyTo(AstNode[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public bool Remove(AstNode item) => Items.Remove(item);

        public int Count => Items.Count;
        public bool IsReadOnly => false;
        public int IndexOf(AstNode item) => Items.IndexOf(item);

        public void Insert(int index, AstNode item)
        {
            Items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Items.RemoveAt(index);
        }

        public AstNode this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }
    }
}

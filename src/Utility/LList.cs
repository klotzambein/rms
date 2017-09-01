using System;

namespace Utility
{
    /// <summary>
    /// Linked List
    /// </summary>
    public class LList<T>
    {
        public LListNode<T> First { get; set; }
        public void AddFirst(T value)
        {
            First = new LListNode<T>(value, First);
        }
        public void EnumerateWithRemoval(Func<T, bool> shouldRemove)
        {
            LListNode<T> lastNode = null;
            LListNode<T> currNode = First;
            while (currNode != null)
            {
                if (shouldRemove(currNode.Value))
                {
                    if (lastNode == null)
                        First = currNode.Next;
                    else
                        lastNode = currNode.Next;
                }
                else
                    lastNode = currNode;

                currNode = currNode.Next;
            }
        }

        public T this[int index]
        {
            get
            {
                var c = First;
                for (int i = 0; i < index; i++)
                    c = c.Next;

                return c.Value;
            }
            set
            {
                var c = First;
                for (int i = 0; i < index; i++)
                    c = c.Next;

                c.Value = value;
            }
        }

        public bool TryFindWhere(Func<T, bool> predicate, out T result)
        {
            var c = First;
            while (c != null)
            {
                if (predicate(c.Value))
                {
                    result = c.Value;
                    return true;
                }
                c = c.Next;
            }
            result = default(T);
            return false;
        }
        public T FindWhere(Func<T, bool> predicate)
        {
            T result;
            if (TryFindWhere(predicate, out result))
                return result;
            return default(T);
        }

        public T PopFirstWhere(Func<T, bool> predicate)
        {
            var last = (LListNode<T>)null;
            var cur = First;
            while (!predicate(cur.Value))
            {
                last = cur;
                cur = cur.Next;
                if (cur == null)
                    return default(T);
            }
            if (last == null)
                First.Next = cur.Next;
            else
                last.Next = cur.Next;
            return cur.Value;
        }
    }

    public class LListNode<T>
    {
        public T Value { get; set; }
        public LListNode<T> Next { get; set; }

        public LListNode(T value, LListNode<T> next)
        {
            Value = value;
            Next = next;
        }
    }
}
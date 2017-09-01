using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    /// <summary>
    /// Linked List always containing one element
    /// </summary>
    public class OneItemLList<T> : IEnumerable<T>
    {
        public OneItemLListNode<T> First
        {
            get
            {
                return new FirstOneItemLListNode(this);
            }
            set
            {
                firstValue = value.Value;
                second = value.Next;
            }
        }
        private T firstValue;
        private OneItemLListNode<T> second;

        public T FirstValue { get => firstValue; set => firstValue = value; }
        public bool HasMore { get => second != null; }

        public OneItemLList(T firstValue)
        {
            this.firstValue = firstValue;
        }
        public OneItemLList(IEnumerable<T> values)
        {
            firstValue = values.First();

            var currNode = First;
            foreach (var value in values.Skip(1))
            {
                var newNode = OneItemLListNode<T>.New(value, null);
                currNode.Next = newNode;
                currNode = newNode;
            }
        }

        public void AddFirst(T value)
        {
            First = OneItemLListNode<T>.New(value, OneItemLListNode<T>.New(First.Value, First.Next));
        }
        public void EnumerateWithRemoval(Func<T, bool> shouldRemove)
        {
            OneItemLListNode<T> lastNode = null;
            OneItemLListNode<T> currNode = First;
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
        public void Foreach(Action<T> action)
        {
            action(firstValue);

            OneItemLListNode<T> currNode = First;
            while (currNode != null)
            {
                action(currNode.Value);
                currNode = currNode.Next;
            }
        }

        public IEnumerable<T> ToEnumerable()
        {
            OneItemLListNode<T> currNode = First;
            while (currNode != null)
            {
                yield return currNode.Value;
                currNode = currNode.Next;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index == 0)
                    return firstValue;

                var c = second;
                for (int i = 1; i < index; i++)
                    c = c.Next;

                return c.Value;
            }
            set
            {
                if (index == 0)
                    firstValue = value;

                var c = second;
                for (int i = 1; i < index; i++)
                    c = c.Next;

                c.Value = value;
            }
        }

        public bool Exists(Func<T, bool> predicate)
        {
            if (predicate(firstValue))
                return true;

            var c = second;
            while (c != null)
            {
                if (predicate(c.Value))
                    return true;
                c = c.Next;
            }

            return false;
        }

        public bool TryFindWhere(Func<T, bool> predicate, out T result)
        {
            if (predicate(firstValue))
            {
                result = firstValue;
                return true;
            }

            var c = second;
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

        public IEnumerator<T> GetEnumerator()
        {
            yield return firstValue;
            var c = second;
            while (c != null)
            {
                yield return c.Value;
                c = c.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class FirstOneItemLListNode : OneItemLListNode<T>
        {
            public override T Value { get { return _list.firstValue; } set { _list.firstValue = value; } }
            public override OneItemLListNode<T> Next { get { return _list.second; } set { _list.second = value; } }

            private OneItemLList<T> _list;

            public FirstOneItemLListNode(OneItemLList<T> _list)
            {
                this._list = _list;
            }
        }

        public override string ToString()
        {
            return $"[{this.Aggregate("", (a, f) => a + f.ToString() + ",").TrimEnd(',')}]";
        }
    }

    public abstract class OneItemLListNode<T>
    {
        public abstract T Value { get; set; }
        public abstract OneItemLListNode<T> Next { get; set; }

        public static OneItemLListNode<T> New(T value, OneItemLListNode<T> next)
        {
            return new ConcreteOneItemLListNode() { Value = value, Next = next };
        }

        class ConcreteOneItemLListNode : OneItemLListNode<T>
        {
            public override T Value { get; set; }
            public override OneItemLListNode<T> Next { get; set; }
        }
    }
}
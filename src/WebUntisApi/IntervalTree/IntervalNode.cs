using System;
using System.Text;

namespace IntervalArray
{

    /// <summary>
    /// Node of interval Tree
    /// </summary>
    /// <typeparam name="T">type of interval bounds</typeparam>
    internal class IntervalNode<TKey, TValue> : IComparable<IntervalNode<TKey, TValue>> where TKey : struct, IComparable<TKey>
    {

        public IntervalNode<TKey, TValue> Left { get; set; }
        public IntervalNode<TKey, TValue> Right { get; set; }
        public IntervalNode<TKey, TValue> Parent { get; set; }

        /// <summary>
        /// Maximum "end" value of interval in node subtree
        /// </summary>
        public TKey MaxEnd { get; set; }

        /// <summary>
        /// The interval this node holds
        /// </summary>
        public Interval<TKey> Interval;
        public TValue Value;

        private NodeColor color;
        /// <summary>
        /// Color of the node used for R-B implementation
        /// </summary>
        public NodeColor Color
        {
            get { return color; }
            set { this.color = value; }
        }

        public IntervalNode()
        {
            Parent = Left = Right = IntervalTree<TKey, TValue>.Sentinel;
            Color = NodeColor.BLACK;
        }

        public IntervalNode(Interval<TKey> interval, TValue value) : this()
        {
            MaxEnd = interval.End;
            Interval = interval;
            Value = value;
        }

        /// <summary>
        /// Indicates wheter the node has a parent
        /// </summary>
        public bool IsRoot
        {
            get { return Parent == IntervalTree<TKey, TValue>.Sentinel; }
        }

        /// <summary>
        /// Indicator whether the node has children
        /// </summary>
        public bool IsLeaf
        {
            get { return Right == IntervalTree<TKey, TValue>.Sentinel && Left == IntervalTree<TKey, TValue>.Sentinel; }
        }

        /// <summary>
        /// The direction of the parent, from the child point-of-view
        /// </summary>
        public NodeDirection ParentDirection
        {
            get
            {
                if (Parent == IntervalTree<TKey, TValue>.Sentinel)
                {
                    return NodeDirection.NONE;
                }

                return Parent.Left == this ? NodeDirection.RIGHT : NodeDirection.LEFT;
            }
        }

        public IntervalNode<TKey, TValue> GetSuccessor()
        {
            if (Right == IntervalTree<TKey, TValue>.Sentinel)
            {
                return IntervalTree<TKey, TValue>.Sentinel;
            }

            var node = Right;
            while (node.Left != IntervalTree<TKey, TValue>.Sentinel)
            {
                node = node.Left;
            }

            return node;
        }

        public int CompareTo(IntervalNode<TKey, TValue> other)
        {
            return Interval.CompareTo(other.Interval);
        }

        /// <summary>
        /// Refreshes the MaxEnd value after node manipulation
        /// 
        /// This is a local operation only
        /// </summary>
        public void RecalculateMaxEnd()
        {
            TKey max = Interval.End;

            if (Right != IntervalTree<TKey, TValue>.Sentinel)
            {
                if (Right.MaxEnd.CompareTo(max) > 0)
                {
                    max = Right.MaxEnd;
                }
            }

            if (Left != IntervalTree<TKey, TValue>.Sentinel)
            {
                if (Left.MaxEnd.CompareTo(max) > 0)
                {
                    max = Left.MaxEnd;
                }
            }

            MaxEnd = max;

            if (Parent != IntervalTree<TKey, TValue>.Sentinel)
            {
                Parent.RecalculateMaxEnd();
            }
        }

        /// <summary>
        /// Return grandparent node
        /// </summary>
        /// <returns>grandparent node or IntervalTree<TKey, TValue>.Sentinel if none</returns>
        public IntervalNode<TKey, TValue> GrandParent
        {
            get
            {
                if (Parent != IntervalTree<TKey, TValue>.Sentinel)
                {
                    return Parent.Parent;
                }
                return IntervalTree<TKey, TValue>.Sentinel;
            }
        }

        /// <summary>
        /// Returns sibling of parent node
        /// </summary>
        /// <returns>sibling of parent node or IntervalTree<TKey, TValue>.Sentinel if none</returns>
        public IntervalNode<TKey, TValue> Uncle
        {
            get
            {
                var gparent = GrandParent;
                if (gparent == IntervalTree<TKey, TValue>.Sentinel)
                {
                    return IntervalTree<TKey, TValue>.Sentinel;
                }

                if (Parent == gparent.Left)
                {
                    return gparent.Right;
                }

                return gparent.Left;
            }
        }

        /// <summary>
        /// Returns sibling node
        /// </summary>
        /// <returns>sibling node or IntervalTree<TKey, TValue>.Sentinel if none</returns>
        public IntervalNode<TKey, TValue> Sibling
        {
            get
            {
                if (Parent != IntervalTree<TKey, TValue>.Sentinel)
                {
                    if (Parent.Right == this)
                    {
                        return Parent.Left;
                    }

                    return Parent.Right;
                }

                return IntervalTree<TKey, TValue>.Sentinel;
            }
        }
    }

    internal enum NodeColor
    {
        RED,
        BLACK
    }

    internal enum NodeDirection
    {
        LEFT,
        RIGHT,
        NONE
    }
}

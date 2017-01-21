namespace ProtoBuf.Meta
{
    using ProtoBuf;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class BasicList : IEnumerable
    {
        protected Node head = nil;
        private static readonly Node nil = new Node(null, 0);

        public int Add(object value)
        {
            return ((this.head = this.head.Append(value)).Length - 1);
        }

        internal bool Contains(object value)
        {
            NodeEnumerator enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                if (object.Equals(current, value))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(Array array, int offset)
        {
            this.head.CopyTo(array, offset);
        }

        internal static BasicList GetContiguousGroups(int[] keys, object[] values)
        {
            int num2;
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (values.Length < keys.Length)
            {
                throw new ArgumentException("Not all keys are covered by values", "values");
            }
            BasicList list = new BasicList();
            Group group = null;
            for (int i = 0; i < keys.Length; i = num2 + 1)
            {
                if ((i == 0) || (keys[i] != keys[i - 1]))
                {
                    group = null;
                }
                if (group == null)
                {
                    group = new Group(keys[i]);
                    list.Add(group);
                }
                group.Items.Add(values[i]);
                num2 = i;
            }
            return list;
        }

        public NodeEnumerator GetEnumerator()
        {
            return new NodeEnumerator(this.head);
        }

        internal int IndexOf(MatchPredicate predicate, object ctx)
        {
            return this.head.IndexOf(predicate, ctx);
        }

        internal int IndexOfReference(object instance)
        {
            return this.head.IndexOfReference(instance);
        }

        internal int IndexOfString(string value)
        {
            return this.head.IndexOfString(value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new NodeEnumerator(this.head);
        }

        public void Trim()
        {
            this.head = this.head.Trim();
        }

        public int Count
        {
            get
            {
                return this.head.Length;
            }
        }

        public object this[int index]
        {
            get
            {
                return this.head[index];
            }
        }

        internal sealed class Group
        {
            public readonly int First;
            public readonly BasicList Items;

            public Group(int first)
            {
                this.First = first;
                this.Items = new BasicList();
            }
        }

        internal delegate bool MatchPredicate(object value, object ctx);

        internal sealed class Node
        {
            private readonly object[] data;
            private int length;

            internal Node(object[] data, int length)
            {
                Helpers.DebugAssert(((data == null) && (length == 0)) || (((data != null) && (length > 0)) && (length <= data.Length)));
                this.data = data;
                this.length = length;
            }

            public BasicList.Node Append(object value)
            {
                object[] data;
                int length = this.length + 1;
                if (this.data == null)
                {
                    data = new object[10];
                }
                else if (this.length == this.data.Length)
                {
                    data = new object[this.data.Length * 2];
                    Array.Copy(this.data, data, this.length);
                }
                else
                {
                    data = this.data;
                }
                data[this.length] = value;
                return new BasicList.Node(data, length);
            }

            internal void Clear()
            {
                if (this.data > null)
                {
                    Array.Clear(this.data, 0, this.data.Length);
                }
                this.length = 0;
            }

            internal void CopyTo(Array array, int offset)
            {
                if (this.length > 0)
                {
                    Array.Copy(this.data, 0, array, offset, this.length);
                }
            }

            internal int IndexOf(BasicList.MatchPredicate predicate, object ctx)
            {
                int num3;
                for (int i = 0; i < this.length; i = num3 + 1)
                {
                    if (predicate(this.data[i], ctx))
                    {
                        return i;
                    }
                    num3 = i;
                }
                return -1;
            }

            internal int IndexOfReference(object instance)
            {
                int num3;
                for (int i = 0; i < this.length; i = num3 + 1)
                {
                    if (instance == this.data[i])
                    {
                        return i;
                    }
                    num3 = i;
                }
                return -1;
            }

            internal int IndexOfString(string value)
            {
                int num3;
                for (int i = 0; i < this.length; i = num3 + 1)
                {
                    if (value == ((string) this.data[i]))
                    {
                        return i;
                    }
                    num3 = i;
                }
                return -1;
            }

            public void RemoveLastWithMutate()
            {
                if (this.length == 0)
                {
                    throw new InvalidOperationException();
                }
                this.length--;
            }

            public BasicList.Node Trim()
            {
                if ((this.length == 0) || (this.length == this.data.Length))
                {
                    return this;
                }
                object[] destinationArray = new object[this.length];
                Array.Copy(this.data, destinationArray, this.length);
                return new BasicList.Node(destinationArray, this.length);
            }

            public object this[int index]
            {
                get
                {
                    if ((index < 0) || (index >= this.length))
                    {
                        throw new ArgumentOutOfRangeException("index");
                    }
                    return this.data[index];
                }
                set
                {
                    if ((index < 0) || (index >= this.length))
                    {
                        throw new ArgumentOutOfRangeException("index");
                    }
                    this.data[index] = value;
                }
            }

            public int Length
            {
                get
                {
                    return this.length;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NodeEnumerator : IEnumerator
        {
            private int position;
            private readonly BasicList.Node node;
            internal NodeEnumerator(BasicList.Node node)
            {
                this.position = -1;
                this.node = node;
            }

            void IEnumerator.Reset()
            {
                this.position = -1;
            }

            public object Current
            {
                get
                {
                    return this.node[this.position];
                }
            }
            public bool MoveNext()
            {
                int length = this.node.Length;
                return ((this.position <= length) && ((this.position + 1) < length));
            }
        }
    }
}


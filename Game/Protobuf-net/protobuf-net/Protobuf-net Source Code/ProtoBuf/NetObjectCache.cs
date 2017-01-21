namespace ProtoBuf
{
    using ProtoBuf.Meta;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class NetObjectCache
    {
        private Dictionary<object, int> objectKeys;
        internal const int Root = 0;
        private object rootObject;
        private Dictionary<string, int> stringKeys;
        private int trapStartIndex;
        private MutableList underlyingList;

        internal int AddObjectKey(object value, out bool existing)
        {
            int num;
            bool flag9;
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value == this.rootObject)
            {
                existing = true;
                return 0;
            }
            string key = value as string;
            BasicList list = this.List;
            if (key == null)
            {
                if (this.objectKeys == null)
                {
                    this.objectKeys = new Dictionary<object, int>(ReferenceComparer.Default);
                    num = -1;
                }
                else if (!this.objectKeys.TryGetValue(value, out num))
                {
                    num = -1;
                }
            }
            else if (this.stringKeys == null)
            {
                this.stringKeys = new Dictionary<string, int>();
                num = -1;
            }
            else if (!this.stringKeys.TryGetValue(key, out num))
            {
                num = -1;
            }
            existing = flag9 = num >= 0;
            if (!flag9)
            {
                num = list.Add(value);
                if (key == null)
                {
                    this.objectKeys.Add(value, num);
                }
                else
                {
                    this.stringKeys.Add(key, num);
                }
            }
            return (num + 1);
        }

        internal void Clear()
        {
            this.trapStartIndex = 0;
            this.rootObject = null;
            if (this.underlyingList > null)
            {
                this.underlyingList.Clear();
            }
            if (this.stringKeys > null)
            {
                this.stringKeys.Clear();
            }
            if (this.objectKeys > null)
            {
                this.objectKeys.Clear();
            }
        }

        internal object GetKeyedObject(int key)
        {
            int num = key;
            key = num - 1;
            if (num == 0)
            {
                if (this.rootObject == null)
                {
                    throw new ProtoException("No root object assigned");
                }
                return this.rootObject;
            }
            BasicList list = this.List;
            if ((key < 0) || (key >= list.Count))
            {
                Helpers.DebugWriteLine("Missing key: " + key);
                throw new ProtoException("Internal error; a missing key occurred");
            }
            object obj2 = list[key];
            if (obj2 == null)
            {
                throw new ProtoException("A deferred key does not have a value yet");
            }
            return obj2;
        }

        internal void RegisterTrappedObject(object value)
        {
            if (this.rootObject == null)
            {
                this.rootObject = value;
            }
            else if (this.underlyingList > null)
            {
                int num2;
                for (int i = this.trapStartIndex; i < this.underlyingList.Count; i = num2 + 1)
                {
                    this.trapStartIndex = i + 1;
                    if (this.underlyingList[i] == null)
                    {
                        this.underlyingList[i] = value;
                        break;
                    }
                    num2 = i;
                }
            }
        }

        internal void SetKeyedObject(int key, object value)
        {
            int num = key;
            key = num - 1;
            if (num == 0)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if ((this.rootObject != null) && (this.rootObject != value))
                {
                    throw new ProtoException("The root object cannot be reassigned");
                }
                this.rootObject = value;
            }
            else
            {
                MutableList list = this.List;
                if (key < list.Count)
                {
                    object obj2 = list[key];
                    if (obj2 == null)
                    {
                        list[key] = value;
                    }
                    else if (obj2 != value)
                    {
                        throw new ProtoException("Reference-tracked objects cannot change reference");
                    }
                }
                else if (key != list.Add(value))
                {
                    throw new ProtoException("Internal error; a key mismatch occurred");
                }
            }
        }

        private MutableList List
        {
            get
            {
                if (this.underlyingList == null)
                {
                    this.underlyingList = new MutableList();
                }
                return this.underlyingList;
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly NetObjectCache.ReferenceComparer Default = new NetObjectCache.ReferenceComparer();

            private ReferenceComparer()
            {
            }

            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                return (x == y);
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}


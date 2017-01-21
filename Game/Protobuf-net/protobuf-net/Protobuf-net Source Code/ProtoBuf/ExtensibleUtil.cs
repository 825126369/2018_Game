namespace ProtoBuf
{
    using ProtoBuf.Meta;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class ExtensibleUtil
    {
        internal static void AppendExtendValue(TypeModel model, IExtensible instance, int tag, DataFormat format, object value)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            IExtension extensionObject = instance.GetExtensionObject(true);
            if (extensionObject == null)
            {
                throw new InvalidOperationException("No extension object available; appended data would be lost.");
            }
            bool commit = false;
            Stream dest = extensionObject.BeginAppend();
            try
            {
                using (ProtoWriter writer = new ProtoWriter(dest, model, null))
                {
                    model.TrySerializeAuxiliaryType(writer, null, format, tag, value, false);
                    writer.Close();
                }
                commit = true;
            }
            finally
            {
                extensionObject.EndAppend(dest, commit);
            }
        }

        internal static IEnumerable<TValue> GetExtendedValues<TValue>(IExtensible instance, int tag, DataFormat format, bool singleton, bool allowDefinedTag)
        {
            this.<>s__1 = GetExtendedValues(RuntimeTypeModel.Default, typeof(TValue), instance, tag, format, singleton, allowDefinedTag).GetEnumerator();
            while (this.<>s__1.MoveNext())
            {
                this.<value>5__2 = (TValue) this.<>s__1.Current;
                yield return this.<value>5__2;
                this.<value>5__2 = default(TValue);
            }
            this.<>s__1 = null;
        }

        internal static IEnumerable GetExtendedValues(TypeModel model, Type type, IExtensible instance, int tag, DataFormat format, bool singleton, bool allowDefinedTag)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (tag <= 0)
            {
                throw new ArgumentOutOfRangeException("tag");
            }
            this.<extn>5__1 = instance.GetExtensionObject(false);
            if (this.<extn>5__1 == null)
            {
            }
            this.<stream>5__2 = this.<extn>5__1.BeginQuery();
            this.<value>5__3 = null;
            this.<reader>5__4 = null;
            this.<ctx>5__5 = new SerializationContext();
            this.<reader>5__4 = ProtoReader.Create(this.<stream>5__2, model, this.<ctx>5__5, -1);
            while (model.TryDeserializeAuxiliaryType(this.<reader>5__4, format, tag, type, ref this.<value>5__3, true, false, false, false) && (this.<value>5__3 > null))
            {
                if (singleton)
                {
                    continue;
                }
                yield return this.<value>5__3;
                this.<value>5__3 = null;
            }
            if (singleton && (this.<value>5__3 > null))
            {
                yield return this.<value>5__3;
            }
            this.<ctx>5__5 = null;
        }

        [CompilerGenerated]
        private sealed class <GetExtendedValues>d__0<TValue> : IEnumerable<TValue>, IEnumerable, IEnumerator<TValue>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private TValue <>2__current;
            public bool <>3__allowDefinedTag;
            public DataFormat <>3__format;
            public IExtensible <>3__instance;
            public bool <>3__singleton;
            public int <>3__tag;
            private int <>l__initialThreadId;
            private IEnumerator <>s__1;
            private TValue <value>5__2;
            private bool allowDefinedTag;
            private DataFormat format;
            private IExtensible instance;
            private bool singleton;
            private int tag;

            [DebuggerHidden]
            public <GetExtendedValues>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                IDisposable disposable = this.<>s__1 as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>s__1 = ExtensibleUtil.GetExtendedValues(RuntimeTypeModel.Default, typeof(TValue), this.instance, this.tag, this.format, this.singleton, this.allowDefinedTag).GetEnumerator();
                            this.<>1__state = -3;
                            while (this.<>s__1.MoveNext())
                            {
                                this.<value>5__2 = (TValue) this.<>s__1.Current;
                                this.<>2__current = this.<value>5__2;
                                this.<>1__state = 1;
                                return true;
                            Label_009E:
                                this.<>1__state = -3;
                                this.<value>5__2 = default(TValue);
                            }
                            this.<>m__Finally1();
                            this.<>s__1 = null;
                            return false;

                        case 1:
                            goto Label_009E;
                    }
                    return false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                ExtensibleUtil.<GetExtendedValues>d__0<TValue> d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Thread.CurrentThread.ManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = (ExtensibleUtil.<GetExtendedValues>d__0<TValue>) this;
                }
                else
                {
                    d__ = new ExtensibleUtil.<GetExtendedValues>d__0<TValue>(0);
                }
                d__.instance = this.<>3__instance;
                d__.tag = this.<>3__tag;
                d__.format = this.<>3__format;
                d__.singleton = this.<>3__singleton;
                d__.allowDefinedTag = this.<>3__allowDefinedTag;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<TValue>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                switch (this.<>1__state)
                {
                    case -3:
                    case 1:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally1();
                        }
                        break;
                }
            }

            TValue IEnumerator<TValue>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

    }
}


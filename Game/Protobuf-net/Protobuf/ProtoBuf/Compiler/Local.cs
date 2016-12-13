namespace ProtoBuf.Compiler
{
    using System;
    using System.Reflection.Emit;

    internal sealed class Local : IDisposable
    {
        private CompilerContext ctx;
        private readonly System.Type type;
        private LocalBuilder value;

        internal Local(CompilerContext ctx, System.Type type)
        {
            this.ctx = ctx;
            if (ctx > null)
            {
                this.value = ctx.GetFromPool(type);
            }
            this.type = type;
        }

        private Local(LocalBuilder value, System.Type type)
        {
            this.value = value;
            this.type = type;
        }

        public Local AsCopy()
        {
            if (this.ctx == null)
            {
                return this;
            }
            return new Local(this.value, this.type);
        }

        public void Dispose()
        {
            if (this.ctx > null)
            {
                this.ctx.ReleaseToPool(this.value);
                this.value = null;
                this.ctx = null;
            }
        }

        internal bool IsSame(Local other)
        {
            if (this == other)
            {
                return true;
            }
            object obj2 = this.value;
            return ((other != null) && (obj2 == other.value));
        }

        public System.Type Type
        {
            get
            {
                return this.type;
            }
        }

        internal LocalBuilder Value
        {
            get
            {
                if (this.value == null)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.value;
            }
        }
    }
}


namespace ProtoBuf
{
    using System;
    using System.Runtime.Serialization;

    public sealed class SerializationContext
    {
        private object context;
        private static readonly SerializationContext @default = new SerializationContext();
        private bool frozen;
        private StreamingContextStates state = StreamingContextStates.Persistence;

        static SerializationContext()
        {
            @default.Freeze();
        }

        internal void Freeze()
        {
            this.frozen = true;
        }

        public static implicit operator StreamingContext(SerializationContext ctx)
        {
            if (ctx == null)
            {
                return new StreamingContext(StreamingContextStates.Persistence);
            }
            return new StreamingContext(ctx.state, ctx.context);
        }

        public static implicit operator SerializationContext(StreamingContext ctx)
        {
            return new SerializationContext { Context = ctx.Context, State = ctx.State };
        }

        private void ThrowIfFrozen()
        {
            if (this.frozen)
            {
                throw new InvalidOperationException("The serialization-context cannot be changed once it is in use");
            }
        }

        public object Context
        {
            get
            {
                return this.context;
            }
            set
            {
                if (this.context != value)
                {
                    this.ThrowIfFrozen();
                    this.context = value;
                }
            }
        }

        internal static SerializationContext Default
        {
            get
            {
                return @default;
            }
        }

        public StreamingContextStates State
        {
            get
            {
                return this.state;
            }
            set
            {
                if (this.state != value)
                {
                    this.ThrowIfFrozen();
                    this.state = value;
                }
            }
        }
    }
}


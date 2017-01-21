namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using System;
    using System.Reflection;

    internal sealed class FieldDecorator : ProtoDecoratorBase
    {
        private readonly FieldInfo field;
        private readonly Type forType;

        public FieldDecorator(Type forType, FieldInfo field, IProtoSerializer tail) : base(tail)
        {
            Helpers.DebugAssert(forType > null);
            Helpers.DebugAssert(field > null);
            this.forType = forType;
            this.field = field;
        }

        protected override void EmitRead(CompilerContext ctx, Local valueFrom)
        {
            using (Local local = ctx.GetLocalWithValue(this.ExpectedType, valueFrom))
            {
                if (base.Tail.RequiresOldValue)
                {
                    ctx.LoadAddress(local, this.ExpectedType);
                    ctx.LoadValue(this.field);
                }
                ctx.ReadNullCheckedTail(this.field.FieldType, base.Tail, null);
                if (base.Tail.ReturnsValue)
                {
                    using (Local local2 = new Local(ctx, this.field.FieldType))
                    {
                        ctx.StoreValue(local2);
                        if (this.field.FieldType.IsValueType)
                        {
                            ctx.LoadAddress(local, this.ExpectedType);
                            ctx.LoadValue(local2);
                            ctx.StoreValue(this.field);
                        }
                        else
                        {
                            CodeLabel label = ctx.DefineLabel();
                            ctx.LoadValue(local2);
                            ctx.BranchIfFalse(label, true);
                            ctx.LoadAddress(local, this.ExpectedType);
                            ctx.LoadValue(local2);
                            ctx.StoreValue(this.field);
                            ctx.MarkLabel(label);
                        }
                    }
                }
            }
        }

        protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.LoadAddress(valueFrom, this.ExpectedType);
            ctx.LoadValue(this.field);
            ctx.WriteNullCheckedTail(this.field.FieldType, base.Tail, null);
        }

        public override object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value > null);
            object obj2 = base.Tail.Read(base.Tail.RequiresOldValue ? this.field.GetValue(value) : null, source);
            if (obj2 > null)
            {
                this.field.SetValue(value, obj2);
            }
            return null;
        }

        public override void Write(object value, ProtoWriter dest)
        {
            Helpers.DebugAssert(value > null);
            value = this.field.GetValue(value);
            if (value > null)
            {
                base.Tail.Write(value, dest);
            }
        }

        public override Type ExpectedType
        {
            get
            {
                return this.forType;
            }
        }

        public override bool RequiresOldValue
        {
            get
            {
                return true;
            }
        }

        public override bool ReturnsValue
        {
            get
            {
                return false;
            }
        }
    }
}


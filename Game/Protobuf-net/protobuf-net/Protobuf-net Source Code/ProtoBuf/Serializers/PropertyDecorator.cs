namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class PropertyDecorator : ProtoDecoratorBase
    {
        private readonly Type forType;
        private readonly PropertyInfo property;
        private readonly bool readOptionsWriteValue;
        private readonly MethodInfo shadowSetter;

        public PropertyDecorator(TypeModel model, Type forType, PropertyInfo property, IProtoSerializer tail) : base(tail)
        {
            Helpers.DebugAssert(forType > null);
            Helpers.DebugAssert(property > null);
            this.forType = forType;
            this.property = property;
            SanityCheck(model, property, tail, out this.readOptionsWriteValue, true, true);
            this.shadowSetter = GetShadowSetter(model, property);
        }

        internal static bool CanWrite(TypeModel model, MemberInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            PropertyInfo property = member as PropertyInfo;
            if (property > null)
            {
                return (property.CanWrite || (GetShadowSetter(model, property) > null));
            }
            return (member is FieldInfo);
        }

        protected override void EmitRead(CompilerContext ctx, Local valueFrom)
        {
            bool flag;
            SanityCheck(ctx.Model, this.property, base.Tail, out flag, ctx.NonPublic, ctx.AllowInternal(this.property));
            if (this.ExpectedType.IsValueType && (valueFrom == null))
            {
                throw new InvalidOperationException("Attempt to mutate struct on the head of the stack; changes would be lost");
            }
            using (Local local = ctx.GetLocalWithValue(this.ExpectedType, valueFrom))
            {
                if (base.Tail.RequiresOldValue)
                {
                    ctx.LoadAddress(local, this.ExpectedType);
                    ctx.LoadValue(this.property);
                }
                Type propertyType = this.property.PropertyType;
                ctx.ReadNullCheckedTail(propertyType, base.Tail, null);
                if (flag)
                {
                    using (Local local2 = new Local(ctx, this.property.PropertyType))
                    {
                        ctx.StoreValue(local2);
                        CodeLabel label = new CodeLabel();
                        if (!propertyType.IsValueType)
                        {
                            label = ctx.DefineLabel();
                            ctx.LoadValue(local2);
                            ctx.BranchIfFalse(label, true);
                        }
                        ctx.LoadAddress(local, this.ExpectedType);
                        ctx.LoadValue(local2);
                        if (this.shadowSetter == null)
                        {
                            ctx.StoreValue(this.property);
                        }
                        else
                        {
                            ctx.EmitCall(this.shadowSetter);
                        }
                        if (!propertyType.IsValueType)
                        {
                            ctx.MarkLabel(label);
                        }
                    }
                }
                else if (base.Tail.ReturnsValue)
                {
                    ctx.DiscardValue();
                }
            }
        }

        protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.LoadAddress(valueFrom, this.ExpectedType);
            ctx.LoadValue(this.property);
            ctx.WriteNullCheckedTail(this.property.PropertyType, base.Tail, null);
        }

        private static MethodInfo GetShadowSetter(TypeModel model, PropertyInfo property)
        {
            Type[] types = new Type[] { property.PropertyType };
            MethodInfo info = Helpers.GetInstanceMethod(property.ReflectedType, "Set" + property.Name, types);
            if (((info == null) || !info.IsPublic) || (info.ReturnType != model.MapType(typeof(void))))
            {
                return null;
            }
            return info;
        }

        public override object Read(object value, ProtoReader source)
        {
            Helpers.DebugAssert(value > null);
            object obj2 = base.Tail.RequiresOldValue ? this.property.GetValue(value, null) : null;
            object obj3 = base.Tail.Read(obj2, source);
            if (this.readOptionsWriteValue && (obj3 > null))
            {
                if (this.shadowSetter == null)
                {
                    this.property.SetValue(value, obj3, null);
                }
                else
                {
                    object[] parameters = new object[] { obj3 };
                    this.shadowSetter.Invoke(value, parameters);
                }
            }
            return null;
        }

        private static void SanityCheck(TypeModel model, PropertyInfo property, IProtoSerializer tail, out bool writeValue, bool nonPublic, bool allowInternal)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            writeValue = tail.ReturnsValue && ((GetShadowSetter(model, property) != null) || (property.CanWrite && (Helpers.GetSetMethod(property, nonPublic, allowInternal) > null)));
            if (!property.CanRead || (Helpers.GetGetMethod(property, nonPublic, allowInternal) == null))
            {
                throw new InvalidOperationException("Cannot serialize property without a get accessor");
            }
            if (!writeValue && (!tail.RequiresOldValue || Helpers.IsValueType(tail.ExpectedType)))
            {
                throw new InvalidOperationException("Cannot apply changes to property " + property.DeclaringType.FullName + "." + property.Name);
            }
        }

        public override void Write(object value, ProtoWriter dest)
        {
            Helpers.DebugAssert(value > null);
            value = this.property.GetValue(value, null);
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


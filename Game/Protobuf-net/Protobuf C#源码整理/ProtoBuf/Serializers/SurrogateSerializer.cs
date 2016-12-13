namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class SurrogateSerializer : IProtoTypeSerializer, IProtoSerializer
    {
        private readonly Type declaredType;
        private readonly Type forType;
        private readonly MethodInfo fromTail;
        private IProtoTypeSerializer rootTail;
        private readonly MethodInfo toTail;

        public SurrogateSerializer(Type forType, Type declaredType, IProtoTypeSerializer rootTail)
        {
            Helpers.DebugAssert(forType > null, "forType");
            Helpers.DebugAssert(declaredType > null, "declaredType");
            Helpers.DebugAssert(rootTail > null, "rootTail");
            Helpers.DebugAssert(rootTail.RequiresOldValue, "RequiresOldValue");
            Helpers.DebugAssert(!rootTail.ReturnsValue, "ReturnsValue");
            Helpers.DebugAssert((declaredType == rootTail.ExpectedType) || Helpers.IsSubclassOf(declaredType, rootTail.ExpectedType));
            this.forType = forType;
            this.declaredType = declaredType;
            this.rootTail = rootTail;
            this.toTail = this.GetConversion(true);
            this.fromTail = this.GetConversion(false);
        }

        public MethodInfo GetConversion(bool toTail)
        {
            MethodInfo info;
            Type to = toTail ? this.declaredType : this.forType;
            Type from = toTail ? this.forType : this.declaredType;
            if (!HasCast(this.declaredType, from, to, out info) && !HasCast(this.forType, from, to, out info))
            {
                throw new InvalidOperationException("No suitable conversion operator found for surrogate: " + this.forType.FullName + " / " + this.declaredType.FullName);
            }
            return info;
        }

        private static bool HasCast(Type type, Type from, Type to, out MethodInfo op)
        {
            int num2;
            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i = num2 + 1)
            {
                MethodInfo info = methods[i];
                if (((info.Name == "op_Implicit") || (info.Name == "op_Explicit")) && (info.ReturnType == to))
                {
                    ParameterInfo[] parameters = info.GetParameters();
                    if ((parameters.Length == 1) && (parameters[0].ParameterType == from))
                    {
                        op = info;
                        return true;
                    }
                }
                num2 = i;
            }
            op = null;
            return false;
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            Helpers.DebugAssert(valueFrom > null);
            using (Local local = new Local(ctx, this.declaredType))
            {
                ctx.LoadValue(valueFrom);
                ctx.EmitCall(this.toTail);
                ctx.StoreValue(local);
                this.rootTail.EmitRead(ctx, local);
                ctx.LoadValue(local);
                ctx.EmitCall(this.fromTail);
                ctx.StoreValue(valueFrom);
            }
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.LoadValue(valueFrom);
            ctx.EmitCall(this.toTail);
            this.rootTail.EmitWrite(ctx, null);
        }

        void IProtoTypeSerializer.Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
        {
        }

        bool IProtoTypeSerializer.CanCreateInstance()
        {
            return false;
        }

        object IProtoTypeSerializer.CreateInstance(ProtoReader source)
        {
            throw new NotSupportedException();
        }

        void IProtoTypeSerializer.EmitCallback(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
        {
        }

        void IProtoTypeSerializer.EmitCreateInstance(CompilerContext ctx)
        {
            throw new NotSupportedException();
        }

        bool IProtoTypeSerializer.HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return false;
        }

        public object Read(object value, ProtoReader source)
        {
            object[] parameters = new object[] { value };
            value = this.toTail.Invoke(null, parameters);
            parameters[0] = this.rootTail.Read(value, source);
            return this.fromTail.Invoke(null, parameters);
        }

        public void Write(object value, ProtoWriter writer)
        {
            object[] parameters = new object[] { value };
            this.rootTail.Write(this.toTail.Invoke(null, parameters), writer);
        }

        public Type ExpectedType
        {
            get
            {
                return this.forType;
            }
        }

        public bool RequiresOldValue
        {
            get
            {
                return true;
            }
        }

        public bool ReturnsValue
        {
            get
            {
                return false;
            }
        }
    }
}


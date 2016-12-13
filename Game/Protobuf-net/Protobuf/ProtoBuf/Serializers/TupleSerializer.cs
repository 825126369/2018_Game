namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;
    using System.Reflection;

    internal sealed class TupleSerializer : IProtoTypeSerializer, IProtoSerializer
    {
        private readonly ConstructorInfo ctor;
        private readonly MemberInfo[] members;
        private IProtoSerializer[] tails;

        public TupleSerializer(RuntimeTypeModel model, ConstructorInfo ctor, MemberInfo[] members)
        {
            int num3;
            if (ctor == null)
            {
                throw new ArgumentNullException("ctor");
            }
            if (members == null)
            {
                throw new ArgumentNullException("members");
            }
            this.ctor = ctor;
            this.members = members;
            this.tails = new IProtoSerializer[members.Length];
            ParameterInfo[] parameters = ctor.GetParameters();
            for (int i = 0; i < members.Length; i = num3 + 1)
            {
                WireType type;
                IProtoSerializer serializer2;
                Type parameterType = parameters[i].ParameterType;
                Type itemType = null;
                Type defaultType = null;
                MetaType.ResolveListTypes(model, parameterType, ref itemType, ref defaultType);
                Type type5 = (itemType == null) ? parameterType : itemType;
                bool asReference = false;
                if (model.FindOrAddAuto(type5, false, true, false) >= 0)
                {
                    asReference = model[type5].AsReferenceDefault;
                }
                IProtoSerializer tail = ValueMember.TryGetCoreSerializer(model, DataFormat.Default, type5, out type, asReference, false, false, true);
                if (tail == null)
                {
                    throw new InvalidOperationException("No serializer defined for type: " + type5.FullName);
                }
                tail = new TagDecorator(i + 1, type, false, tail);
                if (itemType == null)
                {
                    serializer2 = tail;
                }
                else if (parameterType.IsArray)
                {
                    serializer2 = new ArrayDecorator(model, tail, i + 1, false, type, parameterType, false, false);
                }
                else
                {
                    serializer2 = ListDecorator.Create(model, parameterType, defaultType, tail, i + 1, false, type, true, false, false);
                }
                this.tails[i] = serializer2;
                num3 = i;
            }
        }

        public void EmitCallback(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
        {
        }

        public void EmitRead(CompilerContext ctx, Local incoming)
        {
            using (Local local = ctx.GetLocalWithValue(this.ExpectedType, incoming))
            {
                int num2;
                Local[] localArray = new Local[this.members.Length];
                try
                {
                    for (int i = 0; i < localArray.Length; i = num2 + 1)
                    {
                        Type memberType = this.GetMemberType(i);
                        bool flag = true;
                        localArray[i] = new Local(ctx, memberType);
                        if (this.ExpectedType.IsValueType)
                        {
                            goto Label_0143;
                        }
                        if (memberType.IsValueType)
                        {
                            switch (Helpers.GetTypeCode(memberType))
                            {
                                case ProtoTypeCode.Boolean:
                                case ProtoTypeCode.SByte:
                                case ProtoTypeCode.Byte:
                                case ProtoTypeCode.Int16:
                                case ProtoTypeCode.UInt16:
                                case ProtoTypeCode.Int32:
                                case ProtoTypeCode.UInt32:
                                    ctx.LoadValue(0);
                                    goto Label_012E;

                                case ProtoTypeCode.Int64:
                                case ProtoTypeCode.UInt64:
                                    ctx.LoadValue((long) 0L);
                                    goto Label_012E;

                                case ProtoTypeCode.Single:
                                    ctx.LoadValue((float) 0f);
                                    goto Label_012E;

                                case ProtoTypeCode.Double:
                                    ctx.LoadValue((double) 0.0);
                                    goto Label_012E;

                                case ProtoTypeCode.Decimal:
                                    ctx.LoadValue(decimal.Zero);
                                    goto Label_012E;

                                case ProtoTypeCode.Guid:
                                    ctx.LoadValue(Guid.Empty);
                                    goto Label_012E;
                            }
                            ctx.LoadAddress(localArray[i], memberType);
                            ctx.EmitCtor(memberType);
                            flag = false;
                        }
                        else
                        {
                            ctx.LoadNullRef();
                        }
                    Label_012E:
                        if (flag)
                        {
                            ctx.StoreValue(localArray[i]);
                        }
                    Label_0143:
                        num2 = i;
                    }
                    CodeLabel label = this.ExpectedType.IsValueType ? new CodeLabel() : ctx.DefineLabel();
                    if (!this.ExpectedType.IsValueType)
                    {
                        ctx.LoadAddress(local, this.ExpectedType);
                        ctx.BranchIfFalse(label, false);
                    }
                    for (int j = 0; j < this.members.Length; j = num2 + 1)
                    {
                        ctx.LoadAddress(local, this.ExpectedType);
                        switch (this.members[j].MemberType)
                        {
                            case MemberTypes.Field:
                                ctx.LoadValue((FieldInfo) this.members[j]);
                                break;

                            case MemberTypes.Property:
                                ctx.LoadValue((PropertyInfo) this.members[j]);
                                break;
                        }
                        ctx.StoreValue(localArray[j]);
                        num2 = j;
                    }
                    if (!this.ExpectedType.IsValueType)
                    {
                        ctx.MarkLabel(label);
                    }
                    using (Local local2 = new Local(ctx, ctx.MapType(typeof(int))))
                    {
                        CodeLabel label3 = ctx.DefineLabel();
                        CodeLabel label4 = ctx.DefineLabel();
                        CodeLabel label5 = ctx.DefineLabel();
                        ctx.Branch(label3, false);
                        CodeLabel[] jumpTable = new CodeLabel[this.members.Length];
                        for (int m = 0; m < this.members.Length; m = num2 + 1)
                        {
                            jumpTable[m] = ctx.DefineLabel();
                            num2 = m;
                        }
                        ctx.MarkLabel(label4);
                        ctx.LoadValue(local2);
                        ctx.LoadValue(1);
                        ctx.Subtract();
                        ctx.Switch(jumpTable);
                        ctx.Branch(label5, false);
                        for (int n = 0; n < jumpTable.Length; n = num2 + 1)
                        {
                            ctx.MarkLabel(jumpTable[n]);
                            IProtoSerializer tail = this.tails[n];
                            Local valueFrom = tail.RequiresOldValue ? localArray[n] : null;
                            ctx.ReadNullCheckedTail(localArray[n].Type, tail, valueFrom);
                            if (tail.ReturnsValue)
                            {
                                if (localArray[n].Type.IsValueType)
                                {
                                    ctx.StoreValue(localArray[n]);
                                }
                                else
                                {
                                    CodeLabel label6 = ctx.DefineLabel();
                                    CodeLabel label7 = ctx.DefineLabel();
                                    ctx.CopyValue();
                                    ctx.BranchIfTrue(label6, true);
                                    ctx.DiscardValue();
                                    ctx.Branch(label7, true);
                                    ctx.MarkLabel(label6);
                                    ctx.StoreValue(localArray[n]);
                                    ctx.MarkLabel(label7);
                                }
                            }
                            ctx.Branch(label3, false);
                            num2 = n;
                        }
                        ctx.MarkLabel(label5);
                        ctx.LoadReaderWriter();
                        ctx.EmitCall(ctx.MapType(typeof(ProtoReader)).GetMethod("SkipField"));
                        ctx.MarkLabel(label3);
                        ctx.EmitBasicRead("ReadFieldHeader", ctx.MapType(typeof(int)));
                        ctx.CopyValue();
                        ctx.StoreValue(local2);
                        ctx.LoadValue(0);
                        ctx.BranchIfGreater(label4, false);
                    }
                    for (int k = 0; k < localArray.Length; k = num2 + 1)
                    {
                        ctx.LoadValue(localArray[k]);
                        num2 = k;
                    }
                    ctx.EmitCtor(this.ctor);
                    ctx.StoreValue(local);
                }
                finally
                {
                    for (int num7 = 0; num7 < localArray.Length; num7 = num2 + 1)
                    {
                        if (localArray[num7] > null)
                        {
                            localArray[num7].Dispose();
                        }
                        num2 = num7;
                    }
                }
            }
        }

        public void EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            using (Local local = ctx.GetLocalWithValue(this.ctor.DeclaringType, valueFrom))
            {
                int num2;
                for (int i = 0; i < this.tails.Length; i = num2 + 1)
                {
                    Type memberType = this.GetMemberType(i);
                    ctx.LoadAddress(local, this.ExpectedType);
                    switch (this.members[i].MemberType)
                    {
                        case MemberTypes.Field:
                            ctx.LoadValue((FieldInfo) this.members[i]);
                            break;

                        case MemberTypes.Property:
                            ctx.LoadValue((PropertyInfo) this.members[i]);
                            break;
                    }
                    ctx.WriteNullCheckedTail(memberType, this.tails[i], null);
                    num2 = i;
                }
            }
        }

        private Type GetMemberType(int index)
        {
            Type memberType = Helpers.GetMemberType(this.members[index]);
            if (memberType == null)
            {
                throw new InvalidOperationException();
            }
            return memberType;
        }

        private object GetValue(object obj, int index)
        {
            PropertyInfo info = this.members[index] as PropertyInfo;
            if (info > null)
            {
                if (obj == null)
                {
                    return (Helpers.IsValueType(info.PropertyType) ? Activator.CreateInstance(info.PropertyType) : null);
                }
                return info.GetValue(obj, null);
            }
            FieldInfo info2 = this.members[index] as FieldInfo;
            if (info2 <= null)
            {
                throw new InvalidOperationException();
            }
            if (obj == null)
            {
                return (Helpers.IsValueType(info2.FieldType) ? Activator.CreateInstance(info2.FieldType) : null);
            }
            return info2.GetValue(obj);
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return false;
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

        void IProtoTypeSerializer.EmitCreateInstance(CompilerContext ctx)
        {
            throw new NotSupportedException();
        }

        public object Read(object value, ProtoReader source)
        {
            int num;
            int num3;
            object[] parameters = new object[this.members.Length];
            bool flag = false;
            if (value == null)
            {
                flag = true;
            }
            for (int i = 0; i < parameters.Length; i = num3 + 1)
            {
                parameters[i] = this.GetValue(value, i);
                num3 = i;
            }
            while ((num = source.ReadFieldHeader()) > 0)
            {
                flag = true;
                if (num <= this.tails.Length)
                {
                    IProtoSerializer serializer = this.tails[num - 1];
                    parameters[num - 1] = this.tails[num - 1].Read(serializer.RequiresOldValue ? parameters[num - 1] : null, source);
                }
                else
                {
                    source.SkipField();
                }
            }
            return (flag ? this.ctor.Invoke(parameters) : value);
        }

        public void Write(object value, ProtoWriter dest)
        {
            int num2;
            for (int i = 0; i < this.tails.Length; i = num2 + 1)
            {
                object obj2 = this.GetValue(value, i);
                if (obj2 > null)
                {
                    this.tails[i].Write(obj2, dest);
                }
                num2 = i;
            }
        }

        public Type ExpectedType
        {
            get
            {
                return this.ctor.DeclaringType;
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


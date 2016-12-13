namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class ImmutableCollectionDecorator : ListDecorator
    {
        private readonly MethodInfo add;
        private readonly MethodInfo addRange;
        private readonly MethodInfo builderFactory;
        private readonly MethodInfo finish;

        internal ImmutableCollectionDecorator(TypeModel model, Type declaredType, Type concreteType, IProtoSerializer tail, int fieldNumber, bool writePacked, WireType packedWireType, bool returnList, bool overwriteList, bool supportNull, MethodInfo builderFactory, MethodInfo add, MethodInfo addRange, MethodInfo finish) : base(model, declaredType, concreteType, tail, fieldNumber, writePacked, packedWireType, returnList, overwriteList, supportNull)
        {
            this.builderFactory = builderFactory;
            this.add = add;
            this.addRange = addRange;
            this.finish = finish;
        }

        protected override void EmitRead(CompilerContext ctx, Local valueFrom)
        {
            using (Local local = base.AppendToCollection ? ctx.GetLocalWithValue(this.ExpectedType, valueFrom) : null)
            {
                using (Local local2 = new Local(ctx, this.builderFactory.ReturnType))
                {
                    ctx.EmitCall(this.builderFactory);
                    ctx.StoreValue(local2);
                    if (base.AppendToCollection)
                    {
                        CodeLabel label = ctx.DefineLabel();
                        if (!this.ExpectedType.IsValueType)
                        {
                            ctx.LoadValue(local);
                            ctx.BranchIfFalse(label, false);
                        }
                        PropertyInfo property = Helpers.GetProperty(this.ExpectedType, "Length", false);
                        if (property == null)
                        {
                            property = Helpers.GetProperty(this.ExpectedType, "Count", false);
                        }
                        if (property == null)
                        {
                            property = Helpers.GetProperty(ResolveIReadOnlyCollection(this.ExpectedType, base.Tail.ExpectedType), "Count", false);
                        }
                        ctx.LoadAddress(local, local.Type);
                        ctx.EmitCall(Helpers.GetGetMethod(property, false, false));
                        ctx.BranchIfFalse(label, false);
                        Type type = ctx.MapType(typeof(void));
                        if (this.addRange > null)
                        {
                            ctx.LoadValue(local2);
                            ctx.LoadValue(local);
                            ctx.EmitCall(this.addRange);
                            if ((this.addRange.ReturnType != null) && (this.add.ReturnType != type))
                            {
                                ctx.DiscardValue();
                            }
                        }
                        else
                        {
                            MethodInfo info2;
                            MethodInfo info3;
                            MethodInfo method = base.GetEnumeratorInfo(ctx.Model, out info2, out info3);
                            Helpers.DebugAssert(info2 > null);
                            Helpers.DebugAssert(info3 > null);
                            Helpers.DebugAssert(method > null);
                            Type returnType = method.ReturnType;
                            using (Local local3 = new Local(ctx, returnType))
                            {
                                ctx.LoadAddress(local, this.ExpectedType);
                                ctx.EmitCall(method);
                                ctx.StoreValue(local3);
                                using (ctx.Using(local3))
                                {
                                    CodeLabel label2 = ctx.DefineLabel();
                                    CodeLabel label3 = ctx.DefineLabel();
                                    ctx.Branch(label3, false);
                                    ctx.MarkLabel(label2);
                                    ctx.LoadAddress(local2, local2.Type);
                                    ctx.LoadAddress(local3, returnType);
                                    ctx.EmitCall(info3);
                                    ctx.EmitCall(this.add);
                                    if ((this.add.ReturnType != null) && (this.add.ReturnType != type))
                                    {
                                        ctx.DiscardValue();
                                    }
                                    ctx.MarkLabel(label3);
                                    ctx.LoadAddress(local3, returnType);
                                    ctx.EmitCall(info2);
                                    ctx.BranchIfTrue(label2, false);
                                }
                            }
                        }
                        ctx.MarkLabel(label);
                    }
                    ListDecorator.EmitReadList(ctx, local2, base.Tail, this.add, base.packedWireType, false);
                    ctx.LoadAddress(local2, local2.Type);
                    ctx.EmitCall(this.finish);
                    if (this.ExpectedType != this.finish.ReturnType)
                    {
                        ctx.Cast(this.ExpectedType);
                    }
                }
            }
        }

        internal static bool IdentifyImmutable(TypeModel model, Type declaredType, out MethodInfo builderFactory, out MethodInfo add, out MethodInfo addRange, out MethodInfo finish)
        {
            Type type;
            Type[] genericArguments;
            Type[] typeArray2;
            MethodInfo info;
            finish = (MethodInfo) (info = null);
            addRange = info = info;
            builderFactory = add = info;
            if ((model != null) && (declaredType != null))
            {
                type = declaredType;
                if (!type.IsGenericType)
                {
                    return false;
                }
                genericArguments = type.GetGenericArguments();
                switch (genericArguments.Length)
                {
                    case 1:
                        typeArray2 = genericArguments;
                        goto Label_00AF;

                    case 2:
                    {
                        Type type4 = model.MapType(typeof(KeyValuePair<,>));
                        if (type4 == null)
                        {
                            return false;
                        }
                        type4 = type4.MakeGenericType(genericArguments);
                        typeArray2 = new Type[] { type4 };
                        goto Label_00AF;
                    }
                }
            }
            return false;
        Label_00AF:
            if (ResolveIReadOnlyCollection(declaredType, null) == null)
            {
                return false;
            }
            string name = declaredType.Name;
            int index = name.IndexOf('`');
            if (index <= 0)
            {
                return false;
            }
            name = type.IsInterface ? name.Substring(1, index - 1) : name.Substring(0, index);
            Type type2 = model.GetType(declaredType.Namespace + "." + name, type.Assembly);
            if ((type2 == null) && (name == "ImmutableSet"))
            {
                type2 = model.GetType(declaredType.Namespace + ".ImmutableHashSet", type.Assembly);
            }
            if (type2 == null)
            {
                return false;
            }
            foreach (MethodInfo info2 in type2.GetMethods())
            {
                if (((info2.IsStatic && (info2.Name == "CreateBuilder")) && (info2.IsGenericMethodDefinition && (info2.GetParameters().Length == 0))) && (info2.GetGenericArguments().Length == genericArguments.Length))
                {
                    builderFactory = info2.MakeGenericMethod(genericArguments);
                    break;
                }
            }
            Type type3 = model.MapType(typeof(void));
            if (((builderFactory == null) || (builderFactory.ReturnType == null)) || (builderFactory.ReturnType == type3))
            {
                return false;
            }
            add = Helpers.GetInstanceMethod(builderFactory.ReturnType, "Add", typeArray2);
            if (add == null)
            {
                return false;
            }
            finish = Helpers.GetInstanceMethod(builderFactory.ReturnType, "ToImmutable", Helpers.EmptyTypes);
            if (((finish == null) || (finish.ReturnType == null)) || (finish.ReturnType == type3))
            {
                return false;
            }
            if ((finish.ReturnType != declaredType) && !Helpers.IsAssignableFrom(declaredType, finish.ReturnType))
            {
                return false;
            }
            Type[] types = new Type[] { declaredType };
            addRange = Helpers.GetInstanceMethod(builderFactory.ReturnType, "AddRange", types);
            if (addRange == null)
            {
                Type type5 = model.MapType(typeof(IEnumerable<>), false);
                if (type5 > null)
                {
                    Type[] typeArray4 = new Type[] { type5.MakeGenericType(typeArray2) };
                    addRange = Helpers.GetInstanceMethod(builderFactory.ReturnType, "AddRange", typeArray4);
                }
            }
            return true;
        }

        public override object Read(object value, ProtoReader source)
        {
            object obj2 = this.builderFactory.Invoke(null, null);
            int fieldNumber = source.FieldNumber;
            object[] parameters = new object[1];
            if ((base.AppendToCollection && (value != null)) && (((IList) value).Count > 0))
            {
                if (this.addRange > null)
                {
                    parameters[0] = value;
                    this.addRange.Invoke(obj2, parameters);
                }
                else
                {
                    foreach (object obj3 in (IList) value)
                    {
                        parameters[0] = obj3;
                        this.add.Invoke(obj2, parameters);
                    }
                }
            }
            if ((base.packedWireType != WireType.None) && (source.WireType == WireType.String))
            {
                SubItemToken token = ProtoReader.StartSubItem(source);
                while (ProtoReader.HasSubValue(base.packedWireType, source))
                {
                    parameters[0] = base.Tail.Read(null, source);
                    this.add.Invoke(obj2, parameters);
                }
                ProtoReader.EndSubItem(token, source);
            }
            else
            {
                do
                {
                    parameters[0] = base.Tail.Read(null, source);
                    this.add.Invoke(obj2, parameters);
                }
                while (source.TryReadFieldHeader(fieldNumber));
            }
            return this.finish.Invoke(obj2, null);
        }

        private static Type ResolveIReadOnlyCollection(Type declaredType, Type t)
        {
            foreach (Type type in declaredType.GetInterfaces())
            {
                if (type.IsGenericType && type.Name.StartsWith("IReadOnlyCollection`"))
                {
                    if (t > null)
                    {
                        Type[] genericArguments = type.GetGenericArguments();
                        if ((genericArguments.Length != 1) && (genericArguments[0] != t))
                        {
                            continue;
                        }
                    }
                    return type;
                }
            }
            return null;
        }

        protected override bool RequireAdd
        {
            get
            {
                return false;
            }
        }
    }
}


namespace ProtoBuf.Meta
{
    using ProtoBuf;
    using ProtoBuf.Serializers;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public class MetaType : ISerializerProxy
    {
        private MetaType baseType;
        private CallbackSet callbacks;
        private System.Type constructType;
        private MethodInfo factory;
        private readonly BasicList fields = new BasicList();
        private volatile byte flags;
        internal static readonly System.Type ienumerable = typeof(IEnumerable);
        private readonly RuntimeTypeModel model;
        private string name;
        private const byte OPTIONS_AsReferenceDefault = 0x20;
        private const byte OPTIONS_AutoTuple = 0x40;
        private const byte OPTIONS_EnumPassThru = 2;
        private const byte OPTIONS_Frozen = 4;
        private const byte OPTIONS_IgnoreListHandling = 0x80;
        private const byte OPTIONS_Pending = 1;
        private const byte OPTIONS_PrivateOnApi = 8;
        private const byte OPTIONS_SkipConstructor = 0x10;
        private IProtoTypeSerializer serializer;
        private BasicList subTypes;
        private System.Type surrogate;
        private readonly System.Type type;

        internal MetaType(RuntimeTypeModel model, System.Type type, MethodInfo factory)
        {
            this.factory = factory;
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (model.TryGetBasicTypeSerializer(type) > null)
            {
                throw InbuiltType(type);
            }
            this.type = type;
            this.model = model;
            if (Helpers.IsEnum(type))
            {
                this.EnumPassthru = type.IsDefined(model.MapType(typeof(FlagsAttribute)), false);
            }
        }

        public MetaType Add(params string[] memberNames)
        {
            int num3;
            if (memberNames == null)
            {
                throw new ArgumentNullException("memberNames");
            }
            int nextFieldNumber = this.GetNextFieldNumber();
            for (int i = 0; i < memberNames.Length; i = num3 + 1)
            {
                num3 = nextFieldNumber;
                nextFieldNumber = num3 + 1;
                this.Add(num3, memberNames[i]);
                num3 = i;
            }
            return this;
        }

        private void Add(ValueMember member)
        {
            int opaqueToken = 0;
            try
            {
                this.model.TakeLock(ref opaqueToken);
                this.ThrowIfFrozen();
                this.fields.Add(member);
            }
            finally
            {
                this.model.ReleaseLock(opaqueToken);
            }
        }

        public MetaType Add(string memberName)
        {
            this.Add(this.GetNextFieldNumber(), memberName);
            return this;
        }

        public MetaType Add(int fieldNumber, string memberName)
        {
            this.AddField(fieldNumber, memberName, null, null, null);
            return this;
        }

        public MetaType Add(int fieldNumber, string memberName, object defaultValue)
        {
            this.AddField(fieldNumber, memberName, null, null, defaultValue);
            return this;
        }

        public MetaType Add(int fieldNumber, string memberName, System.Type itemType, System.Type defaultType)
        {
            this.AddField(fieldNumber, memberName, itemType, defaultType, null);
            return this;
        }

        public ValueMember AddField(int fieldNumber, string memberName)
        {
            return this.AddField(fieldNumber, memberName, null, null, null);
        }

        public ValueMember AddField(int fieldNumber, string memberName, System.Type itemType, System.Type defaultType)
        {
            return this.AddField(fieldNumber, memberName, itemType, defaultType, null);
        }

        private ValueMember AddField(int fieldNumber, string memberName, System.Type itemType, System.Type defaultType, object defaultValue)
        {
            MemberInfo info = null;
            System.Type fieldType;
            MemberInfo[] infoArray = this.type.GetMember(memberName, Helpers.IsEnum(this.type) ? (BindingFlags.Public | BindingFlags.Static) : (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
            if ((infoArray != null) && (infoArray.Length == 1))
            {
                info = infoArray[0];
            }
            if (info == null)
            {
                throw new ArgumentException("Unable to determine member: " + memberName, "memberName");
            }
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    fieldType = ((FieldInfo) info).FieldType;
                    break;

                case MemberTypes.Property:
                    fieldType = ((PropertyInfo) info).PropertyType;
                    break;

                default:
                    throw new NotSupportedException(info.MemberType.ToString());
            }
            ResolveListTypes(this.model, fieldType, ref itemType, ref defaultType);
            ValueMember member = new ValueMember(this.model, this.type, fieldNumber, info, fieldType, itemType, defaultType, DataFormat.Default, defaultValue);
            this.Add(member);
            return member;
        }

        public MetaType AddSubType(int fieldNumber, System.Type derivedType)
        {
            return this.AddSubType(fieldNumber, derivedType, DataFormat.Default);
        }

        public MetaType AddSubType(int fieldNumber, System.Type derivedType, DataFormat dataFormat)
        {
            if (derivedType == null)
            {
                throw new ArgumentNullException("derivedType");
            }
            if (fieldNumber < 1)
            {
                throw new ArgumentOutOfRangeException("fieldNumber");
            }
            if ((!this.type.IsClass && !this.type.IsInterface) || this.type.IsSealed)
            {
                throw new InvalidOperationException("Sub-types can only be added to non-sealed classes");
            }
            if (!this.IsValidSubType(derivedType))
            {
                throw new ArgumentException(derivedType.Name + " is not a valid sub-type of " + this.type.Name, "derivedType");
            }
            MetaType type = this.model[derivedType];
            this.ThrowIfFrozen();
            type.ThrowIfFrozen();
            SubType type2 = new SubType(fieldNumber, type, dataFormat);
            this.ThrowIfFrozen();
            type.SetBaseType(this);
            if (this.subTypes == null)
            {
                this.subTypes = new BasicList();
            }
            this.subTypes.Add(type2);
            return this;
        }

        internal void ApplyDefaultBehaviour()
        {
            System.Type baseType = GetBaseType(this);
            if (((baseType != null) && (this.model.FindWithoutAdd(baseType) == null)) && (GetContractFamily(this.model, baseType, null) > AttributeFamily.None))
            {
                this.model.FindOrAddAuto(baseType, true, false, false);
            }
            AttributeMap[] attributes = AttributeMap.Create(this.model, this.type, false);
            AttributeFamily family = GetContractFamily(this.model, this.type, attributes);
            if (family == AttributeFamily.AutoTuple)
            {
                this.SetFlag(0x40, true, true);
            }
            bool isEnum = !this.EnumPassthru && Helpers.IsEnum(this.type);
            if ((family != AttributeFamily.None) || isEnum)
            {
                int num5;
                BasicList list = null;
                BasicList partialMembers = null;
                int dataMemberOffset = 0;
                int num2 = 1;
                bool inferTagFromNameDefault = this.model.InferTagFromNameDefault;
                ImplicitFields none = ImplicitFields.None;
                string str = null;
                for (int i = 0; i < attributes.Length; i = num5 + 1)
                {
                    object obj2;
                    AttributeMap map = attributes[i];
                    string fullName = map.AttributeType.FullName;
                    if (!isEnum && (fullName == "ProtoBuf.ProtoIncludeAttribute"))
                    {
                        int fieldNumber = 0;
                        if (map.TryGet("tag", out obj2))
                        {
                            fieldNumber = (int) obj2;
                        }
                        DataFormat dataFormat = DataFormat.Default;
                        if (map.TryGet("DataFormat", out obj2))
                        {
                            dataFormat = (DataFormat) ((int) obj2);
                        }
                        System.Type subType = null;
                        try
                        {
                            if (map.TryGet("knownTypeName", out obj2))
                            {
                                subType = this.model.GetType((string) obj2, this.type.Assembly);
                            }
                            else if (map.TryGet("knownType", out obj2))
                            {
                                subType = (System.Type) obj2;
                            }
                        }
                        catch (Exception exception)
                        {
                            throw new InvalidOperationException("Unable to resolve sub-type of: " + this.type.FullName, exception);
                        }
                        if (subType == null)
                        {
                            throw new InvalidOperationException("Unable to resolve sub-type of: " + this.type.FullName);
                        }
                        if (this.IsValidSubType(subType))
                        {
                            this.AddSubType(fieldNumber, subType, dataFormat);
                        }
                    }
                    if ((fullName == "ProtoBuf.ProtoPartialIgnoreAttribute") && (map.TryGet("MemberName", out obj2) && (obj2 > null)))
                    {
                        if (list == null)
                        {
                            list = new BasicList();
                        }
                        list.Add((string) obj2);
                    }
                    if (!isEnum && (fullName == "ProtoBuf.ProtoPartialMemberAttribute"))
                    {
                        if (partialMembers == null)
                        {
                            partialMembers = new BasicList();
                        }
                        partialMembers.Add(map);
                    }
                    if (fullName == "ProtoBuf.ProtoContractAttribute")
                    {
                        if (map.TryGet("Name", out obj2))
                        {
                            str = (string) obj2;
                        }
                        if (Helpers.IsEnum(this.type))
                        {
                            if ((map.TryGet("EnumPassthruHasValue", false, out obj2) && ((bool) obj2)) && map.TryGet("EnumPassthru", out obj2))
                            {
                                this.EnumPassthru = (bool) obj2;
                                if (this.EnumPassthru)
                                {
                                    isEnum = false;
                                }
                            }
                        }
                        else
                        {
                            if (map.TryGet("DataMemberOffset", out obj2))
                            {
                                dataMemberOffset = (int) obj2;
                            }
                            if ((map.TryGet("InferTagFromNameHasValue", false, out obj2) && ((bool) obj2)) && map.TryGet("InferTagFromName", out obj2))
                            {
                                inferTagFromNameDefault = (bool) obj2;
                            }
                            if (map.TryGet("ImplicitFields", out obj2) && (obj2 > null))
                            {
                                none = (ImplicitFields) ((int) obj2);
                            }
                            if (map.TryGet("SkipConstructor", out obj2))
                            {
                                this.UseConstructor = !((bool) obj2);
                            }
                            if (map.TryGet("IgnoreListHandling", out obj2))
                            {
                                this.IgnoreListHandling = (bool) obj2;
                            }
                            if (map.TryGet("AsReferenceDefault", out obj2))
                            {
                                this.AsReferenceDefault = (bool) obj2;
                            }
                            if (map.TryGet("ImplicitFirstTag", out obj2) && (((int) obj2) > 0))
                            {
                                num2 = (int) obj2;
                            }
                        }
                    }
                    if ((fullName == "System.Runtime.Serialization.DataContractAttribute") && ((str == null) && map.TryGet("Name", out obj2)))
                    {
                        str = (string) obj2;
                    }
                    if ((fullName == "System.Xml.Serialization.XmlTypeAttribute") && ((str == null) && map.TryGet("TypeName", out obj2)))
                    {
                        str = (string) obj2;
                    }
                    num5 = i;
                }
                if (!Helpers.IsNullOrEmpty(str))
                {
                    this.Name = str;
                }
                if (none > ImplicitFields.None)
                {
                    family &= AttributeFamily.ProtoBuf;
                }
                MethodInfo[] callbacks = null;
                BasicList members = new BasicList();
                MemberInfo[] infoArray2 = this.type.GetMembers(isEnum ? (BindingFlags.Public | BindingFlags.Static) : (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
                foreach (MemberInfo info in infoArray2)
                {
                    if (((info.DeclaringType == this.type) && !info.IsDefined(this.model.MapType(typeof(ProtoIgnoreAttribute)), true)) && ((list == null) || !list.Contains(info.Name)))
                    {
                        bool isPublic;
                        bool flag41;
                        System.Type propertyType;
                        bool forced = false;
                        PropertyInfo property = info as PropertyInfo;
                        if (property > null)
                        {
                            if (!isEnum)
                            {
                                propertyType = property.PropertyType;
                                isPublic = Helpers.GetGetMethod(property, false, false) > null;
                                flag41 = false;
                                ApplyDefaultBehaviour_AddMembers(this.model, family, isEnum, partialMembers, dataMemberOffset, inferTagFromNameDefault, none, members, info, ref forced, isPublic, flag41, ref propertyType);
                            }
                        }
                        else
                        {
                            FieldInfo info3 = info as FieldInfo;
                            if (info3 > null)
                            {
                                propertyType = info3.FieldType;
                                isPublic = info3.IsPublic;
                                flag41 = true;
                                if (!isEnum || info3.IsStatic)
                                {
                                    ApplyDefaultBehaviour_AddMembers(this.model, family, isEnum, partialMembers, dataMemberOffset, inferTagFromNameDefault, none, members, info, ref forced, isPublic, flag41, ref propertyType);
                                }
                            }
                            else
                            {
                                MethodInfo info4;
                                if (((info4 = info as MethodInfo) > null) && !isEnum)
                                {
                                    AttributeMap[] mapArray2 = AttributeMap.Create(this.model, info4, false);
                                    if ((mapArray2 != null) && (mapArray2.Length > 0))
                                    {
                                        CheckForCallback(info4, mapArray2, "ProtoBuf.ProtoBeforeSerializationAttribute", ref callbacks, 0);
                                        CheckForCallback(info4, mapArray2, "ProtoBuf.ProtoAfterSerializationAttribute", ref callbacks, 1);
                                        CheckForCallback(info4, mapArray2, "ProtoBuf.ProtoBeforeDeserializationAttribute", ref callbacks, 2);
                                        CheckForCallback(info4, mapArray2, "ProtoBuf.ProtoAfterDeserializationAttribute", ref callbacks, 3);
                                        CheckForCallback(info4, mapArray2, "System.Runtime.Serialization.OnSerializingAttribute", ref callbacks, 4);
                                        CheckForCallback(info4, mapArray2, "System.Runtime.Serialization.OnSerializedAttribute", ref callbacks, 5);
                                        CheckForCallback(info4, mapArray2, "System.Runtime.Serialization.OnDeserializingAttribute", ref callbacks, 6);
                                        CheckForCallback(info4, mapArray2, "System.Runtime.Serialization.OnDeserializedAttribute", ref callbacks, 7);
                                    }
                                }
                            }
                        }
                    }
                }
                ProtoMemberAttribute[] array = new ProtoMemberAttribute[members.Count];
                members.CopyTo(array, 0);
                if (inferTagFromNameDefault || (none > ImplicitFields.None))
                {
                    Array.Sort<ProtoMemberAttribute>(array);
                    int num7 = num2;
                    foreach (ProtoMemberAttribute attribute in array)
                    {
                        if (!attribute.TagIsPinned)
                        {
                            num5 = num7;
                            num7 = num5 + 1;
                            attribute.Rebase(num5);
                        }
                    }
                }
                foreach (ProtoMemberAttribute attribute2 in array)
                {
                    ValueMember member = this.ApplyDefaultBehaviour(isEnum, attribute2);
                    if (member > null)
                    {
                        this.Add(member);
                    }
                }
                if (callbacks > null)
                {
                    this.SetCallbacks(Coalesce(callbacks, 0, 4), Coalesce(callbacks, 1, 5), Coalesce(callbacks, 2, 6), Coalesce(callbacks, 3, 7));
                }
            }
        }

        private ValueMember ApplyDefaultBehaviour(bool isEnum, ProtoMemberAttribute normalizedAttribute)
        {
            MemberInfo info;
            AttributeMap map;
            object obj3;
            if ((normalizedAttribute == null) || ((info = normalizedAttribute.Member) == null))
            {
                return null;
            }
            System.Type memberType = Helpers.GetMemberType(info);
            System.Type itemType = null;
            System.Type defaultType = null;
            ResolveListTypes(this.model, memberType, ref itemType, ref defaultType);
            if ((itemType > null) && ((this.model.FindOrAddAuto(memberType, false, true, false) >= 0) && this.model[memberType].IgnoreListHandling))
            {
                itemType = null;
                defaultType = null;
            }
            AttributeMap[] attribs = AttributeMap.Create(this.model, info, true);
            object defaultValue = null;
            if (this.model.UseImplicitZeroDefaults)
            {
                switch (Helpers.GetTypeCode(memberType))
                {
                    case ProtoTypeCode.Boolean:
                        defaultValue = false;
                        break;

                    case ProtoTypeCode.Char:
                        defaultValue = '\0';
                        break;

                    case ProtoTypeCode.SByte:
                        defaultValue = (sbyte) 0;
                        break;

                    case ProtoTypeCode.Byte:
                        defaultValue = (byte) 0;
                        break;

                    case ProtoTypeCode.Int16:
                        defaultValue = (short) 0;
                        break;

                    case ProtoTypeCode.UInt16:
                        defaultValue = (ushort) 0;
                        break;

                    case ProtoTypeCode.Int32:
                        defaultValue = 0;
                        break;

                    case ProtoTypeCode.UInt32:
                        defaultValue = 0;
                        break;

                    case ProtoTypeCode.Int64:
                        defaultValue = 0L;
                        break;

                    case ProtoTypeCode.UInt64:
                        defaultValue = (ulong) 0L;
                        break;

                    case ProtoTypeCode.Single:
                        defaultValue = 0f;
                        break;

                    case ProtoTypeCode.Double:
                        defaultValue = 0.0;
                        break;

                    case ProtoTypeCode.Decimal:
                        defaultValue = decimal.Zero;
                        break;

                    case ProtoTypeCode.TimeSpan:
                        defaultValue = TimeSpan.Zero;
                        break;

                    case ProtoTypeCode.Guid:
                        defaultValue = Guid.Empty;
                        break;
                }
            }
            if (((map = GetAttribute(attribs, "System.ComponentModel.DefaultValueAttribute")) > null) && map.TryGet("Value", out obj3))
            {
                defaultValue = obj3;
            }
            ValueMember member = (isEnum || (normalizedAttribute.Tag > 0)) ? new ValueMember(this.model, this.type, normalizedAttribute.Tag, info, memberType, itemType, defaultType, normalizedAttribute.DataFormat, defaultValue) : null;
            if (member > null)
            {
                System.Type type = this.type;
                PropertyInfo property = Helpers.GetProperty(type, info.Name + "Specified", true);
                MethodInfo getSpecified = Helpers.GetGetMethod(property, true, true);
                if ((getSpecified == null) || getSpecified.IsStatic)
                {
                    property = null;
                }
                if (property > null)
                {
                    member.SetSpecified(getSpecified, Helpers.GetSetMethod(property, true, true));
                }
                else
                {
                    MethodInfo info4 = Helpers.GetInstanceMethod(type, "ShouldSerialize" + info.Name, Helpers.EmptyTypes);
                    if ((info4 != null) && (info4.ReturnType == this.model.MapType(typeof(bool))))
                    {
                        member.SetSpecified(info4, null);
                    }
                }
                if (!Helpers.IsNullOrEmpty(normalizedAttribute.Name))
                {
                    member.SetName(normalizedAttribute.Name);
                }
                member.IsPacked = normalizedAttribute.IsPacked;
                member.IsRequired = normalizedAttribute.IsRequired;
                member.OverwriteList = normalizedAttribute.OverwriteList;
                if (normalizedAttribute.AsReferenceHasValue)
                {
                    member.AsReference = normalizedAttribute.AsReference;
                }
                member.DynamicType = normalizedAttribute.DynamicType;
            }
            return member;
        }

        private static void ApplyDefaultBehaviour_AddMembers(TypeModel model, AttributeFamily family, bool isEnum, BasicList partialMembers, int dataMemberOffset, bool inferTagByName, ImplicitFields implicitMode, BasicList members, MemberInfo member, ref bool forced, bool isPublic, bool isField, ref System.Type effectiveType)
        {
            ImplicitFields fields = implicitMode;
            if (fields != ImplicitFields.AllPublic)
            {
                if ((fields == ImplicitFields.AllFields) && isField)
                {
                    forced = true;
                }
            }
            else if (isPublic)
            {
                forced = true;
            }
            if (effectiveType.IsSubclassOf(model.MapType(typeof(Delegate))))
            {
                effectiveType = null;
            }
            if (effectiveType > null)
            {
                ProtoMemberAttribute attribute = NormalizeProtoMember(model, member, family, forced, isEnum, partialMembers, dataMemberOffset, inferTagByName);
                if (attribute > null)
                {
                    members.Add(attribute);
                }
            }
        }

        private IProtoTypeSerializer BuildSerializer()
        {
            int num4;
            if (Helpers.IsEnum(this.type))
            {
                return new TagDecorator(1, WireType.Variant, false, new EnumSerializer(this.type, this.GetEnumMap()));
            }
            System.Type itemType = this.IgnoreListHandling ? null : TypeModel.GetListItemType(this.model, this.type);
            if (itemType > null)
            {
                if (this.surrogate > null)
                {
                    throw new ArgumentException("Repeated data (a list, collection, etc) has inbuilt behaviour and cannot use a surrogate");
                }
                if ((this.subTypes != null) && (this.subTypes.Count > 0))
                {
                    throw new ArgumentException("Repeated data (a list, collection, etc) has inbuilt behaviour and cannot be subclassed");
                }
                System.Type defaultType = null;
                ResolveListTypes(this.model, this.type, ref itemType, ref defaultType);
                ValueMember member = new ValueMember(this.model, 1, this.type, itemType, defaultType, DataFormat.Default);
                int[] numArray1 = new int[] { 1 };
                return new TypeSerializer(this.model, this.type, numArray1, new IProtoSerializer[] { member.Serializer }, null, true, true, null, this.constructType, this.factory);
            }
            if (this.surrogate > null)
            {
                MetaType type5;
                MetaType type4 = this.model[this.surrogate];
                while ((type5 = type4.baseType) > null)
                {
                    type4 = type5;
                }
                return new SurrogateSerializer(this.type, this.surrogate, type4.Serializer);
            }
            if (this.IsAutoTuple)
            {
                MemberInfo[] infoArray2;
                ConstructorInfo ctor = ResolveTupleConstructor(this.type, out infoArray2);
                if (ctor == null)
                {
                    throw new InvalidOperationException();
                }
                return new TupleSerializer(this.model, ctor, infoArray2);
            }
            this.fields.Trim();
            int count = this.fields.Count;
            int num2 = (this.subTypes == null) ? 0 : this.subTypes.Count;
            int[] fieldNumbers = new int[count + num2];
            IProtoSerializer[] serializers = new IProtoSerializer[count + num2];
            int index = 0;
            if (num2 > 0)
            {
                BasicList.NodeEnumerator enumerator = this.subTypes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    SubType current = (SubType) enumerator.Current;
                    if (!current.DerivedType.IgnoreListHandling && this.model.MapType(ienumerable).IsAssignableFrom(current.DerivedType.Type))
                    {
                        throw new ArgumentException("Repeated data (a list, collection, etc) has inbuilt behaviour and cannot be used as a subclass");
                    }
                    fieldNumbers[index] = current.FieldNumber;
                    num4 = index;
                    index = num4 + 1;
                    serializers[num4] = current.Serializer;
                }
            }
            if (count > 0)
            {
                BasicList.NodeEnumerator enumerator2 = this.fields.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    ValueMember member2 = (ValueMember) enumerator2.Current;
                    fieldNumbers[index] = member2.FieldNumber;
                    num4 = index;
                    index = num4 + 1;
                    serializers[num4] = member2.Serializer;
                }
            }
            BasicList list = null;
            for (MetaType type2 = this.BaseType; type2 > null; type2 = type2.BaseType)
            {
                MethodInfo info2 = type2.HasCallbacks ? type2.Callbacks.BeforeDeserialize : null;
                if (info2 > null)
                {
                    if (list == null)
                    {
                        list = new BasicList();
                    }
                    list.Add(info2);
                }
            }
            MethodInfo[] array = null;
            if (list > null)
            {
                array = new MethodInfo[list.Count];
                list.CopyTo(array, 0);
                Array.Reverse(array);
            }
            return new TypeSerializer(this.model, this.type, fieldNumbers, serializers, array, this.baseType == null, this.UseConstructor, this.callbacks, this.constructType, this.factory);
        }

        private static void CheckForCallback(MethodInfo method, AttributeMap[] attributes, string callbackTypeName, ref MethodInfo[] callbacks, int index)
        {
            int num2;
            for (int i = 0; i < attributes.Length; i = num2 + 1)
            {
                if (attributes[i].AttributeType.FullName == callbackTypeName)
                {
                    if (callbacks == null)
                    {
                        callbacks = new MethodInfo[8];
                    }
                    else if (callbacks[index] > null)
                    {
                        System.Type reflectedType = method.ReflectedType;
                        throw new ProtoException("Duplicate " + callbackTypeName + " callbacks on " + reflectedType.FullName);
                    }
                    callbacks[index] = method;
                }
                num2 = i;
            }
        }

        private static MethodInfo Coalesce(MethodInfo[] arr, int x, int y)
        {
            MethodInfo info = arr[x];
            if (info == null)
            {
                info = arr[y];
            }
            return info;
        }

        public void CompileInPlace()
        {
            this.serializer = CompiledSerializer.Wrap(this.Serializer, this.model);
        }

        internal static bool GetAsReferenceDefault(RuntimeTypeModel model, System.Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!Helpers.IsEnum(type))
            {
                int num2;
                AttributeMap[] mapArray = AttributeMap.Create(model, type, false);
                for (int i = 0; i < mapArray.Length; i = num2 + 1)
                {
                    object obj2;
                    if ((mapArray[i].AttributeType.FullName == "ProtoBuf.ProtoContractAttribute") && mapArray[i].TryGet("AsReferenceDefault", out obj2))
                    {
                        return (bool) obj2;
                    }
                    num2 = i;
                }
            }
            return false;
        }

        private static AttributeMap GetAttribute(AttributeMap[] attribs, string fullName)
        {
            int num2;
            for (int i = 0; i < attribs.Length; i = num2 + 1)
            {
                AttributeMap map = attribs[i];
                if ((map != null) && (map.AttributeType.FullName == fullName))
                {
                    return map;
                }
                num2 = i;
            }
            return null;
        }

        private static System.Type GetBaseType(MetaType type)
        {
            return type.type.BaseType;
        }

        internal static AttributeFamily GetContractFamily(RuntimeTypeModel model, System.Type type, AttributeMap[] attributes)
        {
            int num2;
            MemberInfo[] infoArray;
            AttributeFamily none = AttributeFamily.None;
            if (attributes == null)
            {
                attributes = AttributeMap.Create(model, type, false);
            }
            for (int i = 0; i < attributes.Length; i = num2 + 1)
            {
                string fullName = attributes[i].AttributeType.FullName;
                if (!(fullName == "ProtoBuf.ProtoContractAttribute"))
                {
                    if (fullName == "System.Xml.Serialization.XmlTypeAttribute")
                    {
                        goto Label_007A;
                    }
                    if (fullName == "System.Runtime.Serialization.DataContractAttribute")
                    {
                        goto Label_0091;
                    }
                }
                else
                {
                    bool flag2 = false;
                    GetFieldBoolean(ref flag2, attributes[i], "UseProtoMembersOnly");
                    if (flag2)
                    {
                        return AttributeFamily.ProtoBuf;
                    }
                    none |= AttributeFamily.ProtoBuf;
                }
                goto Label_00A8;
            Label_007A:
                if (!model.AutoAddProtoContractTypesOnly)
                {
                    none |= AttributeFamily.XmlSerializer;
                }
                goto Label_00A8;
            Label_0091:
                if (!model.AutoAddProtoContractTypesOnly)
                {
                    none |= AttributeFamily.DataContractSerialier;
                }
            Label_00A8:
                num2 = i;
            }
            if ((none == AttributeFamily.None) && (ResolveTupleConstructor(type, out infoArray) > null))
            {
                none |= AttributeFamily.AutoTuple;
            }
            return none;
        }

        private static void GetDataFormat(ref DataFormat value, AttributeMap attrib, string memberName)
        {
            object obj2;
            if (((attrib != null) && (value <= DataFormat.Default)) && (attrib.TryGet(memberName, out obj2) && (obj2 > null)))
            {
                value = (DataFormat) obj2;
            }
        }

        internal EnumSerializer.EnumPair[] GetEnumMap()
        {
            int num3;
            if (this.HasFlag(2))
            {
                return null;
            }
            EnumSerializer.EnumPair[] pairArray = new EnumSerializer.EnumPair[this.fields.Count];
            for (int i = 0; i < pairArray.Length; i = num3 + 1)
            {
                ValueMember member = (ValueMember) this.fields[i];
                int fieldNumber = member.FieldNumber;
                object rawEnumValue = member.GetRawEnumValue();
                pairArray[i] = new EnumSerializer.EnumPair(fieldNumber, rawEnumValue, member.MemberType);
                num3 = i;
            }
            return pairArray;
        }

        private static void GetFieldBoolean(ref bool value, AttributeMap attrib, string memberName)
        {
            GetFieldBoolean(ref value, attrib, memberName, true);
        }

        private static bool GetFieldBoolean(ref bool value, AttributeMap attrib, string memberName, bool publicOnly)
        {
            if (attrib != null)
            {
                object obj2;
                if (value)
                {
                    return true;
                }
                if (attrib.TryGet(memberName, publicOnly, out obj2) && (obj2 > null))
                {
                    value = (bool) obj2;
                    return true;
                }
            }
            return false;
        }

        private static void GetFieldName(ref string name, AttributeMap attrib, string memberName)
        {
            object obj2;
            if (((attrib != null) && Helpers.IsNullOrEmpty(name)) && (attrib.TryGet(memberName, out obj2) && (obj2 > null)))
            {
                name = (string) obj2;
            }
        }

        private static void GetFieldNumber(ref int value, AttributeMap attrib, string memberName)
        {
            object obj2;
            if (((attrib != null) && (value <= 0)) && (attrib.TryGet(memberName, out obj2) && (obj2 > null)))
            {
                value = (int) obj2;
            }
        }

        public ValueMember[] GetFields()
        {
            ValueMember[] array = new ValueMember[this.fields.Count];
            this.fields.CopyTo(array, 0);
            Array.Sort<ValueMember>(array, ValueMember.Comparer.Default);
            return array;
        }

        private static void GetIgnore(ref bool ignore, AttributeMap attrib, AttributeMap[] attribs, string fullName)
        {
            if (!(ignore || (attrib == null)))
            {
                ignore = GetAttribute(attribs, fullName) > null;
            }
        }

        internal int GetKey(bool demand, bool getBaseKey)
        {
            return this.model.GetKey(this.type, demand, getBaseKey);
        }

        private int GetNextFieldNumber()
        {
            int fieldNumber = 0;
            BasicList.NodeEnumerator enumerator = this.fields.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ValueMember current = (ValueMember) enumerator.Current;
                if (current.FieldNumber > fieldNumber)
                {
                    fieldNumber = current.FieldNumber;
                }
            }
            if (this.subTypes > null)
            {
                BasicList.NodeEnumerator enumerator2 = this.subTypes.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    SubType type = (SubType) enumerator2.Current;
                    if (type.FieldNumber > fieldNumber)
                    {
                        fieldNumber = type.FieldNumber;
                    }
                }
            }
            return (fieldNumber + 1);
        }

        internal static MetaType GetRootType(MetaType source)
        {
            MetaType type2;
            while (source.serializer > null)
            {
                MetaType baseType = source.baseType;
                if (baseType == null)
                {
                    return source;
                }
                source = baseType;
            }
            RuntimeTypeModel model = source.model;
            int opaqueToken = 0;
            try
            {
                MetaType type3;
                model.TakeLock(ref opaqueToken);
                while ((type3 = source.baseType) > null)
                {
                    source = type3;
                }
                type2 = source;
            }
            finally
            {
                model.ReleaseLock(opaqueToken);
            }
            return type2;
        }

        internal string GetSchemaTypeName()
        {
            if (this.surrogate > null)
            {
                return this.model[this.surrogate].GetSchemaTypeName();
            }
            if (!Helpers.IsNullOrEmpty(this.name))
            {
                return this.name;
            }
            string name = this.type.Name;
            if (this.type.IsGenericType)
            {
                StringBuilder builder = new StringBuilder(name);
                int index = name.IndexOf('`');
                if (index >= 0)
                {
                    builder.Length = index;
                }
                foreach (System.Type type in this.type.GetGenericArguments())
                {
                    MetaType type3;
                    builder.Append('_');
                    System.Type type2 = type;
                    if (((this.model.GetKey(ref type2) >= 0) && ((type3 = this.model[type2]) != null)) && (type3.surrogate == null))
                    {
                        builder.Append(type3.GetSchemaTypeName());
                    }
                    else
                    {
                        builder.Append(type2.Name);
                    }
                }
                return builder.ToString();
            }
            return name;
        }

        public SubType[] GetSubtypes()
        {
            if ((this.subTypes == null) || (this.subTypes.Count == 0))
            {
                return new SubType[0];
            }
            SubType[] array = new SubType[this.subTypes.Count];
            this.subTypes.CopyTo(array, 0);
            Array.Sort<SubType>(array, SubType.Comparer.Default);
            return array;
        }

        internal MetaType GetSurrogateOrBaseOrSelf(bool deep)
        {
            if (this.surrogate > null)
            {
                return this.model[this.surrogate];
            }
            MetaType baseType = this.baseType;
            if (baseType > null)
            {
                if (deep)
                {
                    MetaType type3;
                    do
                    {
                        type3 = baseType;
                        baseType = baseType.baseType;
                    }
                    while (baseType > null);
                    return type3;
                }
                return baseType;
            }
            return this;
        }

        internal MetaType GetSurrogateOrSelf()
        {
            if (this.surrogate > null)
            {
                return this.model[this.surrogate];
            }
            return this;
        }

        private static bool HasFamily(AttributeFamily value, AttributeFamily required)
        {
            return ((value & required) == required);
        }

        private bool HasFlag(byte flag)
        {
            return ((this.flags & flag) == flag);
        }

        internal static Exception InbuiltType(System.Type type)
        {
            return new ArgumentException("Data of this type has inbuilt behaviour, and cannot be added to a model in this way: " + type.FullName);
        }

        internal bool IsDefined(int fieldNumber)
        {
            BasicList.NodeEnumerator enumerator = this.fields.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ValueMember current = (ValueMember) enumerator.Current;
                if (current.FieldNumber == fieldNumber)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool IsPrepared()
        {
            return (this.serializer is CompiledSerializer);
        }

        private bool IsValidSubType(System.Type subType)
        {
            return this.type.IsAssignableFrom(subType);
        }

        internal static StringBuilder NewLine(StringBuilder builder, int indent)
        {
            return Helpers.AppendLine(builder).Append(' ', indent * 3);
        }

        private static ProtoMemberAttribute NormalizeProtoMember(TypeModel model, MemberInfo member, AttributeFamily family, bool forced, bool isEnum, BasicList partialMembers, int dataMemberOffset, bool inferByTagName)
        {
            AttributeMap attribute;
            if ((member == null) || ((family == AttributeFamily.None) && !isEnum))
            {
                return null;
            }
            int num = -2147483648;
            int num2 = inferByTagName ? -1 : 1;
            string name = null;
            bool flag = false;
            bool ignore = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            bool flag7 = false;
            bool flag8 = false;
            bool flag9 = false;
            DataFormat format = DataFormat.Default;
            if (isEnum)
            {
                forced = true;
            }
            AttributeMap[] attribs = AttributeMap.Create(model, member, true);
            if (isEnum)
            {
                if (GetAttribute(attribs, "ProtoBuf.ProtoIgnoreAttribute") > null)
                {
                    ignore = true;
                }
                else
                {
                    attribute = GetAttribute(attribs, "ProtoBuf.ProtoEnumAttribute");
                    num = Convert.ToInt32(((FieldInfo) member).GetRawConstantValue());
                    if (attribute > null)
                    {
                        object obj2;
                        GetFieldName(ref name, attribute, "Name");
                        if (((bool) Helpers.GetInstanceMethod(attribute.AttributeType, "HasValue").Invoke(attribute.Target, null)) && attribute.TryGet("Value", out obj2))
                        {
                            num = (int) obj2;
                        }
                    }
                }
                flag3 = true;
            }
            if (!ignore && !flag3)
            {
                attribute = GetAttribute(attribs, "ProtoBuf.ProtoMemberAttribute");
                GetIgnore(ref ignore, attribute, attribs, "ProtoBuf.ProtoIgnoreAttribute");
                if (!ignore && (attribute > null))
                {
                    GetFieldNumber(ref num, attribute, "Tag");
                    GetFieldName(ref name, attribute, "Name");
                    GetFieldBoolean(ref flag4, attribute, "IsRequired");
                    GetFieldBoolean(ref flag, attribute, "IsPacked");
                    GetFieldBoolean(ref flag9, attribute, "OverwriteList");
                    GetDataFormat(ref format, attribute, "DataFormat");
                    GetFieldBoolean(ref flag6, attribute, "AsReferenceHasValue", false);
                    if (flag6)
                    {
                        flag6 = GetFieldBoolean(ref flag5, attribute, "AsReference", true);
                    }
                    GetFieldBoolean(ref flag7, attribute, "DynamicType");
                    flag3 = flag8 = num > 0;
                }
                if (!flag3 && (partialMembers > null))
                {
                    BasicList.NodeEnumerator enumerator = partialMembers.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        object obj3;
                        AttributeMap current = (AttributeMap) enumerator.Current;
                        if (current.TryGet("MemberName", out obj3) && (((string) obj3) == member.Name))
                        {
                            GetFieldNumber(ref num, current, "Tag");
                            GetFieldName(ref name, current, "Name");
                            GetFieldBoolean(ref flag4, current, "IsRequired");
                            GetFieldBoolean(ref flag, current, "IsPacked");
                            GetFieldBoolean(ref flag9, attribute, "OverwriteList");
                            GetDataFormat(ref format, current, "DataFormat");
                            GetFieldBoolean(ref flag6, attribute, "AsReferenceHasValue", false);
                            if (flag6)
                            {
                                flag6 = GetFieldBoolean(ref flag5, current, "AsReference", true);
                            }
                            GetFieldBoolean(ref flag7, current, "DynamicType");
                            if (flag3 = flag8 = num > 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            if ((!ignore && !flag3) && HasFamily(family, AttributeFamily.DataContractSerialier))
            {
                attribute = GetAttribute(attribs, "System.Runtime.Serialization.DataMemberAttribute");
                if (attribute > null)
                {
                    GetFieldNumber(ref num, attribute, "Order");
                    GetFieldName(ref name, attribute, "Name");
                    GetFieldBoolean(ref flag4, attribute, "IsRequired");
                    flag3 = num >= num2;
                    if (flag3)
                    {
                        num += dataMemberOffset;
                    }
                }
            }
            if ((!ignore && !flag3) && HasFamily(family, AttributeFamily.XmlSerializer))
            {
                attribute = GetAttribute(attribs, "System.Xml.Serialization.XmlElementAttribute");
                if (attribute == null)
                {
                    attribute = GetAttribute(attribs, "System.Xml.Serialization.XmlArrayAttribute");
                }
                GetIgnore(ref ignore, attribute, attribs, "System.Xml.Serialization.XmlIgnoreAttribute");
                if ((attribute != null) && !ignore)
                {
                    GetFieldNumber(ref num, attribute, "Order");
                    GetFieldName(ref name, attribute, "ElementName");
                    flag3 = num >= num2;
                }
            }
            if ((!ignore && !flag3) && (GetAttribute(attribs, "System.NonSerializedAttribute") > null))
            {
                ignore = true;
            }
            if (ignore || ((num < num2) && !forced))
            {
                return null;
            }
            return new ProtoMemberAttribute(num, forced | inferByTagName) { AsReference = flag5, AsReferenceHasValue = flag6, DataFormat = format, DynamicType = flag7, IsPacked = flag, OverwriteList = flag9, IsRequired = flag4, Name = Helpers.IsNullOrEmpty(name) ? member.Name : name, Member = member, TagIsPinned = flag8 };
        }

        internal static void ResolveListTypes(TypeModel model, System.Type type, ref System.Type itemType, ref System.Type defaultType)
        {
            if (type != null)
            {
                if (type.IsArray)
                {
                    if (type.GetArrayRank() != 1)
                    {
                        throw new NotSupportedException("Multi-dimension arrays are supported");
                    }
                    itemType = type.GetElementType();
                    if (itemType == model.MapType(typeof(byte)))
                    {
                        defaultType = (System.Type) (itemType = null);
                    }
                    else
                    {
                        defaultType = type;
                    }
                }
                if (itemType == null)
                {
                    itemType = TypeModel.GetListItemType(model, type);
                }
                if (itemType > null)
                {
                    System.Type type3 = null;
                    System.Type type4 = null;
                    ResolveListTypes(model, itemType, ref type3, ref type4);
                    if (type3 > null)
                    {
                        throw TypeModel.CreateNestedListsNotSupported();
                    }
                }
                if ((itemType != null) && (defaultType == null))
                {
                    if ((type.IsClass && !type.IsAbstract) && (Helpers.GetConstructor(type, Helpers.EmptyTypes, true) > null))
                    {
                        defaultType = type;
                    }
                    if ((defaultType == null) && type.IsInterface)
                    {
                        System.Type[] typeArray;
                        if ((type.IsGenericType && (type.GetGenericTypeDefinition() == model.MapType(typeof(IDictionary<,>)))) && (itemType == model.MapType(typeof(KeyValuePair<,>)).MakeGenericType(typeArray = type.GetGenericArguments())))
                        {
                            defaultType = model.MapType(typeof(Dictionary<,>)).MakeGenericType(typeArray);
                        }
                        else
                        {
                            System.Type[] typeArguments = new System.Type[] { itemType };
                            defaultType = model.MapType(typeof(List<>)).MakeGenericType(typeArguments);
                        }
                    }
                    if ((defaultType != null) && !Helpers.IsAssignableFrom(type, defaultType))
                    {
                        defaultType = null;
                    }
                }
            }
        }

        private MethodInfo ResolveMethod(string name, bool instance)
        {
            if (Helpers.IsNullOrEmpty(name))
            {
                return null;
            }
            return (instance ? Helpers.GetInstanceMethod(this.type, name) : Helpers.GetStaticMethod(this.type, name));
        }

        internal static ConstructorInfo ResolveTupleConstructor(System.Type type, out MemberInfo[] mappedMembers)
        {
            int num3;
            mappedMembers = null;
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (type.IsAbstract)
            {
                return null;
            }
            ConstructorInfo[] constructors = Helpers.GetConstructors(type, false);
            if ((constructors.Length == 0) || ((constructors.Length == 1) && (constructors[0].GetParameters().Length == 0)))
            {
                return null;
            }
            MemberInfo[] instanceFieldsAndProperties = Helpers.GetInstanceFieldsAndProperties(type, true);
            BasicList list = new BasicList();
            for (int i = 0; i < instanceFieldsAndProperties.Length; i = num3 + 1)
            {
                PropertyInfo property = instanceFieldsAndProperties[i] as PropertyInfo;
                if (property > null)
                {
                    if (!property.CanRead)
                    {
                        return null;
                    }
                    if (property.CanWrite && (Helpers.GetSetMethod(property, false, false) > null))
                    {
                        return null;
                    }
                    list.Add(property);
                }
                else
                {
                    FieldInfo info4 = instanceFieldsAndProperties[i] as FieldInfo;
                    if (info4 > null)
                    {
                        if (!info4.IsInitOnly)
                        {
                            return null;
                        }
                        list.Add(info4);
                    }
                }
                num3 = i;
            }
            if (list.Count == 0)
            {
                return null;
            }
            MemberInfo[] array = new MemberInfo[list.Count];
            list.CopyTo(array, 0);
            int[] numArray = new int[array.Length];
            int num = 0;
            ConstructorInfo info = null;
            mappedMembers = new MemberInfo[numArray.Length];
            for (int j = 0; j < constructors.Length; j = num3 + 1)
            {
                ParameterInfo[] parameters = constructors[j].GetParameters();
                if (parameters.Length == array.Length)
                {
                    for (int k = 0; k < numArray.Length; k = num3 + 1)
                    {
                        numArray[k] = -1;
                        num3 = k;
                    }
                    for (int m = 0; m < parameters.Length; m = num3 + 1)
                    {
                        string str = parameters[m].Name.ToLower();
                        for (int num7 = 0; num7 < array.Length; num7 = num3 + 1)
                        {
                            if (!(array[num7].Name.ToLower() != str) && (Helpers.GetMemberType(array[num7]) == parameters[m].ParameterType))
                            {
                                numArray[m] = num7;
                            }
                            num3 = num7;
                        }
                        num3 = m;
                    }
                    bool flag11 = false;
                    for (int n = 0; n < numArray.Length; n = num3 + 1)
                    {
                        if (numArray[n] < 0)
                        {
                            flag11 = true;
                            break;
                        }
                        mappedMembers[n] = array[numArray[n]];
                        num3 = n;
                    }
                    if (!flag11)
                    {
                        num3 = num;
                        num = num3 + 1;
                        info = constructors[j];
                    }
                }
                num3 = j;
            }
            return ((num == 1) ? info : null);
        }

        private void SetBaseType(MetaType baseType)
        {
            if (baseType == null)
            {
                throw new ArgumentNullException("baseType");
            }
            if (this.baseType != baseType)
            {
                if (this.baseType > null)
                {
                    throw new InvalidOperationException("A type can only participate in one inheritance hierarchy");
                }
                for (MetaType type = baseType; type > null; type = type.baseType)
                {
                    if (type == this)
                    {
                        throw new InvalidOperationException("Cyclic inheritance is not allowed");
                    }
                }
                this.baseType = baseType;
            }
        }

        public MetaType SetCallbacks(MethodInfo beforeSerialize, MethodInfo afterSerialize, MethodInfo beforeDeserialize, MethodInfo afterDeserialize)
        {
            CallbackSet callbacks = this.Callbacks;
            callbacks.BeforeSerialize = beforeSerialize;
            callbacks.AfterSerialize = afterSerialize;
            callbacks.BeforeDeserialize = beforeDeserialize;
            callbacks.AfterDeserialize = afterDeserialize;
            return this;
        }

        public MetaType SetCallbacks(string beforeSerialize, string afterSerialize, string beforeDeserialize, string afterDeserialize)
        {
            if (this.IsValueType)
            {
                throw new InvalidOperationException();
            }
            CallbackSet callbacks = this.Callbacks;
            callbacks.BeforeSerialize = this.ResolveMethod(beforeSerialize, true);
            callbacks.AfterSerialize = this.ResolveMethod(afterSerialize, true);
            callbacks.BeforeDeserialize = this.ResolveMethod(beforeDeserialize, true);
            callbacks.AfterDeserialize = this.ResolveMethod(afterDeserialize, true);
            return this;
        }

        public MetaType SetFactory(MethodInfo factory)
        {
            this.model.VerifyFactory(factory, this.type);
            this.ThrowIfFrozen();
            this.factory = factory;
            return this;
        }

        public MetaType SetFactory(string factory)
        {
            return this.SetFactory(this.ResolveMethod(factory, false));
        }

        private void SetFlag(byte flag, bool value, bool throwIfFrozen)
        {
            if (throwIfFrozen && (this.HasFlag(flag) != value))
            {
                this.ThrowIfFrozen();
            }
            if (value)
            {
                this.flags = (byte) (this.flags | flag);
            }
            else
            {
                this.flags = (byte) (this.flags & ~flag);
            }
        }

        public void SetSurrogate(System.Type surrogateType)
        {
            if (surrogateType == this.type)
            {
                surrogateType = null;
            }
            if ((surrogateType > null) && ((surrogateType != null) && Helpers.IsAssignableFrom(this.model.MapType(typeof(IEnumerable)), surrogateType)))
            {
                throw new ArgumentException("Repeated data (a list, collection, etc) has inbuilt behaviour and cannot be used as a surrogate");
            }
            this.ThrowIfFrozen();
            this.surrogate = surrogateType;
        }

        protected internal void ThrowIfFrozen()
        {
            if ((this.flags & 4) > 0)
            {
                throw new InvalidOperationException("The type cannot be changed once a serializer has been generated for " + this.type.FullName);
            }
        }

        public override string ToString()
        {
            return this.type.ToString();
        }

        internal void WriteSchema(StringBuilder builder, int indent, ref bool requiresBclImport)
        {
            if (this.surrogate <= null)
            {
                ValueMember[] array = new ValueMember[this.fields.Count];
                this.fields.CopyTo(array, 0);
                Array.Sort<ValueMember>(array, ValueMember.Comparer.Default);
                if (this.IsList)
                {
                    string str = this.model.GetSchemaTypeName(TypeModel.GetListItemType(this.model, this.type), DataFormat.Default, false, false, ref requiresBclImport);
                    NewLine(builder, indent).Append("message ").Append(this.GetSchemaTypeName()).Append(" {");
                    NewLine(builder, indent + 1).Append("repeated ").Append(str).Append(" items = 1;");
                    NewLine(builder, indent).Append('}');
                }
                else if (this.IsAutoTuple)
                {
                    MemberInfo[] infoArray;
                    if (ResolveTupleConstructor(this.type, out infoArray) > null)
                    {
                        int num2;
                        NewLine(builder, indent).Append("message ").Append(this.GetSchemaTypeName()).Append(" {");
                        for (int i = 0; i < infoArray.Length; i = num2 + 1)
                        {
                            System.Type propertyType;
                            if (infoArray[i] is PropertyInfo)
                            {
                                propertyType = ((PropertyInfo) infoArray[i]).PropertyType;
                            }
                            else
                            {
                                if (!(infoArray[i] is FieldInfo))
                                {
                                    throw new NotSupportedException("Unknown member type: " + infoArray[i].GetType().Name);
                                }
                                propertyType = ((FieldInfo) infoArray[i]).FieldType;
                            }
                            NewLine(builder, indent + 1).Append("optional ").Append(this.model.GetSchemaTypeName(propertyType, DataFormat.Default, false, false, ref requiresBclImport).Replace('.', '_')).Append(' ').Append(infoArray[i].Name).Append(" = ").Append((int) (i + 1)).Append(';');
                            num2 = i;
                        }
                        NewLine(builder, indent).Append('}');
                    }
                }
                else if (Helpers.IsEnum(this.type))
                {
                    NewLine(builder, indent).Append("enum ").Append(this.GetSchemaTypeName()).Append(" {");
                    if ((array.Length == 0) && this.EnumPassthru)
                    {
                        if (this.type.IsDefined(this.model.MapType(typeof(FlagsAttribute)), false))
                        {
                            NewLine(builder, indent + 1).Append("// this is a composite/flags enumeration");
                        }
                        else
                        {
                            NewLine(builder, indent + 1).Append("// this enumeration will be passed as a raw value");
                        }
                        foreach (FieldInfo info in this.type.GetFields())
                        {
                            if (info.IsStatic && info.IsLiteral)
                            {
                                object rawConstantValue = info.GetRawConstantValue();
                                NewLine(builder, indent + 1).Append(info.Name).Append(" = ").Append(rawConstantValue).Append(";");
                            }
                        }
                    }
                    else
                    {
                        foreach (ValueMember member in array)
                        {
                            NewLine(builder, indent + 1).Append(member.Name).Append(" = ").Append(member.FieldNumber).Append(';');
                        }
                    }
                    NewLine(builder, indent).Append('}');
                }
                else
                {
                    NewLine(builder, indent).Append("message ").Append(this.GetSchemaTypeName()).Append(" {");
                    foreach (ValueMember member2 in array)
                    {
                        string str2 = (member2.ItemType != null) ? "repeated" : (member2.IsRequired ? "required" : "optional");
                        NewLine(builder, indent + 1).Append(str2).Append(' ');
                        if (member2.DataFormat == DataFormat.Group)
                        {
                            builder.Append("group ");
                        }
                        string schemaTypeName = member2.GetSchemaTypeName(true, ref requiresBclImport);
                        builder.Append(schemaTypeName).Append(" ").Append(member2.Name).Append(" = ").Append(member2.FieldNumber);
                        if (member2.DefaultValue > null)
                        {
                            if (member2.DefaultValue is string)
                            {
                                builder.Append(" [default = \"").Append(member2.DefaultValue).Append("\"]");
                            }
                            else if (member2.DefaultValue is bool)
                            {
                                builder.Append(((bool) member2.DefaultValue) ? " [default = true]" : " [default = false]");
                            }
                            else
                            {
                                builder.Append(" [default = ").Append(member2.DefaultValue).Append(']');
                            }
                        }
                        if ((member2.ItemType != null) && member2.IsPacked)
                        {
                            builder.Append(" [packed=true]");
                        }
                        builder.Append(';');
                        if (((schemaTypeName == "bcl.NetObjectProxy") && member2.AsReference) && !member2.DynamicType)
                        {
                            builder.Append(" // reference-tracked ").Append(member2.GetSchemaTypeName(false, ref requiresBclImport));
                        }
                    }
                    if ((this.subTypes != null) && (this.subTypes.Count > 0))
                    {
                        NewLine(builder, indent + 1).Append("// the following represent sub-types; at most 1 should have a value");
                        SubType[] typeArray = new SubType[this.subTypes.Count];
                        this.subTypes.CopyTo(typeArray, 0);
                        Array.Sort<SubType>(typeArray, SubType.Comparer.Default);
                        foreach (SubType type2 in typeArray)
                        {
                            string str4 = type2.DerivedType.GetSchemaTypeName();
                            NewLine(builder, indent + 1).Append("optional ").Append(str4).Append(" ").Append(str4).Append(" = ").Append(type2.FieldNumber).Append(';');
                        }
                    }
                    NewLine(builder, indent).Append('}');
                }
            }
        }

        public bool AsReferenceDefault
        {
            get
            {
                return this.HasFlag(0x20);
            }
            set
            {
                this.SetFlag(0x20, value, true);
            }
        }

        public MetaType BaseType
        {
            get
            {
                return this.baseType;
            }
        }

        public CallbackSet Callbacks
        {
            get
            {
                if (this.callbacks == null)
                {
                    this.callbacks = new CallbackSet(this);
                }
                return this.callbacks;
            }
        }

        public System.Type ConstructType
        {
            get
            {
                return this.constructType;
            }
            set
            {
                this.ThrowIfFrozen();
                this.constructType = value;
            }
        }

        public bool EnumPassthru
        {
            get
            {
                return this.HasFlag(2);
            }
            set
            {
                this.SetFlag(2, value, true);
            }
        }

        internal IEnumerable Fields
        {
            get
            {
                return this.fields;
            }
        }

        public bool HasCallbacks
        {
            get
            {
                return ((this.callbacks != null) && this.callbacks.NonTrivial);
            }
        }

        public bool HasSubtypes
        {
            get
            {
                return ((this.subTypes != null) && (this.subTypes.Count > 0));
            }
        }

        public bool IgnoreListHandling
        {
            get
            {
                return this.HasFlag(0x80);
            }
            set
            {
                this.SetFlag(0x80, value, true);
            }
        }

        public bool IncludeSerializerMethod
        {
            get
            {
                return !this.HasFlag(8);
            }
            set
            {
                this.SetFlag(8, !value, true);
            }
        }

        internal bool IsAutoTuple
        {
            get
            {
                return this.HasFlag(0x40);
            }
        }

        internal bool IsList
        {
            get
            {
                System.Type type = this.IgnoreListHandling ? null : TypeModel.GetListItemType(this.model, this.type);
                return (type > null);
            }
        }

        private bool IsValueType
        {
            get
            {
                return this.type.IsValueType;
            }
        }

        public ValueMember this[int fieldNumber]
        {
            get
            {
                BasicList.NodeEnumerator enumerator = this.fields.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    ValueMember current = (ValueMember) enumerator.Current;
                    if (current.FieldNumber == fieldNumber)
                    {
                        return current;
                    }
                }
                return null;
            }
        }

        public ValueMember this[MemberInfo member]
        {
            get
            {
                if (member != null)
                {
                    BasicList.NodeEnumerator enumerator = this.fields.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        ValueMember current = (ValueMember) enumerator.Current;
                        if (current.Member == member)
                        {
                            return current;
                        }
                    }
                }
                return null;
            }
        }

        internal TypeModel Model
        {
            get
            {
                return this.model;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.ThrowIfFrozen();
                this.name = value;
            }
        }

        internal bool Pending
        {
            get
            {
                return this.HasFlag(1);
            }
            set
            {
                this.SetFlag(1, value, false);
            }
        }

        IProtoSerializer ISerializerProxy.Serializer
        {
            get
            {
                return this.Serializer;
            }
        }

        internal IProtoTypeSerializer Serializer
        {
            get
            {
                if (this.serializer == null)
                {
                    int opaqueToken = 0;
                    try
                    {
                        this.model.TakeLock(ref opaqueToken);
                        if (this.serializer == null)
                        {
                            this.SetFlag(4, true, false);
                            this.serializer = this.BuildSerializer();
                            if (this.model.AutoCompile)
                            {
                                this.CompileInPlace();
                            }
                        }
                    }
                    finally
                    {
                        this.model.ReleaseLock(opaqueToken);
                    }
                }
                return this.serializer;
            }
        }

        public System.Type Type
        {
            get
            {
                return this.type;
            }
        }

        public bool UseConstructor
        {
            get
            {
                return !this.HasFlag(0x10);
            }
            set
            {
                this.SetFlag(0x10, !value, true);
            }
        }

        [Flags]
        internal enum AttributeFamily
        {
            AutoTuple = 8,
            DataContractSerialier = 2,
            None = 0,
            ProtoBuf = 1,
            XmlSerializer = 4
        }

        internal sealed class Comparer : IComparer, IComparer<MetaType>
        {
            public static readonly MetaType.Comparer Default = new MetaType.Comparer();

            public int Compare(MetaType x, MetaType y)
            {
                if (x == y)
                {
                    return 0;
                }
                if (x == null)
                {
                    return -1;
                }
                if (y == null)
                {
                    return 1;
                }
                return string.Compare(x.GetSchemaTypeName(), y.GetSchemaTypeName(), StringComparison.Ordinal);
            }

            public int Compare(object x, object y)
            {
                return this.Compare(x as MetaType, y as MetaType);
            }
        }
    }
}


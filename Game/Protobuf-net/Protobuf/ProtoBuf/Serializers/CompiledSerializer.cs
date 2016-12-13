namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;

    internal sealed class CompiledSerializer : IProtoTypeSerializer, IProtoSerializer
    {
        private readonly ProtoDeserializer deserializer;
        private readonly IProtoTypeSerializer head;
        private readonly ProtoSerializer serializer;

        private CompiledSerializer(IProtoTypeSerializer head, TypeModel model)
        {
            this.head = head;
            this.serializer = CompilerContext.BuildSerializer(head, model);
            this.deserializer = CompilerContext.BuildDeserializer(head, model);
        }

        public void Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
        {
            this.head.Callback(value, callbackType, context);
        }

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            this.head.EmitRead(ctx, valueFrom);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            this.head.EmitWrite(ctx, valueFrom);
        }

        object IProtoSerializer.Read(object value, ProtoReader source)
        {
            return this.deserializer(value, source);
        }

        void IProtoSerializer.Write(object value, ProtoWriter dest)
        {
            this.serializer(value, dest);
        }

        bool IProtoTypeSerializer.CanCreateInstance()
        {
            return this.head.CanCreateInstance();
        }

        object IProtoTypeSerializer.CreateInstance(ProtoReader source)
        {
            return this.head.CreateInstance(source);
        }

        void IProtoTypeSerializer.EmitCallback(CompilerContext ctx, Local valueFrom, TypeModel.CallbackType callbackType)
        {
            this.head.EmitCallback(ctx, valueFrom, callbackType);
        }

        void IProtoTypeSerializer.EmitCreateInstance(CompilerContext ctx)
        {
            this.head.EmitCreateInstance(ctx);
        }

        bool IProtoTypeSerializer.HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return this.head.HasCallbacks(callbackType);
        }

        public static CompiledSerializer Wrap(IProtoTypeSerializer head, TypeModel model)
        {
            CompiledSerializer serializer = head as CompiledSerializer;
            if (serializer == null)
            {
                serializer = new CompiledSerializer(head, model);
                Helpers.DebugAssert(((IProtoSerializer) serializer).ExpectedType == head.ExpectedType);
            }
            return serializer;
        }

        Type IProtoSerializer.ExpectedType
        {
            get
            {
                return this.head.ExpectedType;
            }
        }

        bool IProtoSerializer.RequiresOldValue
        {
            get
            {
                return this.head.RequiresOldValue;
            }
        }

        bool IProtoSerializer.ReturnsValue
        {
            get
            {
                return this.head.ReturnsValue;
            }
        }
    }
}


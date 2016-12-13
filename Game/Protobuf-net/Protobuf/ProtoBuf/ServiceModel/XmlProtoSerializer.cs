namespace ProtoBuf.ServiceModel
{
    using ProtoBuf;
    using ProtoBuf.Meta;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Xml;

    public sealed class XmlProtoSerializer : XmlObjectSerializer
    {
        private readonly bool isEnum;
        private readonly bool isList;
        private readonly int key;
        private readonly TypeModel model;
        private const string PROTO_ELEMENT = "proto";
        private readonly Type type;

        public XmlProtoSerializer(TypeModel model, Type type)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.key = GetKey(model, ref type, out this.isList);
            this.model = model;
            this.type = type;
            this.isEnum = Helpers.IsEnum(type);
            if (this.key < 0)
            {
                throw new ArgumentOutOfRangeException("type", "Type not recognised by the model: " + type.FullName);
            }
        }

        internal XmlProtoSerializer(TypeModel model, int key, Type type, bool isList)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (key < 0)
            {
                throw new ArgumentOutOfRangeException("key");
            }
            if (type == null)
            {
                throw new ArgumentOutOfRangeException("type");
            }
            this.model = model;
            this.key = key;
            this.isList = isList;
            this.type = type;
            this.isEnum = Helpers.IsEnum(type);
        }

        private static int GetKey(TypeModel model, ref Type type, out bool isList)
        {
            if ((model != null) && (type > null))
            {
                int key = model.GetKey(ref type);
                if (key >= 0)
                {
                    isList = false;
                    return key;
                }
                Type listItemType = TypeModel.GetListItemType(model, type);
                if (listItemType > null)
                {
                    key = model.GetKey(ref listItemType);
                    if (key >= 0)
                    {
                        isList = true;
                        return key;
                    }
                }
            }
            isList = false;
            return -1;
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            reader.MoveToContent();
            return ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "proto"));
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            object obj2;
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            reader.MoveToContent();
            bool isEmptyElement = reader.IsEmptyElement;
            bool flag2 = reader.GetAttribute("nil") == "true";
            reader.ReadStartElement("proto");
            if (flag2)
            {
                if (!isEmptyElement)
                {
                    reader.ReadEndElement();
                }
                return null;
            }
            if (isEmptyElement)
            {
                if (this.isList || this.isEnum)
                {
                    return this.model.Deserialize(Stream.Null, null, this.type, (SerializationContext) null);
                }
                ProtoReader source = null;
                try
                {
                    source = ProtoReader.Create(Stream.Null, this.model, null, -1);
                    return this.model.Deserialize(this.key, null, source);
                }
                finally
                {
                    ProtoReader.Recycle(source);
                }
            }
            Helpers.DebugAssert(reader.CanReadBinaryContent, "CanReadBinaryContent");
            using (MemoryStream stream = new MemoryStream(reader.ReadContentAsBase64()))
            {
                if (this.isList || this.isEnum)
                {
                    obj2 = this.model.Deserialize(stream, null, this.type, (SerializationContext) null);
                }
                else
                {
                    ProtoReader reader3 = null;
                    try
                    {
                        reader3 = ProtoReader.Create(stream, this.model, null, -1);
                        obj2 = this.model.Deserialize(this.key, null, reader3);
                    }
                    finally
                    {
                        ProtoReader.Recycle(reader3);
                    }
                }
            }
            reader.ReadEndElement();
            return obj2;
        }

        public static XmlProtoSerializer TryCreate(TypeModel model, Type type)
        {
            bool flag;
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            int key = GetKey(model, ref type, out flag);
            if (key >= 0)
            {
                return new XmlProtoSerializer(model, key, type, flag);
            }
            return null;
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            writer.WriteEndElement();
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (graph == null)
            {
                writer.WriteAttributeString("nil", "true");
            }
            else
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    if (this.isList)
                    {
                        this.model.Serialize(stream, graph, null);
                    }
                    else
                    {
                        using (ProtoWriter writer2 = new ProtoWriter(stream, this.model, null))
                        {
                            this.model.Serialize(this.key, graph, writer2);
                        }
                    }
                    byte[] buffer = stream.GetBuffer();
                    writer.WriteBase64(buffer, 0, (int) stream.Length);
                }
            }
        }

        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            writer.WriteStartElement("proto");
        }
    }
}


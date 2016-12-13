namespace ProtoBuf
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ProtoException : Exception
    {
        public ProtoException()
        {
        }

        public ProtoException(string message) : base(message)
        {
        }

        protected ProtoException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ProtoException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}


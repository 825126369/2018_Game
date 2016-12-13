namespace ProtoBuf
{
    using System;
    using System.Threading;

    internal sealed class BufferPool
    {
        internal const int BufferLength = 0x400;
        private static readonly object[] pool = new object[20];
        private const int PoolSize = 20;

        private BufferPool()
        {
        }

        internal static void Flush()
        {
            int num2;
            for (int i = 0; i < pool.Length; i = num2 + 1)
            {
                Interlocked.Exchange(ref pool[i], null);
                num2 = i;
            }
        }

        internal static byte[] GetBuffer()
        {
            int num2;
            for (int i = 0; i < pool.Length; i = num2 + 1)
            {
                object obj2 = Interlocked.Exchange(ref pool[i], null);
                if (obj2 > null)
                {
                    return (byte[]) obj2;
                }
                num2 = i;
            }
            return new byte[0x400];
        }

        internal static void ReleaseBufferToPool(ref byte[] buffer)
        {
            if (buffer != null)
            {
                if (buffer.Length == 0x400)
                {
                    int num2;
                    for (int i = 0; i < pool.Length; i = num2 + 1)
                    {
                        if (Interlocked.CompareExchange(ref pool[i], buffer, null) == null)
                        {
                            break;
                        }
                        num2 = i;
                    }
                }
                buffer = null;
            }
        }

        internal static void ResizeAndFlushLeft(ref byte[] buffer, int toFitAtLeastBytes, int copyFromIndex, int copyBytes)
        {
            Helpers.DebugAssert(buffer > null);
            Helpers.DebugAssert(toFitAtLeastBytes > buffer.Length);
            Helpers.DebugAssert(copyFromIndex >= 0);
            Helpers.DebugAssert(copyBytes >= 0);
            int num = buffer.Length * 2;
            if (num < toFitAtLeastBytes)
            {
                num = toFitAtLeastBytes;
            }
            byte[] to = new byte[num];
            if (copyBytes > 0)
            {
                Helpers.BlockCopy(buffer, copyFromIndex, to, 0, copyBytes);
            }
            if (buffer.Length == 0x400)
            {
                ReleaseBufferToPool(ref buffer);
            }
            buffer = to;
        }
    }
}


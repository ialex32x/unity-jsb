using System.Collections.Generic;

namespace QuickJS.IO
{
    public class ByteBufferUnpooledAllocator : IByteBufferAllocator
    {
        private List<ByteBuffer> _autoreleases = new List<ByteBuffer>();

        public void AutoRelease(ByteBuffer b)
        {
            _autoreleases.Add(b);
        }

        public void Drain()
        {
            var size = _autoreleases.Count;
            if (size > 0)
            {
                for (var i = 0; i < size; ++i)
                {
                    var b = _autoreleases[i];
                    b.Release();
                }
                _autoreleases.Clear();
            }
        }

        // 返回一个 ByteBuffer 对象, 大小至少为 size
        public ByteBuffer Alloc(int size)
        {
            return new ByteBuffer(size, int.MaxValue, null);
        }

        public void Recycle(ByteBuffer byteBuffer)
        {
        }
    }
}

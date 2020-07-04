using System.Collections.Generic;

namespace QuickJS.IO
{
    // 简易的 ByteBuffer 池分配器, 非线程安全
    public class ByteBufferPooledAllocator : IByteBufferAllocator
    {
        private int _maxCapacity;
        private List<ByteBuffer> __freelist;
        private List<ByteBuffer> _autoreleases = new List<ByteBuffer>();

        public ByteBufferPooledAllocator()
        : this(24, 512, int.MaxValue)
        {
        }

        // 预分配缓冲池
        public ByteBufferPooledAllocator(int prealloc, int initialCapacity, int maxCapacity)
        {
            _maxCapacity = maxCapacity;
            __freelist = new List<ByteBuffer>(prealloc);
            while (prealloc-- > 0)
            {
                __freelist.Add(new ByteBuffer(initialCapacity, maxCapacity, this));
            }
        }

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

        // 返回一个由对象池分配的 ByteBuffer 对象, 大小至少为 size
        public ByteBuffer Alloc(int size)
        {
            var count = __freelist.Count;
            if (count > 0)
            {
                var free = __freelist[count - 1];
                __freelist.RemoveAt(count - 1);
                free.Retain();
                return free;
            }
            return new ByteBuffer(size, _maxCapacity, this).Retain();
        }

        public void Recycle(ByteBuffer byteBuffer)
        {
            __freelist.Add(byteBuffer);
        }
    }
}

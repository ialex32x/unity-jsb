using System.Collections.Generic;

namespace QuickJS.IO
{
    public abstract class ByteBufferAllocator
    {
        public const int DEFAULT_SIZE = 1024;
        public const int DEFAULT_MAX_CAPACITY = int.MaxValue;

        protected bool _traceMemoryLeak = false;

        private List<ByteBuffer> _autoreleases = new List<ByteBuffer>();

        public bool traceMemoryLeak
        {
            get { return _traceMemoryLeak; }
        }

        public ByteBuffer Alloc()
        {
            return Alloc(DEFAULT_SIZE);
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

        // 返回一个由对象池分配的 ByteBuffer 对象, 初始大小至少为 size
        // Alloc、Release 调用为线程安全的，其余操作需要调用者自己保证线程安全性
        public abstract ByteBuffer Alloc(int size);

        public abstract void Recycle(ByteBuffer byteBuffer);
    }
}

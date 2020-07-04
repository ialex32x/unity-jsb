using System.Collections.Generic;

namespace QuickJS.IO
{
    public interface IByteBufferAllocator
    {
        void AutoRelease(ByteBuffer b);

        void Drain();

        // 返回一个由对象池分配的 ByteBuffer 对象, 初始大小至少为 size
        ByteBuffer Alloc(int size);

        void Recycle(ByteBuffer byteBuffer);
    }
}

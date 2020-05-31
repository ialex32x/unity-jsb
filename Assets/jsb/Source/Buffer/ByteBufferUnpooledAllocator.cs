
namespace QuickJS.IO
{
    public class ByteBufferUnpooledAllocator : ByteBufferAllocator
    {
        // 返回一个由对象池分配的 ByteBuffer 对象, 大小至少为 size
        // Alloc、Release 调用为线程安全的，其余操作需要调用者自己保证线程安全性
        public override ByteBuffer Alloc(int size)
        {
            return new ByteBuffer(size, ByteBufferAllocator.DEFAULT_MAX_CAPACITY, null);
        }

        public override void Recycle(ByteBuffer byteBuffer)
        {
        }
    }
}

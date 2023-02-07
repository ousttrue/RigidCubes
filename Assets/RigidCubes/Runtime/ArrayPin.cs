using System;
using System.Runtime.InteropServices;

namespace RigidCubes
{
    public class ArrayPin : IDisposable
    {
        GCHandle pinnedJointArray_;
        int offset_;

        public IntPtr Ptr => IntPtr.Add(pinnedJointArray_.AddrOfPinnedObject(), offset_);

        public ArrayPin(ArraySegment<byte> bytes)
        {
            pinnedJointArray_ = GCHandle.Alloc(bytes.Array, GCHandleType.Pinned);
            offset_ = bytes.Offset;
        }

        public ArrayPin(Array array)
        {
            pinnedJointArray_ = GCHandle.Alloc(array, GCHandleType.Pinned);
            offset_ = 0;
        }

        public void Dispose()
        {
            pinnedJointArray_.Free();
        }
    }
}

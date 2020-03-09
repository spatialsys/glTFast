using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;

namespace GLTFast
{
    enum AccessorUsage {
        Unknown,
        Ignore,
        Index,
        IndexFlipped,
        Position,
        Normal,
        Tangent,
        UV,
        Color
    }

    abstract class AccessorDataBase {
        public abstract void Unpin();
        public abstract void Dispose();
    }

    class AccessorData<T> : AccessorDataBase {
        public T[] data;
        public GCHandle gcHandle;

        public override void Unpin() {
            gcHandle.Free();
        }
        public override void Dispose() {}
    }

    class AccessorNativeData<T> : AccessorDataBase where T: struct
    {
        public NativeArray<T> data;
        public override void Unpin() {}
        public override void Dispose() {
            data.Dispose();
        }
    }
}

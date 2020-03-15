using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;
using Unity.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Profiling;

namespace GLTFast {

    using Schema;

    class PrimitiveCreateContext : PrimitiveCreateContextBase {

        const int MAX_STREAM = 4;

        public Mesh mesh;
        public VertexBufferConfigBase vertexData;

        public JobHandle jobHandle;
        public int[][] indices;

        public GCHandle calculatedIndicesHandle;

        public MeshTopology topology;

        public override bool IsCompleted {
            get {
                return jobHandle.IsCompleted;
            }  
        }

        public override Primitive? CreatePrimitive() {
            Profiler.BeginSample("CreatePrimitive");
            Profiler.BeginSample("Job Complete");
            jobHandle.Complete();
            Profiler.EndSample();
            var msh = new UnityEngine.Mesh();
            msh.name = mesh.name;

            bool hasUVs = false; // TODO: dynamic
            bool calculateNormals = needsNormals && ( topology==MeshTopology.Triangles || topology==MeshTopology.Quads );
            bool calculateTangents = needsTangents && hasUVs && (topology==MeshTopology.Triangles || topology==MeshTopology.Quads);
            CreatePrimitiveAdvanced(msh,calculateNormals,calculateTangents);

            Profiler.BeginSample("Dispose");
            Dispose();
            Profiler.EndSample();
            return new Primitive(msh,materials);
        }
        
        void CreatePrimitiveAdvanced(UnityEngine.Mesh msh,bool calculateNormals,bool calculateTangents) {

            Profiler.BeginSample("CreatePrimitiveAdvanced");
            Profiler.BeginSample("SetVertexBufferParams");

            MeshUpdateFlags flags = MeshUpdateFlags.Default;// (MeshUpdateFlags)~0;

            vertexData.ApplyOnMesh(msh,flags);

            Profiler.BeginSample("SetIndices");
            int indexCount = 0;
            for (int i = 0; i < indices.Length; i++) {
                indexCount += indices[i].Length;
            }
            msh.SetIndexBufferParams(indexCount,IndexFormat.UInt32); //TODO: UInt16 maybe?
            msh.subMeshCount = indices.Length;
            indexCount = 0;
            for (int i = 0; i < indices.Length; i++) {
                Profiler.BeginSample("SetIndexBufferData");
                msh.SetIndexBufferData(indices[i],0,indexCount,indices[i].Length,flags);
                Profiler.EndSample();
                Profiler.BeginSample("SetSubMesh");
                msh.SetSubMesh(i,new SubMeshDescriptor(indexCount,indices[i].Length,topology),flags);
                Profiler.EndSample();
                indexCount += indices[i].Length;
            }
            Profiler.EndSample();

            /*
            if(!normals.IsCreated && calculateNormals) {
                Profiler.BeginSample("RecalculateNormals");
                msh.RecalculateNormals();
                Profiler.EndSample();
            }
            if(!tangents.IsCreated && calculateTangents) {
                Profiler.BeginSample("RecalculateTangents");
                msh.RecalculateTangents();
                Profiler.EndSample();
            }
            //*/
            
            Profiler.BeginSample("RecalculateBounds");
            msh.RecalculateBounds(); // TODO: make optional! maybe calculate bounds in Job.
            Profiler.EndSample();

            Profiler.BeginSample("UploadMeshData");
            msh.UploadMeshData(true);
            Profiler.EndSample();

            Profiler.EndSample(); // CreatePrimitiveAdvances
        }
        
        void Dispose() {
            if(calculatedIndicesHandle.IsAllocated) {
                calculatedIndicesHandle.Free();
            }
        }
    }
}
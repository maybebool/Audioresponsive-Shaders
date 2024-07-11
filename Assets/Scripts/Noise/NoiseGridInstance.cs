using UnityEngine;

namespace Noise {
    public abstract class NoiseGridInstance : MonoBehaviour
    {

        public uint rez = 64; // How many instances per side. (rez * rez => total instances)
        public CustomRenderTexture inTex; // The texture we will use as a base
    
        public Material outMat; // the instancing shader material
        public Mesh iMesh; // the mesh to instance
    
        protected ComputeBuffer _bufferArgumentsData; // instance info to pass to the shader

        protected Bounds _bounds;
    
        void Start()
        {
            // Indirect Args Buffer determines how many instances to draw
            _bufferArgumentsData = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
            var arguments = new uint[5];
            arguments[0] = iMesh.GetIndexCount(0); // index count per instance,
            arguments[1] = rez * rez; // instance count,
            arguments[2] = iMesh.GetIndexStart(0); // start index location,
            arguments[3] = iMesh.GetBaseVertex(0); // base vertex location,
            arguments[4] = 0; // start instance location.
            _bufferArgumentsData.SetData(arguments); // Store it to a buffer in GPU land

            // Set bounds
            _bounds = new Bounds(Vector3.zero, Vector3.one * 100);
        
            // Pass texture to shader graph
            outMat.SetTexture("_inTex", inTex);
            outMat.SetFloat("_Rez", rez);

        }

        

        private void OnDestroy()
        {
            _bufferArgumentsData.Release();
        }
    }
}

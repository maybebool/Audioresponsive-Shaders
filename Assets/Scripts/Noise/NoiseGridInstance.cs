using UnityEngine;

namespace Noise {
    public abstract class NoiseGridInstance : MonoBehaviour {
        private const string InTex = "_inTex";
        private const string Rez = "_Rez";
        

        public uint rez = 64; 
        public CustomRenderTexture inTex; 
    
        public Material outMat; 
        public Mesh iMesh;
    
        protected ComputeBuffer _bufferArgumentsData;
        protected Bounds _bounds;
    
        private void Start()
        {
            _bufferArgumentsData = new ComputeBuffer(5, sizeof(int), ComputeBufferType.IndirectArguments);
            var arguments = new uint[5];
            arguments[0] = iMesh.GetIndexCount(0);
            arguments[1] = rez * rez;
            arguments[2] = iMesh.GetIndexStart(0); 
            arguments[3] = iMesh.GetBaseVertex(0); 
            arguments[4] = 0; 
            _bufferArgumentsData.SetData(arguments); 
            _bounds = new Bounds(Vector3.zero, Vector3.one * 100);
            outMat.SetTexture(InTex, inTex);
            outMat.SetFloat(Rez, rez);
        }

        

        private void OnDestroy()
        {
            _bufferArgumentsData.Release();
        }
    }
}

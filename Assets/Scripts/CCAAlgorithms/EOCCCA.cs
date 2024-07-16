using UnityEngine;

namespace CCAAlgorithms {
    public class EOCCCA : MonoBehaviour
    {
        [Range(32, 2048)] public int rez = 128;
        [Range(0, 1)] public double lambda = .2;

        const int MAX_STATES = 15;
        [Range(1, MAX_STATES)] public int nStates = 3;

        public int seed;
        public bool symmetry = false;
        public bool randomize = false;

        [Range(0, 1024)] public int stepsPerFrame = 1;


        public ComputeShader cs;
        public Material mat;


        private RenderTexture statesT;
        private RenderTexture renderT;
        private int stepK;
        private int renderK;
        private int step;

        public bool save;

        void Start() {
            Reset();
        }

        void Update() {
            for (int i = 0; i < stepsPerFrame; i++) {
                Step();
            }
        }

        private Vector4[] GenerateTransitions(int seed) {
        
            Vector4[] t = new Vector4[MAX_STATES * MAX_STATES * MAX_STATES];
            for (int i = 0; i < t.Length; i++) {
                t[i] = Vector4.one * -1;
            }

            var rand = new System.Random(seed);

            // Outer loop is left state
            for (int l = 0; l < nStates; l++) {
                // Middle loop is self state
                for (int s = 0; s < nStates; s++) {
                    // Inner loop is right state
                    for (int r = 0; r < nStates; r++) {
                        int i = l * MAX_STATES * MAX_STATES + s * MAX_STATES + r;
                        int i2 = r * MAX_STATES * MAX_STATES + s * MAX_STATES + l;
                        if (rand.NextDouble() < lambda) {
                            t[i] = Vector4.zero;
                            // Debug.Log($"l: {l}, s: {s}, r:{r}, i: {i} ---> {t[i]}");
                            if (symmetry) {
                                t[i2] = Vector4.zero;
                            }
                        }
                        else {
                            //t[i] = rand.Next(nStates - 1);
                            t[i] = new Vector4(rand.Next(1, nStates), -1, -1, -1);
                            // Debug.Log($"l: {l}, s: {s}, r:{r}, i: {i} ---> {t[i]}");
                            if (symmetry) {
                                t[i2] = t[i];
                            }
                        }
                    }
                }
            }

            //t[0] = Vector4.zero; // no stripes
            return t;
        }
    
    
        public void Reset() {
            // Create textures
            if (!(statesT == null)) {
                statesT.Release();
            }

            if (!(renderT == null)) {
                renderT.Release();
            }

            renderT = CreateTexture(rez, FilterMode.Point, RenderTextureFormat.Default);
            statesT = CreateTexture(rez, FilterMode.Point, RenderTextureFormat.RInt);

            if (randomize) {
                seed = Time.frameCount;
            }

            step = 0;

            // Set up a transition table
            Vector4[] t = GenerateTransitions(seed);
            cs.SetVectorArray("t", t);

            // Set other compute constants
            cs.SetFloat("rez", rez);
            cs.SetInt("highState", nStates - 1);
            cs.SetInt("seed", Time.frameCount);

            // Set up and dispatch reset kernel
            int resetK = cs.FindKernel("ResetK");
            cs.SetTexture(resetK, "renderT", renderT);
            cs.SetTexture(resetK, "statesT", statesT);
            cs.Dispatch(resetK, rez / 32, rez / 32, 1);

            // Set up step kernel
            stepK = cs.FindKernel("StepK");
            cs.SetTexture(stepK, "statesT", statesT);

            // Set up render kernel
            renderK = cs.FindKernel("RenderK");
            cs.SetTexture(renderK, "statesT", statesT);
            cs.SetTexture(renderK, "renderT", renderT);

            // Assign texture to output material
            // mat.SetTexture("_UnlitColorMap", renderT);
            mat.SetTexture("_BaseMap", renderT);
        }

    
        public void Step() {
            if (step < rez) {
                cs.SetInt("step", step);
                cs.Dispatch(stepK, rez / 32, 1, 1);
                step++;
            }
            else {
            
                Reset();
            }
        
            cs.Dispatch(renderK, rez / 32, rez / 32, 1);
        }

    

        protected RenderTexture CreateTexture(int rez, FilterMode filterMode, RenderTextureFormat format) {
            RenderTexture texture = new RenderTexture(rez, rez, 1, format);

            texture.name = "Output";
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            texture.volumeDepth = 1;
            texture.filterMode = filterMode;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.autoGenerateMips = false;
            texture.useMipMap = false;
            texture.Create();

            return texture;
        }
    }
}

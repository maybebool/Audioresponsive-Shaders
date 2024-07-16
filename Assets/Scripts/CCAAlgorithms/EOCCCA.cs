using UnityEngine;
using UnityEngine.Rendering;
using Random = System.Random;

namespace CCAAlgorithms {
    public class EOCCCA : MonoBehaviour
    {

        #region Constants and Statics

        private static readonly int Step1 = Shader.PropertyToID("step");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int RenderT = Shader.PropertyToID("renderT");
        private static readonly int StatesT = Shader.PropertyToID("statesT");
        private static readonly int Seed = Shader.PropertyToID("seed");
        private static readonly int HighState = Shader.PropertyToID("highState");
        private static readonly int Rez = Shader.PropertyToID("rez");
        private static readonly int T = Shader.PropertyToID("t");

        private const string ResetK = "ResetK";
        private const string StepK = "StepK";
        private const string RenderK = "RenderK";
        const int MAX_STATES = 15;

        #endregion
        
        [Header("EOC Setup")]
        [Range(32, 2048)] public int rez = 128;
        [Range(0, 1)] public double lambda = .2;
        [Range(1, MAX_STATES)] public int nStates = 3;
        [Range(0, 1024)] public int stepsPerFrame = 1;
        [SerializeField] private int seed;
        [SerializeField] private bool symmetry;
        public bool randomize;
        public ComputeShader cs;
        public Material mat;
        
        private RenderTexture _statesT;
        private RenderTexture _renderT;
        private int _stepK;
        private int _renderK;
        private int _step;
        
        

        private void Start() {
            Reset();
        }

        private void Update() {
            for (int i = 0; i < stepsPerFrame; i++) {
                Step();
            }
        }


        /// <summary>
        /// GenerateTransitions method generates a transition table of size MAX_STATES * MAX_STATES * MAX_STATES
        /// initialized with random values based on a given seed. The method returns the generated transition table.
        /// </summary>
        /// <param name="seed">The seed used to initialize the random number generator.</param>
        /// <returns>The generated transition table.</returns>
        private Vector4[] GenerateTransitions(int seed) {
            var t = new Vector4[MAX_STATES * MAX_STATES * MAX_STATES];
            InitializeVectorArray(ref t);
            var rand = new Random(seed);
            FillTransitionTable(ref t, rand);
            return t;
        }

        /// <summary>
        /// Initializes the vector array by setting all elements to -1.
        /// </summary>
        /// <param name="t">The vector array to initialize.</param>
        private void InitializeVectorArray(ref Vector4[] t) {
            for (int i = 0; i < t.Length; i++) {
                t[i] = Vector4.one * -1;
            }
        }

        /// <summary>
        /// Fills the transition table with random values based on the given seed.
        /// </summary>
        /// <param name="t">The transition table to fill.</param>
        /// <param name="rand">The random number generator.</param>
        private void FillTransitionTable(ref Vector4[] t, Random rand) {
            for (int l = 0; l < nStates; l++) {
                for (int s = 0; s < nStates; s++) {
                    for (int r = 0; r < nStates; r++) {
                        var i = CalculateComputeIndex(l, s, r);
                        var i2 = CalculateComputeIndex(r, s, l);
                        FillTransitions(ref t, i, i2, rand);
                    }
                }
            }
        }

        private int CalculateComputeIndex(int a, int b, int c) {
            return a * MAX_STATES * MAX_STATES + b * MAX_STATES + c;
        }

        /// <summary>
        /// Fills the transition table with random values based on the given seed.
        /// </summary>
        /// <param name="t">The transition table to fill.</param>
        /// <param name="i">The first index of the transition table.</param>
        /// <param name="i2">The second index of the transition table.</param>
        /// <param name="rand">The random number generator.</param>
        private void FillTransitions(ref Vector4[] t, int i, int i2, Random rand) {
            if (rand.NextDouble() < lambda) {
                t[i] = Vector4.zero;
                if (symmetry) {
                    t[i2] = Vector4.zero;
                }
            }
            else {
                t[i] = new Vector4(rand.Next(1, nStates), -1, -1, -1);
                if (symmetry) {
                    t[i2] = t[i];
                }
            }
        }
        
        public void Reset() {
            
            if (!(_statesT == null)) {
                _statesT.Release();
            }

            if (!(_renderT == null)) {
                _renderT.Release();
            }

            _renderT = CreateTexture(rez, FilterMode.Point, RenderTextureFormat.Default);
            _statesT = CreateTexture(rez, FilterMode.Point, RenderTextureFormat.RInt);

            if (randomize) {
                seed = Time.frameCount;
            }

            _step = 0;
            
            var t = GenerateTransitions(seed);
            cs.SetVectorArray(T, t);
            
            cs.SetFloat(Rez, rez);
            cs.SetInt(HighState, nStates - 1);
            cs.SetInt(Seed, Time.frameCount);
            
            var resetK = cs.FindKernel(ResetK);
            cs.SetTexture(resetK, RenderT, _renderT);
            cs.SetTexture(resetK, StatesT, _statesT);
            cs.Dispatch(resetK, rez / 32, rez / 32, 1);
            
            _stepK = cs.FindKernel(StepK);
            cs.SetTexture(_stepK, StatesT, _statesT);

            _renderK = cs.FindKernel(RenderK);
            cs.SetTexture(_renderK, StatesT, _statesT);
            cs.SetTexture(_renderK, RenderT, _renderT);
            mat.SetTexture(BaseMap, _renderT);
        }


        /// <summary>
        /// Step method performs a single step of the EOCCCA algorithm. It dispatches the compute shader kernels to
        /// update the transition states and render the result.
        /// If the current step is less than the given resolution, it updates the transition
        /// states and increments the step count. Otherwise, it resets the algorithm and starts from the beginning.
        /// </summary>
        /// <remarks>
        /// This method should be called in the Update method or whenever a step of the algorithm needs to be performed.
        /// </remarks>
        private void Step() {
            if (_step < rez) {
                cs.SetInt(Step1, _step);
                cs.Dispatch(_stepK, rez / 32, 1, 1);
                _step++;
            }
            else {
            
                Reset();
            }
        
            cs.Dispatch(_renderK, rez / 32, rez / 32, 1);
        }


        /// <summary>
        /// CreateTexture method creates a new RenderTexture with the specified resolution, filter mode, and format.
        /// </summary>
        /// <param name="rez">The resolution of the RenderTexture.</param>
        /// <param name="filterMode">The filter mode of the RenderTexture.</param>
        /// <param name="format">The format of the RenderTexture.</param>
        /// <returns>The newly created RenderTexture.</returns>
        private RenderTexture CreateTexture(int rez, FilterMode filterMode, RenderTextureFormat format) {
            var texture = new RenderTexture(rez, rez, 1, format);
            texture.name = "Output";
            texture.enableRandomWrite = true;
            texture.dimension = TextureDimension.Tex2D;
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

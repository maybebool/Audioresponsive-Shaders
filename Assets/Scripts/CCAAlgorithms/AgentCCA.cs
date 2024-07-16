using System.Collections.Generic;
using UnityEngine;

namespace CCAAlgorithms {
    public class AgentCCA : MonoBehaviour {

        #region Constants and Statics

        private static readonly int WriteTex = Shader.PropertyToID("writeTex");
        private static readonly int Rez = Shader.PropertyToID("rez");
        private static readonly int Time1 = Shader.PropertyToID("time");
        private static readonly int AgentsBuffer = Shader.PropertyToID("agentsBuffer");
        private static readonly int StepN = Shader.PropertyToID("stepN");
        private static readonly int ReadTex = Shader.PropertyToID("readTex");
        private static readonly int TrailDecayFactor = Shader.PropertyToID("trailDecayFactor");
        private static readonly int DebugTex = Shader.PropertyToID("debugTex");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int OutTex = Shader.PropertyToID("outTex");

        private const string AgentsDebugKernel = "AgentsDebugKernel";
        private const string MoveAgentsKernel = "MoveAgentsKernel";
        private const string RenderKernel = "RenderKernel";
        private const string WriteTrailsKernel = "WriteTrailsKernel";
        private const string DiffuseTextureKernel = "DiffuseTextureKernel";
        private const string ResetTextureKernel = "ResetTextureKernel";
        private const string ResetAgentsKernel = "ResetAgentsKernel";

        #endregion
    
        public Material outMat;
        public ComputeShader cs;
    
        [Header("Trail Agent Setup")] 
        [Range(64, 1000000)] public int agentsCount = 1;
        [Range(0, 1)] public float trailDecayFactor = .9f;

        [Header(" Scene Setup")] 
        [Range(8, 2048)] [SerializeField] private int rez = 8;
        [Range(0, 50)] [SerializeField] private int stepsPerFrame = 0;
        [Range(1, 50)] [SerializeField] private int stepMod = 1;


        private RenderTexture _readTex;
        private RenderTexture _writeTex;
        private RenderTexture _outTex;
        private RenderTexture _debugTex;

        private int _agentsDebugKernel;
        private int _moveAgentsKernel;
        private int _writeTrailsKernel;
        private int _renderKernel;
        private int _diffuseTextureKernel;


        private ComputeBuffer _agentsBuffer;
        private List<ComputeBuffer> _buffers;
        private List<RenderTexture> _textures;
        private int _stepN = -1;

        void Start() {
            Reset();
        }
    
        void Update() {
            if (Time.frameCount % stepMod != 0) return;
            for (int i = 0; i < stepsPerFrame; i++) {
                Step();
            }
        }
    
        private void OnDestroy() {
            Release();
        }

        private void OnEnable() {
            Release();
        }

        private void OnDisable() {
            Release();
        }

        public void Reset() {
            _agentsDebugKernel = cs.FindKernel(AgentsDebugKernel);
            _moveAgentsKernel = cs.FindKernel(MoveAgentsKernel);
            _renderKernel = cs.FindKernel(RenderKernel);
            _writeTrailsKernel = cs.FindKernel(WriteTrailsKernel);
            _diffuseTextureKernel = cs.FindKernel(DiffuseTextureKernel);

            _readTex = CreateTexture(rez, FilterMode.Point);
            _writeTex = CreateTexture(rez, FilterMode.Point);
            _outTex = CreateTexture(rez, FilterMode.Point);
            _debugTex = CreateTexture(rez, FilterMode.Point);

            _agentsBuffer = new ComputeBuffer(agentsCount, sizeof(float) * 4);
            _buffers.Add(_agentsBuffer);

            SetupForResetKernel();
            Render();
        }


        /// <summary>
        /// Sets up the necessary parameters and resources for the <c>ResetKernel</c> in the <c>ComputeShader</c>.
        /// </summary>
        private void SetupForResetKernel() {
            int kernel;

            cs.SetInt(Rez, rez);
            cs.SetInt(Time1, Time.frameCount);

            kernel = cs.FindKernel(ResetTextureKernel);
            cs.SetTexture(kernel, WriteTex, _writeTex);
            cs.Dispatch(kernel, rez, rez, 1);
            cs.SetTexture(kernel, WriteTex, _readTex);
            cs.Dispatch(kernel, rez, rez, 1);
        
            kernel = cs.FindKernel(ResetAgentsKernel);
            cs.SetBuffer(kernel, AgentsBuffer, _agentsBuffer);
            cs.Dispatch(kernel, agentsCount / 64, 1, 1);
        }

        /// <summary>
        /// Executes a single step of the simulation by updating the necessary parameters and resources for the simulation kernels.
        /// </summary>
        private void Step() {
            _stepN += 1;
            cs.SetInt(Time1, Time.frameCount);
            cs.SetInt(StepN, _stepN);

            SetupForMoveAgentsKernel();

            if (_stepN % 2 == 1) {
                SetupForDiffuseTextureKernel();
                SetupForWriteTrailsKernel();
                SwapTex();
            }

            Render();
        }

        /// <summary>
        /// Sets up the necessary parameters and resources for the WriteTrailsKernel.
        /// </summary>
        private void SetupForWriteTrailsKernel() {
            cs.SetBuffer(_writeTrailsKernel, AgentsBuffer, _agentsBuffer);
            cs.SetTexture(_writeTrailsKernel, WriteTex, _writeTex);
            cs.Dispatch(_writeTrailsKernel, agentsCount / 64, 1, 1);
        }

        private void SwapTex() {
            (_readTex, _writeTex) = (_writeTex, _readTex);
        }

        /// <summary>
        /// Sets up the necessary parameters and resources for the RenderKernel.
        /// </summary>
        private void Render() {
            SetupForRenderKernel();
            SetupAgentsForDebugKernel();
            outMat.SetTexture(BaseMap, _outTex);
        }


        /// <summary>
        /// Sets up the necessary parameters and resources for the RenderKernel.
        /// </summary>
        private void SetupForRenderKernel() {
            cs.SetTexture(_renderKernel, ReadTex, _readTex);
            cs.SetTexture(_renderKernel, OutTex, _outTex);
            cs.SetTexture(_renderKernel, DebugTex, _debugTex);
            cs.Dispatch(_renderKernel, rez, rez, 1);
        }


        /// <summary>
        /// Sets up the necessary parameters and resources for the AgentsDebugKernel.
        /// </summary>
        private void SetupAgentsForDebugKernel() {
            cs.SetBuffer(_agentsDebugKernel, AgentsBuffer, _agentsBuffer);
            cs.SetTexture(_agentsDebugKernel, OutTex, _outTex);
            cs.Dispatch(_agentsDebugKernel, agentsCount / 64, 1, 1);
        }

        /// <summary>
        /// Sets up the necessary parameters and resources for the DiffuseTextureKernel.
        /// </summary>
        private void SetupForDiffuseTextureKernel() {
            cs.SetTexture(_diffuseTextureKernel, ReadTex, _readTex);
            cs.SetTexture(_diffuseTextureKernel, WriteTex, _writeTex);
            cs.SetFloat(TrailDecayFactor, trailDecayFactor);
            cs.Dispatch(_diffuseTextureKernel, rez, rez, 1);
        }


        /// <summary>
        /// Sets up the necessary parameters and resources for the MoveAgentsKernel.
        /// </summary>
        private void SetupForMoveAgentsKernel() {
            cs.SetBuffer(_moveAgentsKernel, AgentsBuffer, _agentsBuffer);
            cs.SetTexture(_moveAgentsKernel, ReadTex, _readTex);
            cs.SetTexture(_moveAgentsKernel, DebugTex, _debugTex);
            cs.Dispatch(_moveAgentsKernel, agentsCount / 64, 1, 1);
        }


        /// <summary>
        /// Releases all the ComputeBuffers and RenderTextures used by the AgentCCA class.
        /// </summary>
        private void Release() {
            if (_buffers != null) {
                foreach (ComputeBuffer buffer in _buffers) {
                    if (buffer != null) {
                        buffer.Release();
                    }
                }
            }

            _buffers = new List<ComputeBuffer>();

            if (_textures != null) {
                foreach (RenderTexture tex in _textures) {
                    if (tex != null) {
                        tex.Release();
                    }
                }
            }

            _textures = new List<RenderTexture>();
        }

        /// <summary>
        /// Creates a new RenderTexture with the specified dimensions and filter mode.
        /// </summary>
        /// <param name="r">The width and height of the RenderTexture.</param>
        /// <param name="filterMode">The filter mode to be applied to the texture.</param>
        /// <returns>The created RenderTexture.</returns>
        private RenderTexture CreateTexture(int r, FilterMode filterMode) {
            var texture = new RenderTexture(r, r, 1, RenderTextureFormat.ARGBFloat);

            texture.name = "out";
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            texture.volumeDepth = 1;
            texture.filterMode = filterMode;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.autoGenerateMips = false;
            texture.useMipMap = false;
            texture.Create();
            _textures.Add(texture);

            return texture;
        }
    }
}
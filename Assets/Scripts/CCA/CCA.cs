using UnityEngine;

public class CCA : MonoBehaviour {
    
    #region Constants

    private const string StepKernel = "StepKernel";
    private const string ResetKernel = "ResetKernel";
    private const string BaseMap = "_BaseMap";
    private const string WriteTex = "writeTex";
    private const string ReadTex = "readTex";
    private const string OutTex = "outTex";
    private const string Rez = "rez";
    private const string Range = "range";
    private const string Threshold = "threshold";
    private const string NStates = "nStates";
    private const string MooreAlgo = "mooreAlgo";
    private const string Colors = "colors";
    private const int MAX_RANGE = 10;
    private const int MAX_THRESHOLD = 25;
    private const int MAX_STATES = 20;

    #endregion

    [Header("CCA Params")]
    
    [Range(1, MAX_RANGE)] [SerializeField] private int range = 2;
    [Range(0, MAX_THRESHOLD)] [SerializeField] private int threshold = 8;
    [Range(1, MAX_STATES)] [SerializeField] private int nStates = 4;
    [Range(1, 50)] [SerializeField] private int stepsPerFrame = 1;
    [Range(1, 9)] [SerializeField] private int stepMod = 1;
    [SerializeField] private bool mooreAlgo;

    [Header("Setup")] 
    [Range(8, 6144)] [SerializeField] private int rez;
    [SerializeField] private ComputeShader cs;
    [SerializeField] private Material outMaterial;
    
    private RenderTexture _outTex;
    private RenderTexture _readTex;
    private RenderTexture _writeTex;
    private int _stepKernel;
    private System.Random _randColor;
    private Gradient _gColor = new();
    private Color _nc;
    private int _keycount = 8;
    private GradientColorKey[] _c;
    private GradientAlphaKey[] _a;
    private float _hueMax = 1f;
    private float _hueMin = 0.9f;
    private float _sMax = 2;
    private float _sMin = 0;
    private float _vMax = 1;
    private float _vMin = 0;
    
    private void Update() {
        if (Time.frameCount % stepMod == 0) {
            for (int i = 0; i < stepsPerFrame; i++) {
                Step();
            }
        }
    }

    private void Start() {
        _nc = new Color();
        _randColor = new System.Random();
        _c = new GradientColorKey[_keycount];
        _a = new GradientAlphaKey[_keycount];
        Reset();
        SetColors();
    }


    /// <summary>
    /// Reset the compute shader kernel to its initial state.
    /// </summary>
    public void Reset() {
        _readTex = CreateTexture(RenderTextureFormat.RFloat);
        _writeTex = CreateTexture(RenderTextureFormat.RFloat);
        _outTex = CreateTexture(RenderTextureFormat.ARGBFloat);
        _stepKernel = cs.FindKernel(StepKernel);
        GPUResetKernel();
    }

    /// <summary>
    /// Create a RenderTexture with the specified format and size, used for CCA2D object.
    /// </summary>
    /// <param name="format">The format of the RenderTexture</param>
    /// <returns>The created RenderTexture</returns>
    private RenderTexture CreateTexture(RenderTextureFormat format) {
        var texture = new RenderTexture(rez, rez, 1, format);
        texture.enableRandomWrite = true;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.useMipMap = false;
        texture.Create();
        return texture;
    }

   
    /// /// <summary>
    /// Reset the compute shader kernel to its initial state.
    /// </summary>
    private void GPUResetKernel() {
        var k = cs.FindKernel(ResetKernel);
        cs.SetTexture(k, WriteTex, _writeTex);

        cs.SetInt(Range, range);
        cs.SetInt(Threshold, threshold);
        cs.SetInt(NStates, nStates);
        cs.SetBool(MooreAlgo, mooreAlgo);

        cs.SetInt(Rez, rez);
        cs.Dispatch(k, rez, rez, 1);

        SwapTex();
    }

    /// <summary>
    /// Randomize the parameters of the Cellular Automata algorithm.
    /// </summary>
    private void RandomizeParams() {
        var rand = new System.Random();
        range = (int)(rand.NextDouble() * (MAX_RANGE - 1)) + 1;
        threshold = (int)(rand.NextDouble() * MAX_THRESHOLD - 1) + 1;
        nStates = (int)(rand.NextDouble() * (MAX_STATES - 2)) + 2;
        mooreAlgo = rand.NextDouble() <= 0.5;
    
        cs.SetInt(Range, range);
        cs.SetInt(Threshold, threshold);
        cs.SetInt(NStates, nStates);
        cs.SetBool(MooreAlgo, mooreAlgo);
    }


    public void ResetAndRandomize() {
        RandomizeParams();
        Reset();
    }

    /// <summary>
    /// Set the colors for the CCA based on random values within specified ranges.
    /// </summary>
    public void SetColors() {
        var rand = new System.Random(Time.frameCount);
    
        for (int x = 0; x < _keycount; x++) {
            var h = (float)rand.NextDouble() * (_hueMax - _hueMin) + _hueMin;
            float s = (float)rand.NextDouble() * (_sMax - _sMin) + _sMin;
            float v = (float)rand.NextDouble() * (_vMax - _vMin) + _vMin;
            _nc = Color.HSVToRGB(h, s, v);
            _c[x].color = _nc;
            _a[x].time = _c[x].time = (x * (1.0f / _keycount));
            _a[x].alpha = 1.0f;
        }
    
        _gColor.SetKeys(_c, _a);
        var colors = new Vector4[nStates];
        for (int j = 0; j < nStates; j++) {
            var t = (float)_randColor.NextDouble();
            colors[j] = _gColor.Evaluate(t);
        }
    
        cs.SetVectorArray(Colors, colors);
    }

    /// <summary>
    /// Perform a single step of the Cellular Automata algorithm.
    /// </summary>
    public void Step() {
        cs.SetTexture(_stepKernel, ReadTex, _readTex);
        cs.SetTexture(_stepKernel, WriteTex, _writeTex);
        cs.SetTexture(_stepKernel, OutTex, _outTex);
        cs.Dispatch(_stepKernel, rez / 16, rez / 16, 1);

        SwapTex();

        outMaterial.SetTexture(BaseMap, _outTex);
    }

    private void SwapTex() {
        (_readTex, _writeTex) = (_writeTex, _readTex);
    }
}
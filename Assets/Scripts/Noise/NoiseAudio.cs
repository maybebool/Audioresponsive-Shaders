using System;
using AudioAnalysis;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

namespace Noise {
    public class NoiseAudio : NoiseGridInstance {
        #region strings

        private const string SaturationValue = "_SaturationValue";
        private const string ComputeLerpSaturationValue = "computeLerpSaturationValue";
        private const string HueSpeed = "_Hue_Speed";
        private const string CellAmount = "_CellAmount";
        private const string MinMaxSaturation = "_minMaxSaturation";
        private const string AmplitudeBuffer = "amplitudeBuffer";
        private const string UseSaturation = "useSaturation";
        private const string Threshold = "threshold";
        #endregion

        [SerializeField] private AudioData audioData;
        [SerializeField] private Material audioMatValues;
        [SerializeField] public bool useSaturation;
        [SerializeField] private bool useHueFloating;
        [SerializeField] private bool useCellAmount;
        public ComputeShader audioCalcCompShader;
        public Vector2 _minMaxSaturation;
        private float saturationValue;
        public float threshold = 1.0f;
        public float thresholdHue = 1.0f;
        public float thresholdCells = 1.0f;

        public float computeLerpSaturationValue;
        

        
        
        private void Update()
        {
            SetComputeParameters();
            audioCalcCompShader.Dispatch(0,8,1,1);
            Graphics.DrawMeshInstancedIndirect(iMesh, 0, outMat, _bounds, _bufferArgumentsData);
        }


        private void SetComputeParameters() {
            // var lerpBuffer = new ComputeBuffer(128, sizeof(float));
            // audioCalcCompShader.SetBuffer(0,"LerpedValue", lerpBuffer);
            // audioCalcCompShader.SetFloat(AmplitudeBuffer, audioData.amplitudeBuffer);
            // audioCalcCompShader.SetFloat(Threshold, threshold);
            // audioCalcCompShader.SetFloat(ComputeLerpSaturationValue, computeLerpSaturationValue);
            // audioCalcCompShader.SetBool(UseSaturation, useSaturation);
            // //audioCalcCompShader.SetFloat(SaturationValue, useSaturation ? audioMatValues.GetFloat(SaturationValue) : 0);
            // audioCalcCompShader.SetVector(MinMaxSaturation, _minMaxSaturation);
            if (useSaturation) {
                if (audioData.amplitudeBuffer < threshold) {
                    var lerpSaturationValue = Mathf.Lerp(_minMaxSaturation.x, _minMaxSaturation.y, audioData.amplitudeBuffer);
                    audioMatValues.SetFloat(SaturationValue, lerpSaturationValue);
                }
            }

            // audioMatValues.SetFloat(SaturationValue, computeLerpSaturationValue);
            // if (useHueFloating) {
            //     if (audioData.amplitudeBuffer < thresholdHue) {
            //         var lerpHueValue = Mathf.Lerp(90, 91, audioData.amplitudeBuffer);
            //         audioMatValues.SetFloat(HueSpeed, lerpHueValue);  
            //     }
            //     else {
            //         audioMatValues.SetFloat(HueSpeed, 88);  
            //     }
            // }
            
            // Debug.Log(audioData.amplitudeBuffer);
            if (useCellAmount) {
                if (audioData.amplitudeBuffer < thresholdCells) {
                    var lerpCells = Mathf.Lerp(5, 6, audioData.amplitudeBuffer);
                    audioMatValues.SetFloat(CellAmount, lerpCells);  
                }
            }
        }
    }
}
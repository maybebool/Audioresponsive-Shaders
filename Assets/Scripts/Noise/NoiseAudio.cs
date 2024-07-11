using System;
using AudioAnalysis;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

namespace Noise {
    public class NoiseAudio : NoiseGridInstance {
        #region strings

        private const string SaturationValue = "_SaturationValue";
        private const string HueSpeed = "_Hue_Speed";
        private const string CellAmount = "_CellAmount";
        private const string MinMaxSaturation = "_minMaxSaturation";
        private const string AmplitudeBuffer = "amplitudeBuffer";
        #endregion

        [SerializeField] private AudioData audioData;
        [SerializeField] private Material audioMatValues;
        [SerializeField] private bool useSaturation;
        [SerializeField] private bool useHueFloating;
        [SerializeField] private bool useCellAmount;
        //public ComputeShader audioCalcCompShader;
        public Vector2 _minMaxSaturation;
        private float saturationValue;
        public float threshold = 1.0f;
        public float thresholdHue = 1.0f;
        public float thresholdCells = 1.0f;
        

        
        
        private void Update()
        {
            SetComputeParameters();
            // audioCalcCompShader.Dispatch(0,8,1,1);
            Graphics.DrawMeshInstancedIndirect(iMesh, 0, outMat, _bounds, _bufferArgumentsData);
        }


        private void SetComputeParameters() {
            
            // audioCalcCompShader.SetFloat(AmplitudeBuffer, audioData.amplitudeBuffer);
            // audioCalcCompShader.SetFloat(SaturationValue, useSaturation ? audioMatValues.GetFloat(SaturationValue) : 0);
            // audioCalcCompShader.SetVector(MinMaxSaturation, _minMaxSaturation);
            if (useSaturation) {
                if (audioData.amplitudeBuffer > threshold) {
                    
                }
                var lerpSaturationValue = Mathf.Lerp(0, 2, audioData.amplitudeBuffer);
                audioMatValues.SetFloat(SaturationValue, lerpSaturationValue);
            }

            // if (useHueFloating) {
            //     if (audioData.amplitudeBuffer < thresholdHue) {
            //         var lerpHueValue = Mathf.Lerp(90, 100, audioData.amplitudeBuffer);
            //         audioMatValues.SetFloat(HueSpeed, lerpHueValue);  
            //     }
            //     else {
            //         audioMatValues.SetFloat(HueSpeed, 90);  
            //     }
            // }
            
            Debug.Log(audioData.amplitudeBuffer);
            if (useCellAmount) {
                if (audioData.amplitudeBuffer < thresholdCells) {
                    var lerpCells = Mathf.Lerp(5, 6, audioData.amplitudeBuffer);
                    audioMatValues.SetFloat(CellAmount, lerpCells);  
                }
            }
        }
    }
}
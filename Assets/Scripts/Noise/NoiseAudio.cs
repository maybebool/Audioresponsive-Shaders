using AudioAnalysis;
using UnityEngine;

namespace Noise {
    public class NoiseAudio : NoiseGridInstance {
        
        private const string SaturationValue = "_SaturationValue";
        private const string AmplitudeValue = "_Amplitude";

        [SerializeField] private AudioData audioData;
        [SerializeField] private Material audioMatValues;
        [SerializeField] public bool useSaturation;
        [SerializeField] public bool useAmplitude;
        [SerializeField] private Vector2 minMaxSaturation;
        [SerializeField] private Vector2 minMaxAmplitude;
        private float _saturationValue;
        public float threshold = 1.0f;
        

        
        
        private void Update()
        {
            SetAudioReactiveParameters();
            Graphics.DrawMeshInstancedIndirect(iMesh, 0, outMat, _bounds, _bufferArgumentsData);
        }


        private void SetAudioReactiveParameters() {
            if (useSaturation) {
                if (audioData.amplitudeBuffer > threshold) {
                    var lerpSaturationValue = Mathf.Lerp(minMaxSaturation.x, minMaxSaturation.y, audioData.amplitudeBuffer);
                    audioMatValues.SetFloat(SaturationValue, lerpSaturationValue);
                }
            }
            else {
                audioMatValues.SetFloat(SaturationValue, 0f);
            }
            if (useAmplitude) {
                if (audioData.amplitudeBuffer > threshold) {
                    var lerpAmplitudeValue = Mathf.Lerp(minMaxAmplitude.x, minMaxAmplitude.y, audioData.amplitudeBuffer);
                    outMat.SetFloat(AmplitudeValue, lerpAmplitudeValue);
                }
            }else {
                outMat.SetFloat(AmplitudeValue, 0f);
            }
            
        }
    }
}
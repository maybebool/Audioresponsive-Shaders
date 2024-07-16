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
            
            // main idea from this source https://www.youtube.com/watch?v=eyaxqo9JV4w
            Graphics.DrawMeshInstancedIndirect(iMesh, 0, outMat, _bounds, _bufferArgumentsData);
        }


        /// <summary>
        /// Sets the audio reactive parameters for the NoiseAudio instance.
        /// </summary>
        /// <remarks>
        /// This method is called in the Update method and updates the audio reactive parameters based on the audio data and user-defined settings.
        /// </remarks>
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
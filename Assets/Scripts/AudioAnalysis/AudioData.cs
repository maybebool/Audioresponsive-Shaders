using UnityEngine;

namespace AudioAnalysis {
    public class SpectrumBandData {
        public float[] FrequencyBand { get; } = new float[8];
        public float[] BandBuffer { get; } = new float[8];
        public float[] BufferReduction { get; } = new float[8];
        public float[] FrequencyBandHighest { get; } = new float[8];
    }

    [RequireComponent(typeof(AudioSource))]
    public class AudioData : MonoBehaviour {
        private SpectrumBandData _spectrumBandData = new();
        [SerializeField] private int bandCount = 8;
        [HideInInspector] public float[] audioBand;
        [HideInInspector] public float[] audioBandBuffer;
        [HideInInspector] public float amplitude;
        [HideInInspector] public float amplitudeBuffer;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioClip;
        private float[] _samples = new float[512];
        private float _amplitudeHighest;
        private float _audioProfile;


        private void Start() {
            _audioProfile = 0.5f;
            audioBand = new float[bandCount];
            audioBandBuffer = new float[bandCount];
            AudioProfile(_audioProfile);
            audioSource.clip = audioClip;
            audioSource.Play();
        }


        private void Update() {
            if (audioSource.clip == null) return;
            GetAudioSpectrumData();
            GenerateFrequencyFilters();
            CalculateBandBuffer();
            GenerateAudioBands();
            GetAmplitude();
        }

        private void GetAudioSpectrumData() {
            audioSource.GetSpectrumData(_samples, 0, FFTWindow.BlackmanHarris);
        }

        private void AudioProfile(float audioProfile) {
            for (int i = 0; i < bandCount; i++) {
                _spectrumBandData.FrequencyBandHighest[i] = audioProfile;
            }
        }

        /// <summary>
        /// Calculates the amplitude of the audio, based on the audio bands.
        /// </summary>
        /// <remarks>
        /// This method calculates the amplitude of the audio by summing up the values of the audio bands.
        /// It also calculates the amplitude buffer, which is a smoothed version of the amplitude.
        /// The resulting amplitudes are normalized between 0 and 1.
        /// </remarks>
        private void GetAmplitude() {
            float currentAmplitude = 0;
            float currentAmplitudeBuffer = 0;
            for (int i = 0; i < bandCount; i++) {
                currentAmplitude += audioBand[i];
                currentAmplitudeBuffer += audioBandBuffer[i];
            }

            if (currentAmplitude > _amplitudeHighest) {
                _amplitudeHighest = currentAmplitude;
            }

            amplitude = currentAmplitude / _amplitudeHighest;
            amplitudeBuffer = currentAmplitudeBuffer / _amplitudeHighest;
        }

        /// <summary>
        /// Generates audio bands based on the spectrum band data.
        /// </summary>
        /// <remarks>
        /// This method iterates through each band and calculates the audio band
        /// value by calling the CalculateBand method with the appropriate band index.
        /// The calculated audio band values are stored in the audioBand array.
        /// The audio band buffer values are stored in the audioBandBuffer array.
        /// </remarks>
        private void GenerateAudioBands() {
            for (int i = 0; i < bandCount; i++) {
                UpdateHighestFrequency(i);
                audioBand[i] = CalculateBand(i, _spectrumBandData.FrequencyBand);
                audioBandBuffer[i] = CalculateBand(i, _spectrumBandData.BandBuffer);
            }
        }

        /// <summary>
        /// Updates the highest frequency value for a given band index.
        /// </summary>
        /// <param name="i">The index of the band.</param>
        private void UpdateHighestFrequency(int i) {
            if (_spectrumBandData.FrequencyBand[i] > _spectrumBandData.FrequencyBandHighest[i]) {
                _spectrumBandData.FrequencyBandHighest[i] = _spectrumBandData.FrequencyBand[i];
            }
        }

        private float CalculateBand(int i, float[] band) {
            return Mathf.Clamp((band[i] / _spectrumBandData.FrequencyBandHighest[i]), 0, 1);
        }


        /// <summary>
        /// Calculates the band buffer for each audio band based on the spectrum band data.
        /// </summary>
        /// <remarks>
        /// This method iterates through each band and calculates the band buffer value
        /// by comparing the current frequency band with the previous band buffer value.
        /// If the current frequency band is greater than the previous band buffer value,
        /// the band buffer is updated with the current frequency band.
        /// If the current frequency band is less than the band buffer and greater than zero,
        /// a buffer reduction is calculated and subtracted from the band buffer.
        /// The band buffer values are stored in the SpectrumBandData object.
        /// </remarks>
        private void CalculateBandBuffer() {
            for (int i = 0; i < bandCount; ++i) {
                var frequencyBand = _spectrumBandData.FrequencyBand[i];
                var bandBuffer = _spectrumBandData.BandBuffer[i];

                if (frequencyBand > bandBuffer) {
                    _spectrumBandData.BandBuffer[i] = frequencyBand;
                }

                if (!(frequencyBand < bandBuffer) || !(frequencyBand > 0)) continue;
                var bufferReduction = (bandBuffer - frequencyBand) / bandCount;
                _spectrumBandData.BufferReduction[i] = bufferReduction;
                _spectrumBandData.BandBuffer[i] -= bufferReduction;
            }
        }


        /// <summary>
        /// Generates frequency filters based on the audio spectrum data.
        /// </summary>
        /// <remarks>
        /// This method calculates the frequency filters that are used in audio analysis.
        /// It iterates through each band and calculates the frequency band value by calling the CalculateSampleAverage method.
        /// The calculated frequency band values are stored in the SpectrumBandData object.
        /// </remarks>
        private void GenerateFrequencyFilters() {
            var count = 0;
            for (int i = 0; i < bandCount; i++) {
                var sampleCount = (int)Mathf.Pow(2, i) * 2;
                if (i == 7) {
                    sampleCount += 2;
                }
                _spectrumBandData.FrequencyBand[i] = CalculateSampleAverage(i, count, sampleCount) * 10;
                count += sampleCount;
            }
        }

        /// <summary>
        /// Calculates the average value of the audio samples within a specified range.
        /// </summary>
        /// <param name="i">The index of the frequency band.</param>
        /// <param name="start">The starting index of the audio samples.</param>
        /// <param name="sampleCount">The number of audio samples to consider.</param>
        /// <returns>The average value of the audio samples within the specified range.</returns>
        private float CalculateSampleAverage(int i, int start, int sampleCount) {
            float total = 0;
            for (int j = start; j < start + sampleCount; j++) {
                total += (_samples[j] + _samples[j]) * (j + 1);
            }
            return total / sampleCount;
        }
    }
}
using UnityEngine;
using UnityEngine.Serialization;

namespace AudioAnalysis {
	
	[RequireComponent (typeof (AudioSource))]
	public class AudioData : MonoBehaviour {
		[HideInInspector] public float[] audioBand;
		[HideInInspector] public float[] audioBandBuffer;
		[HideInInspector] public float amplitude;
		[HideInInspector] public float amplitudeBuffer;
		
		[SerializeField] private AudioSource audioSource;
		[SerializeField] private AudioClip audioClip;
		
		//FFT based values for Audio transformation
		private float[] _samples = new float[512];
		private float[] _frequencyBand = new float[8];
		private float[] _bandBuffer = new float[8];
		private float[] _bufferReduction = new float[8];
		private float[] _frequencyBandHighest = new float[8];

		private float _AmplitudeHighest;
		private float _audioProfile;


		private void Start() {
			_audioProfile = 0.5f;
			audioBand = new float[8];
			audioBandBuffer = new float[8];
			AudioProfile(_audioProfile);
			audioSource.clip = audioClip;
			audioSource.Play();
		}


		private void Update() {
			if (audioSource.clip == null) return;
			GetAudioSpectrumData();
			GenerateFrequencyFilters();
			BandBuffer();
			GenerateAudioBands();
			GetAmplitude();
		}

		private void GetAudioSpectrumData() {
			audioSource.GetSpectrumData(_samples, 0, FFTWindow.BlackmanHarris);
		}

		private void AudioProfile(float audioProfile) {
			for (int i = 0; i < 8; i++) {
				_frequencyBandHighest[i] = audioProfile;
			}
		}

		private void GetAmplitude() {
			float currentAmplitude = 0;
			float currentAmplitudeBuffer = 0;
			for (int i = 0; i < 8; i++) {
				currentAmplitude += audioBand[i];
				currentAmplitudeBuffer += audioBandBuffer[i];
			}

			if (currentAmplitude > _AmplitudeHighest) {
				_AmplitudeHighest = currentAmplitude;
			}

			amplitude = currentAmplitude / _AmplitudeHighest;
			amplitudeBuffer = currentAmplitudeBuffer / _AmplitudeHighest;
		}

		private void GenerateAudioBands() {
			for (int i = 0; i < 8; i++) {
				if (_frequencyBand[i] > _frequencyBandHighest[i]) {
					_frequencyBandHighest[i] = _frequencyBand[i];
				}

				audioBand[i] = Mathf.Clamp((_frequencyBand[i] / _frequencyBandHighest[i]), 0, 1);
				audioBandBuffer[i] = Mathf.Clamp((_bandBuffer[i] / _frequencyBandHighest[i]), 0, 1);
			}
		}
		

		private void BandBuffer() {
			for (int i = 0; i < 8; ++i) {
				if (_frequencyBand[i] > _bandBuffer[i]) {
					_bandBuffer[i] = _frequencyBand[i];
					//_bufferDecrease [g] = 0.005f;
				}

				if (_frequencyBand[i] < _bandBuffer[i] && _frequencyBand[i] > 0) {
					_bufferReduction[i] = (_bandBuffer[i] - _frequencyBand[i]) / 8;
					_bandBuffer[i] -= _bufferReduction[i];

				}
			}
		}


		private void GenerateFrequencyFilters() {
			int count = 0;

			for (int i = 0; i < 8; i++) {


				float average = 0;
				int sampleCount = (int)Mathf.Pow(2, i) * 2;

				if (i == 7) {
					sampleCount += 2;
				}

				for (int j = 0; j < sampleCount; j++) {
					average += (_samples [count] + _samples [count]) * (count + 1);
					count++;

				}

				average /= count;
				_frequencyBand[i] = average * 10;

			}
		}
	}
}

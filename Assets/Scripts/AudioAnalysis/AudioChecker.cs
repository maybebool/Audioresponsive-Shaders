using UnityEngine;

namespace AudioAnalysis {
	public class AudioChecker : MonoBehaviour {
		AudioSource _audioSource;

		//FFT based values for Audio transformation
		private float[] _samples = new float[512];
		

		private float[] _frequencyBand = new float[8];
		private float[] _bandBuffer = new float[8];
		private float[] _bufferReduction = new float[8];
		private float[] _frequencyBandHighest = new float[8];

		[HideInInspector] public float[] _audioBand, _audioBandBuffer;

		[HideInInspector] public float _Amplitude, _AmplitudeBuffer;
		private float _AmplitudeHighest;
		private float _audioProfile;


		private void Start() {
			_audioProfile = 0.5f;
			_audioBand = new float[8];
			_audioBandBuffer = new float[8];
			_audioSource = GetComponent<AudioSource>();
			AudioProfile(_audioProfile);
			
			_audioSource.Play();
		}


		private void Update() {
			if (_audioSource.clip == null) return;
			GetAudioSpectrumData();
			GenerateFrequencyFilters();
			BandBuffer();
			GenerateAudioBands();
			GetAmplitude();
		}

		private void GetAudioSpectrumData() {
			_audioSource.GetSpectrumData(_samples, 0, FFTWindow.BlackmanHarris);
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
				currentAmplitude += _audioBand[i];
				currentAmplitudeBuffer += _audioBandBuffer[i];
			}

			if (currentAmplitude > _AmplitudeHighest) {
				_AmplitudeHighest = currentAmplitude;
			}

			_Amplitude = currentAmplitude / _AmplitudeHighest;
			_AmplitudeBuffer = currentAmplitudeBuffer / _AmplitudeHighest;
		}

		private void GenerateAudioBands() {
			for (int i = 0; i < 8; i++) {
				if (_frequencyBand[i] > _frequencyBandHighest[i]) {
					_frequencyBandHighest[i] = _frequencyBand[i];
				}

				_audioBand[i] = Mathf.Clamp((_frequencyBand[i] / _frequencyBandHighest[i]), 0, 1);
				_audioBandBuffer[i] = Mathf.Clamp((_bandBuffer[i] / _frequencyBandHighest[i]), 0, 1);
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
					count++;

				}

				average /= count;
				_frequencyBand[i] = average * 10;

			}
		}
	}
}

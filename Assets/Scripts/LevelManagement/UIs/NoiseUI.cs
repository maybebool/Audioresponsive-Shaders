using LevelManagement;
using LevelManagement.Scenes;
using Noise;
using UnityEngine;
using UnityEngine.UI;

public class NoiseUI : MonoBehaviour
{
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private Button backToSceneButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject panel;
    [SerializeField] private NoiseAudio noiseScript;
    [SerializeField] private Toggle useAmplitude;
    [SerializeField] private Toggle useSaturation;
    

    private void OnEnable() {
        backToMenuButton.onClick.AddListener(OnClickBackToMenu);
        useAmplitude.onValueChanged.AddListener(OnClickUseAmplitudeToggle);
        useSaturation.onValueChanged.AddListener(OnClickUseSaturationToggle);
        settingsButton.onClick.AddListener(OnClickPanelButton);
        backToSceneButton.onClick.AddListener(OnClickBackToSceneButton);
            
            
    }

    private void OnDisable() {
        backToMenuButton.onClick.RemoveListener(OnClickBackToMenu);
        useAmplitude.onValueChanged.RemoveListener(OnClickUseAmplitudeToggle);
        useSaturation.onValueChanged.RemoveListener(OnClickUseSaturationToggle);
        settingsButton.onClick.RemoveListener(OnClickPanelButton);
        backToSceneButton.onClick.RemoveListener(OnClickBackToSceneButton);
            
    }
        
    private void OnClickBackToMenu() {
        ScenesManager.Instance.LoadMainMenu();
    }

    private void OnClickUseSaturationToggle(bool value) {
        noiseScript.useSaturation = value;
    }

    private void OnClickUseAmplitudeToggle(bool value) {
        noiseScript.useAmplitude = value;
    }

    private void OnClickPanelButton() {
        panel.SetActive(true);
    }

    private void OnClickBackToSceneButton() {
        panel.SetActive(false);
    }
    
}

using Flocking;
using UnityEngine;
using UnityEngine.UI;

namespace LevelManagement.UIs {
    public class FlockingUI : MonoBehaviour {
        [SerializeField] private FlockingBehaviour flockingScript;
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Toggle toggleSmoothness;
        [SerializeField] private Toggle toggleScale;
        [SerializeField] private Toggle toggleSpeed;
        [SerializeField] private GameObject panel;
        [SerializeField] private Button backToSceneButton;
      

        private void OnEnable() {
            backToMenuButton.onClick.AddListener(OnClickBackToMenu);
            toggleSmoothness.onValueChanged.AddListener(OnClickToggleSmoothness);
            toggleScale.onValueChanged.AddListener(OnClickToggleScale);
            toggleSpeed.onValueChanged.AddListener(OnClickSpeed);
            settingsButton.onClick.AddListener(OnCLickSettingsButton);
            backToSceneButton.onClick.AddListener(OnCLickBackButton);
            
        }

        private void OnDisable() {
            backToMenuButton.onClick.RemoveListener(OnClickBackToMenu);
            toggleSmoothness.onValueChanged.RemoveListener(OnClickToggleSmoothness);
            toggleScale.onValueChanged.RemoveListener(OnClickToggleScale);
            toggleSpeed.onValueChanged.RemoveListener(OnClickSpeed);
            settingsButton.onClick.RemoveListener(OnCLickSettingsButton);
            backToSceneButton.onClick.RemoveListener(OnCLickBackButton);
            
        }
        
        
        private void OnClickBackToMenu() {
            ScenesManager.Instance.LoadMainMenu();
        }

        private void OnClickToggleSmoothness(bool value) {
            flockingScript.useMaterialSmoothness = value;
        }

        private void OnClickToggleScale(bool value) {
            flockingScript.useScale = value;
        }

        private void OnClickSpeed(bool value) {
            flockingScript.useAudioBasedSpeed = value;
        }

        private void OnCLickSettingsButton() {
            panel.SetActive(true);
        }

        private void OnCLickBackButton() {
            panel.SetActive(false);
        }
    }
}

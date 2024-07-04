using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LevelManagement.UIs {
    public class MainMenuUI : MonoBehaviour {

        [SerializeField] private Button boidsLevelButton;
        [SerializeField] private Button ccaLevelButton;
        [SerializeField] private Button quitApplicationButton;
        private void OnEnable() {
            boidsLevelButton.onClick.AddListener(OnBoidsLevelButtonClicked);
            quitApplicationButton.onClick.AddListener(OnApplicationQuit);
            ccaLevelButton.onClick.AddListener(OnCCALevelButtonClicked);
            
        }

        private void OnDisable() {
            boidsLevelButton.onClick.RemoveListener(OnBoidsLevelButtonClicked);
            quitApplicationButton.onClick.RemoveListener(OnApplicationQuit);
            ccaLevelButton.onClick.RemoveListener(OnCCALevelButtonClicked);
        }
        
        
        private void OnBoidsLevelButtonClicked() {
            ScenesManager.Instance.LoadScene(Scene.Flocking);
        }
        
        private void OnCCALevelButtonClicked() {
            ScenesManager.Instance.LoadScene(Scene.CCA);
        }
        
        private void OnApplicationQuit() {
            ScenesManager.Instance.QuitGame();
        }
    }
}

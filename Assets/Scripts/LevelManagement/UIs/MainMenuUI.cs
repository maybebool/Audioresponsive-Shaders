using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LevelManagement.UIs {
    public class MainMenuUI : MonoBehaviour {

        [SerializeField] private Button boidsLevelButton;
        [SerializeField] private Button quitApplicationButton;
        private void OnEnable() {
            boidsLevelButton.onClick.AddListener(OnBoidsLevelButtonClicked);
            quitApplicationButton.onClick.AddListener(OnApplicationQuit);
            
        }

        private void OnDisable() {
            boidsLevelButton.onClick.RemoveListener(OnBoidsLevelButtonClicked);
            quitApplicationButton.onClick.RemoveListener(OnApplicationQuit);
        }
        
        
        private void OnBoidsLevelButtonClicked() {
            ScenesManager.Instance.LoadScene(Scene.Flocking);
        }
        
        private void OnApplicationQuit() {
            ScenesManager.Instance.QuitGame();
        }
    }
}

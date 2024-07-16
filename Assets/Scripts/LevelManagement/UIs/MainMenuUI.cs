using LevelManagement.Scenes;
using UnityEngine;
using UnityEngine.UI;

namespace LevelManagement.UIs {
    public class MainMenuUI : MonoBehaviour {

        [SerializeField] private Button boidsLevelButton;
        [SerializeField] private Button noiseLevelButton;
        [SerializeField] private Button ccaLevelButton;
        [SerializeField] private Button eocLevelButton;
        [SerializeField] private Button agentsLevelButton;
        [SerializeField] private Button quitApplicationButton;
        private void OnEnable() {
            boidsLevelButton.onClick.AddListener(OnBoidsLevelButtonClicked);
            noiseLevelButton.onClick.AddListener(OnNoiseLevelButtonClicked);
            ccaLevelButton.onClick.AddListener(OnCCALevelButtonClicked);
            eocLevelButton.onClick.AddListener(OnEOCLevelButtonClicked);
            agentsLevelButton.onClick.AddListener(OnAgentsButtonClicked);
            quitApplicationButton.onClick.AddListener(OnApplicationQuit);
            
        }

        private void OnDisable() {
            boidsLevelButton.onClick.RemoveListener(OnBoidsLevelButtonClicked);
            noiseLevelButton.onClick.RemoveListener(OnNoiseLevelButtonClicked);
            ccaLevelButton.onClick.RemoveListener(OnCCALevelButtonClicked);
            eocLevelButton.onClick.RemoveListener(OnEOCLevelButtonClicked);
            agentsLevelButton.onClick.RemoveListener(OnAgentsButtonClicked);
            quitApplicationButton.onClick.RemoveListener(OnApplicationQuit);
        }
        
        
        private void OnBoidsLevelButtonClicked() {
            ScenesManager.Instance.LoadScene(Scene.Flocking);
        }
        
        private void OnNoiseLevelButtonClicked() {
            ScenesManager.Instance.LoadScene(Scene.Noise);
        }

        private void OnCCALevelButtonClicked() {
            ScenesManager.Instance.LoadScene(Scene.CCA);
        }

        private void OnEOCLevelButtonClicked() {
            ScenesManager.Instance.LoadScene(Scene.EOC);
        }

        private void OnAgentsButtonClicked() {
            ScenesManager.Instance.LoadScene(Scene.AgentCCA);
        }
        
        private void OnApplicationQuit() {
            ScenesManager.Instance.QuitGame();
        }
    }
}

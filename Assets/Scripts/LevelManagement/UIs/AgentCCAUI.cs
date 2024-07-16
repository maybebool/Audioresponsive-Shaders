using CCAAlgorithms;
using LevelManagement.Scenes;
using UnityEngine;
using UnityEngine.UI;

namespace LevelManagement.UIs {
    public class AgentCCAUI : MonoBehaviour
    {
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private AgentCCA agentCca;
        [SerializeField] private Slider trailDecay;

        

        private void OnEnable() {
            backToMenuButton.onClick.AddListener(OnClickBackToMenu);
            trailDecay.onValueChanged.AddListener(OnClickTrailDecaySlider);
            
        }

        private void OnDisable() {
            backToMenuButton.onClick.RemoveListener(OnClickBackToMenu);
            trailDecay.onValueChanged.RemoveListener(OnClickTrailDecaySlider);
        }
        
        
        private void OnClickBackToMenu() {
            ScenesManager.Instance.LoadMainMenu();
        }
        
        private void OnClickTrailDecaySlider(float value) {
            agentCca.trailDecayFactor = value;
        }
    
    }
}

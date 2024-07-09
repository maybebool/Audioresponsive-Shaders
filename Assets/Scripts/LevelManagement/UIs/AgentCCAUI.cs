using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LevelManagement.UIs {
    public class AgentCCAUI : MonoBehaviour
    {
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private AgentCCA agentCca;
        // [SerializeField] private Slider agentamount;
        [SerializeField] private Slider traildecay;

        

        private void OnEnable() {
            backToMenuButton.onClick.AddListener(OnClickBackToMenu);
            traildecay.onValueChanged.AddListener(OnClickTrailDecaySlider);
            
        }

        private void OnDisable() {
            backToMenuButton.onClick.RemoveListener(OnClickBackToMenu);
            // agentamount.onValueChanged.RemoveListener(OnClickAgentAmount);
            traildecay.onValueChanged.RemoveListener(OnClickTrailDecaySlider);
        }
        
        
        private void OnClickBackToMenu() {
            ScenesManager.Instance.LoadMainMenu();
        }

        private void OnClickAgentAmount(float value) {
            agentCca.agentsCount = (int)value;
            
        }
        
        private void OnClickTrailDecaySlider(float value) {
            agentCca.trailDecayFactor = value;
        }
    
    }
}

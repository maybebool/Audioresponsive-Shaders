using UnityEngine;
using UnityEngine.UI;

namespace LevelManagement.UIs {
    public class FlockingUI : MonoBehaviour
    {
        [SerializeField] private Button backToMenuButton;

        private void OnEnable() {
            backToMenuButton.onClick.AddListener(OnClickBackToMenu);
            
            
        }

        private void OnDisable() {
            backToMenuButton.onClick.RemoveListener(OnClickBackToMenu);
            
        }
        
        
        private void OnClickBackToMenu() {
            ScenesManager.Instance.LoadMainMenu();
        }
    }
}

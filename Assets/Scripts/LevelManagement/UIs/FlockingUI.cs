using UnityEngine;
using UnityEngine.UI;

namespace LevelManagement.UIs {
    public class FlockingUI : MonoBehaviour
    {
        [SerializeField] private Button backToMenuButton;
        private void OnEnable() {
            backToMenuButton.onClick.AddListener(BackToMenuButtonClicked);
            
        }

        private void OnDisable() {
            backToMenuButton.onClick.RemoveListener(BackToMenuButtonClicked);
        }
        
        
        private void BackToMenuButtonClicked() {
            ScenesManager.Instance.LoadMainMenu();
        }
    }
}

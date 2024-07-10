using LevelManagement;
using UnityEngine;
using UnityEngine.UI;

public class NoiseUI : MonoBehaviour
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

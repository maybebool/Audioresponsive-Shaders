using CCAAlgorithms;
using LevelManagement;
using UnityEngine;
using UnityEngine.UI;

public class CCAUI : MonoBehaviour
{
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private CCA ccaScript;
    [SerializeField] private Button randomizeColorButton;
    [SerializeField] private Button randomizeParamsButton;
    [SerializeField] private Button defaultParamsButton;

    private void OnEnable() {
        backToMenuButton.onClick.AddListener(OnClickBackToMenu);
        randomizeColorButton.onClick.AddListener(OnClickRandomizeColorButton);
        randomizeParamsButton.onClick.AddListener(OnCLickRandomizeParamsButton);
        defaultParamsButton.onClick.AddListener(OnClickDefaultParamsButton);
            
            
    }

    private void OnDisable() {
        backToMenuButton.onClick.RemoveListener(OnClickBackToMenu);
        randomizeColorButton.onClick.RemoveListener(OnClickRandomizeColorButton);
        randomizeParamsButton.onClick.RemoveListener(OnCLickRandomizeParamsButton);
        defaultParamsButton.onClick.RemoveListener(OnClickDefaultParamsButton);
            
    }
        
        
    private void OnClickBackToMenu() {
        ScenesManager.Instance.LoadMainMenu();
    }

    private void OnClickRandomizeColorButton() {
        ccaScript.SetColors();
        
    }

    private void OnCLickRandomizeParamsButton() {
        ccaScript.ResetAndRandomize();
    }

    private void OnClickDefaultParamsButton() {
        ScenesManager.Instance.LoadScene(Scene.CCA);
    }
}

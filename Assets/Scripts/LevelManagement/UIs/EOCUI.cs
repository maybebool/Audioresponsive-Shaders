using CCAAlgorithms;
using LevelManagement.Scenes;
using UnityEngine;
using UnityEngine.UI;

namespace LevelManagement.UIs {
    public class EOCUI : MonoBehaviour
    {
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private EOCCCA eoc;
        [SerializeField] private Toggle randomizingYesNo;
        [SerializeField] private Slider stepsPerFrame;
        
        private void OnEnable() {
            backToMenuButton.onClick.AddListener(OnClickBackToMenu);
            randomizingYesNo.onValueChanged.AddListener(OnClickToggleRandomize);
            stepsPerFrame.onValueChanged.AddListener(OnClickStepsPerFrameSlider);
            
        }

        private void OnDisable() {
            backToMenuButton.onClick.RemoveListener(OnClickBackToMenu);
            randomizingYesNo.onValueChanged.RemoveListener(OnClickToggleRandomize);
            stepsPerFrame.onValueChanged.RemoveListener(OnClickStepsPerFrameSlider);
        }
        
        
        private void OnClickBackToMenu() {
            ScenesManager.Instance.LoadMainMenu();
        }

        private void OnClickToggleRandomize(bool yesNo) {
                eoc.randomize = yesNo;
            
        }
        
        private void OnClickStepsPerFrameSlider(float value) {
            eoc.stepsPerFrame = (int)value;
        }
        
    }
}

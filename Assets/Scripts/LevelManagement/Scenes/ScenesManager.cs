using LevelManagement.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelManagement.Scenes {
    public class ScenesManager : Singleton<ScenesManager>
    {
        public void LoadScene(Scene scene) {
            SceneManager.LoadScene(scene.ToString());
        }

        public void LoadMainMenu() {
            SceneManager.LoadScene(Scene.MainMenu.ToString());
        }
        
        public void QuitGame() {
            Application.Quit();
        }
    }
}

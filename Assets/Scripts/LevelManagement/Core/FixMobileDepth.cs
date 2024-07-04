using UnityEngine;

namespace LevelManagement.Core {
    
    [ExecuteInEditMode]
    public class FixMobileDepth : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
        }
    }
}
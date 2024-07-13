using UnityEngine;

namespace LevelManagement.UIs {
    public class ButtonAnimation : MonoBehaviour
    {
        [SerializeField] private float amplitude;
        [SerializeField] [Range(0.5f, 20f)] private float frequency;

        private Vector3 _startPos;
        private void Start() {
            _startPos = transform.position;
        }

        private void Update() {
            var easedSin = Mathf.Sin(Time.time * frequency);
            easedSin = EaseInQuad(easedSin);
            transform.position = _startPos + amplitude * easedSin * Vector3.up;
        }
        
        private static float EaseInQuad(float t) { return t * t; }
    }
}

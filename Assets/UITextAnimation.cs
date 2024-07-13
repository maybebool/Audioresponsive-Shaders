using UnityEngine;

public class UITextAnimation : MonoBehaviour {
    private const string GlowProperty = "_GlowOuter";
    [SerializeField] private Material title;
    [SerializeField] private Vector2 minMaxValueLerp;
    [SerializeField] private float frequency;


    private void Update() {

        var timeVariable = Mathf.Sin(Time.time * frequency);
        var lerpValueGlowOuter = Mathf.Lerp(minMaxValueLerp.x, minMaxValueLerp.y, timeVariable);
        title.SetFloat(GlowProperty, lerpValueGlowOuter);
    }
}

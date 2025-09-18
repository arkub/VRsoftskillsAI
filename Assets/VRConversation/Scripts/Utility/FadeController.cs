using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class FadeController : MonoBehaviour
{
    [Tooltip("Material must support alpha (Unlit Transparent).")]
    public Material fadeMaterial;
    [Tooltip("Start fully black when true.")]
    public bool startBlack = true;
   public float alpha = 1f;
    MeshRenderer _renderer;
    Material _runtimeMat;

    void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        if (fadeMaterial == null)
        {
            Debug.LogError("FadeController: assign fadeMaterial.");
            enabled = false;
            return;
        }
        // Use an instance so other objects using same material aren't affected
        _runtimeMat = Instantiate(fadeMaterial);
        _renderer.material = _runtimeMat;

         SetAlpha(alpha);
        
    }

    void SetAlpha(float a)
    {
        if (_runtimeMat.HasProperty("_Color"))
        {
            Color c = _runtimeMat.color;
            c.a = Mathf.Clamp01(a);
            _runtimeMat.color = c;
        }
        else if (_runtimeMat.HasProperty("_BaseColor"))
        {
            Color c = _runtimeMat.GetColor("_BaseColor");
            c.a = Mathf.Clamp01(a);
            _runtimeMat.SetColor("_BaseColor", c);
        }
    }


    public void FadeIn(float duration)
    {
        StartCoroutine(FadeRoutine(alpha, 0f, duration));
    }

    public void FadeOut(float duration)
    {
        StartCoroutine(FadeRoutine(0f, alpha, duration));
    }

    IEnumerator FadeRoutine(float from, float to, float duration)
    {
        float t = 0f;
        SetAlpha(from);
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(to);
    }
}

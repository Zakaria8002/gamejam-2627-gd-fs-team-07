using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class TargetBehaviour : MonoBehaviour
{
    [Header("Lifetime")]
    public float lifetime = 5f;
    [Header("Fade")]
    public float fadeDuration = 0.5f;

    private bool isFading = false;
    private bool isChecked = false;

    private Collider2D[] colliders2D;
    private SpriteRenderer[] spriteRenderers;

    private void Awake()
    {
        colliders2D = GetComponentsInChildren<Collider2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        // start auto lifetime
        StartCoroutine(LifetimeCoroutine());
    }

    private IEnumerator LifetimeCoroutine()
    {
        float t = 0f;
        while (t < lifetime)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // time's up, start fade and destroy
        StartFadeAndDestroy();
    }

    public void OnChecked()
    {
        if (isFading || isChecked) return;
        isChecked = true;
        // prevent further checks immediately
        DisableColliders();
        StartFadeAndDestroy();
    }

    private void DisableColliders()
    {
        if (colliders2D == null) colliders2D = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders2D)
        {
            if (c != null) c.enabled = false;
        }
    }

    private void StartFadeAndDestroy()
    {
        if (isFading) return;
        isFading = true;
        DisableColliders();
        StartCoroutine(FadeCoroutine());
    }

    private IEnumerator FadeCoroutine()
    {
        float elapsed = 0f;
        // capture original colors
        Color[] origColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            origColors[i] = spriteRenderers[i].color;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float a = Mathf.Lerp(1f, 0f, t);
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] == null) continue;
                Color c = origColors[i];
                c.a = a * origColors[i].a; // preserve original alpha multiplier
                spriteRenderers[i].color = c;
            }
            yield return null;
        }

        // ensure alpha zero
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null) continue;
            Color c = origColors[i];
            c.a = 0f;
            spriteRenderers[i].color = c;
        }

        // destroy the gameobject
        Destroy(gameObject);
    }
}

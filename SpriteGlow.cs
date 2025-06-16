using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteGlow : MonoBehaviour
{
    [Header("Glow Settings")]
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private Color glowColor = new Color(1f, 1f, 0f, 0.5f); // Yellow glow
    [SerializeField, Range(0f, 5f)] private float glowSize = 1f;
    [SerializeField, Range(0f, 1f)] private float glowOpacity = 0.5f;
    [SerializeField, Range(2, 8)] private int glowLayerCount = 4; // Number of layers for softness

    private SpriteRenderer mainSprite;
    private GameObject[] glowObjects;
    private SpriteRenderer[] glowSprites;

    void Awake()
    {
        mainSprite = GetComponent<SpriteRenderer>();
        CreateGlowObjects();
    }

    void CreateGlowObjects()
    {
        // Clean up old
        if (glowObjects != null)
        {
            foreach (var obj in glowObjects)
                if (obj != null) Destroy(obj);
        }

        glowObjects = new GameObject[glowLayerCount];
        glowSprites = new SpriteRenderer[glowLayerCount];

        for (int i = 0; i < glowLayerCount; i++)
        {
            var obj = new GameObject($"GlowLayer_{i}");
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = mainSprite.sprite;
            sr.sortingLayerName = mainSprite.sortingLayerName;
            sr.sortingOrder = mainSprite.sortingOrder - 1;

            glowObjects[i] = obj;
            glowSprites[i] = sr;
        }
        UpdateGlow();
    }

    void Update()
    {
        if (!enableGlow || glowSprites == null)
        {
            if (glowObjects != null)
                foreach (var obj in glowObjects) obj.SetActive(false);
            return;
        }

        foreach (var obj in glowObjects) obj.SetActive(true);

        // Update each glow layer
        for (int i = 0; i < glowLayerCount; i++)
        {
            var sr = glowSprites[i];
            sr.sprite = mainSprite.sprite;

            float t = (float)(i + 1) / glowLayerCount;
            float scale = 1f + glowSize * t;
            float currentOpacity = glowOpacity * (1f - t * 0.85f); // Fade out further layers

            Color c = glowColor;
            c.a = currentOpacity;
            sr.color = c;
            glowObjects[i].transform.localScale = Vector3.one * scale;
        }
    }

    void UpdateGlow()
    {
        if (glowSprites == null) return;
        for (int i = 0; i < glowLayerCount; i++)
        {
            float t = (float)(i + 1) / glowLayerCount;
            float scale = 1f + glowSize * t;
            float alpha = glowOpacity * (1f - t * 0.85f);

            Color c = glowColor;
            c.a = alpha;
            glowSprites[i].color = c;
            glowObjects[i].transform.localScale = Vector3.one * scale;
        }
    }

    // Public methods to control glow
    public void SetGlowColor(Color color)
    {
        glowColor = color;
        UpdateGlow();
    }

    public void SetGlowSize(float size)
    {
        glowSize = Mathf.Clamp(size, 0f, 5f);
        UpdateGlow();
    }

    public void SetGlowOpacity(float opacity)
    {
        glowOpacity = Mathf.Clamp01(opacity);
        UpdateGlow();
    }

    public void SetEnabled(bool enabled)
    {
        enableGlow = enabled;
        if (glowObjects != null)
            foreach (var obj in glowObjects) obj.SetActive(enabled);
    }

    void OnDestroy()
    {
        if (glowObjects != null)
            foreach (var obj in glowObjects)
                if (obj != null) Destroy(obj);
    }
}
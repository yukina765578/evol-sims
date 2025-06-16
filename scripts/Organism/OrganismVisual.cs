using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(SpriteGlow))]
public class OrganismVisual : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private SpriteGlow spriteGlow;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteGlow = GetComponent<SpriteGlow>();
        UpdateGlowToMatchColor();
    }

    // Call this whenever the organism color changes
    public void SetOrganismColor(Color color)
    {
        spriteRenderer.color = color;
        UpdateGlowToMatchColor();
    }

    private void UpdateGlowToMatchColor()
    {
        if (spriteGlow != null && spriteRenderer != null)
        {
            // Use the organism's color for the glow, but with custom alpha
            Color glowColor = spriteRenderer.color;
            glowColor.a = 0.5f; // You can adjust this alpha as needed
            spriteGlow.SetGlowColor(glowColor);
        }
    }
}
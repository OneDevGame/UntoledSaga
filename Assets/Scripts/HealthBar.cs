using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Transform fillBar;
    private int maxHp;

    public void Setup(int maxHealth)
    {
        maxHp = maxHealth;

        // 1. Dunkler Hintergrund-Balken
        GameObject bg = new GameObject("HP_Background");
        bg.transform.SetParent(transform);
        bg.transform.localPosition = new Vector3(0, 1.15f, 0); // Über der Figur
        bg.transform.localScale = new Vector3(1.2f, 0.18f, 1f);
        var bgRenderer = bg.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = CreateWhiteSprite();
        bgRenderer.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        bgRenderer.sortingOrder = 10;

        // 2. Grüner Lebensbalken (Vordergrund)
        GameObject fg = new GameObject("HP_Fill");
        fg.transform.SetParent(bg.transform);
        fg.transform.localPosition = new Vector3(0, 0, -0.1f);
        fg.transform.localScale = Vector3.one;
        var fgRenderer = fg.AddComponent<SpriteRenderer>();
        fgRenderer.sprite = CreateWhiteSprite();
        fgRenderer.color = Color.green;
        fgRenderer.sortingOrder = 11;

        fillBar = fg.transform;
    }

    public void UpdateHealth(int currentHealth)
    {
        if (fillBar == null || maxHp <= 0) return;

        float ratio = Mathf.Clamp01((float)currentHealth / maxHp);
        fillBar.localScale = new Vector3(ratio, 1f, 1f);
        // Verschiebt den Balken nach links, damit er von rechts nach links schrumpft
        fillBar.localPosition = new Vector3(-(1f - ratio) * 0.5f, 0, -0.1f);

        // Farbe wechselt bei niedrigem Leben auf Gelb/Rot
        var sr = fillBar.GetComponent<SpriteRenderer>();
        if (ratio > 0.5f) sr.color = Color.green;
        else if (ratio > 0.25f) sr.color = Color.yellow;
        else sr.color = Color.red;
    }

    // Erzeugt per Code eine weiße 1x1-Pixel-Textur (keine Bilddatei nötig)
    private Sprite CreateWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
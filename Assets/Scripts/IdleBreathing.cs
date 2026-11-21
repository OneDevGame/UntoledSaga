using UnityEngine;

public class IdleBreathing : MonoBehaviour
{
    [Header("Atem-Einstellungen")]
    public float breathSpeed = 2.4f;   // Geschwindigkeit der Atmung
    public float breathAmount = 0.035f; // Stärke des Brustkorb-Hebens

    private Vector3 baseScale;
    private float randomOffset;

    void Start()
    {
        baseScale = transform.localScale;
        randomOffset = Random.Range(0f, 6.28f);
    }

    void Update()
    {
        // Sanftes Ein- und Ausatmen (dehnt/staucht leicht die Höhe, während die Breite minimal mitgeht)
        float wave = Mathf.Sin((Time.time * breathSpeed) + randomOffset);
        float newY = baseScale.y + (wave * baseScale.y * breathAmount);
        float newX = baseScale.x - (wave * baseScale.x * (breathAmount * 0.4f));

        transform.localScale = new Vector3(newX, newY, baseScale.z);
    }
}
using System.Collections;
using UnityEngine;

public class DwarfBoneAnimator : MonoBehaviour
{
    [Header("Einzelteile zuweisen (aus Held reinziehen)")]
    public Transform axeTransform;    // Held Axt
    public Transform shieldTransform; // Held Schild

    [Header("Ruhiges Atmen (Idle)")]
    public float breathSpeed = 1.8f;

    [Header("Überkopf-Axthieb (Körper + Faust + Axt schwingen mit!)")]
    public float windupAngle = 55f;     // Axt holt nach hinten aus
    public float smashAngle = -85f;     // Axt schlägt nach vorne unten zu
    public float bodyLeanBack = 8f;     // Oberkörper + Faust lehnen sich beim Ausholen zurück
    public float bodySmashLean = -14f;  // Oberkörper + Faust werfen sich nach vorne in den Schlag!
    public float lungeDistance = 0.35f; // Kleiner wuchtiger Schritt nach vorne

    [Header("Schild-Abwehr beim Treffer")]
    public Vector3 shieldBlockOffset = new Vector3(0.45f, 0.15f, 0f);

    private Vector3 bodyStartPos;
    private Quaternion bodyStartRot;
    private Vector3 bodyStartScale;
    private Quaternion axeStartRot;
    private Vector3 shieldStartPos;
    private bool isBusy = false;

    void Start()
    {
        bodyStartPos = transform.localPosition;
        bodyStartRot = transform.localRotation;
        bodyStartScale = transform.localScale;

        if (axeTransform != null)
            axeStartRot = axeTransform.localRotation;

        if (shieldTransform != null)
            shieldStartPos = shieldTransform.localPosition;
    }

    void Update()
    {
        if (isBusy) return;

        float wave = Mathf.Sin(Time.time * breathSpeed);
        float slowWave = Mathf.Sin(Time.time * breathSpeed * 0.85f + 0.8f);

        // Sanftes Atmen des Körpers (nimmt die Faust leicht mit!)
        transform.localScale = new Vector3(
            bodyStartScale.x,
            bodyStartScale.y + (wave * bodyStartScale.y * 0.012f),
            bodyStartScale.z
        );

        // Ganz leichtes Wippen von Körper, Axt und Schild
        transform.localRotation = bodyStartRot * Quaternion.Euler(0, 0, wave * 0.8f);

        if (axeTransform != null)
            axeTransform.localRotation = axeStartRot * Quaternion.Euler(0, 0, slowWave * 2.0f);

        if (shieldTransform != null)
            shieldTransform.localPosition = shieldStartPos + new Vector3(0, slowWave * 0.04f, 0);
    }

    // 1. Angriff: Körper + Faust lehnen sich zurück und werfen sich mit der Axt nach vorne!
    public void PlayAttackSwing()
    {
        if (!isBusy)
            StartCoroutine(OverheadSmashRoutine());
    }

    // 2. Treffer: Schild gleitet schützend nach vorne + blaues Aufleuchten
    public void PlayShieldBlock()
    {
        StopAllCoroutines();
        transform.localPosition = bodyStartPos;
        transform.localRotation = bodyStartRot;
        if (axeTransform != null) axeTransform.localRotation = axeStartRot;
        StartCoroutine(ShieldBlockRoutine());
    }

    private IEnumerator OverheadSmashRoutine()
    {
        isBusy = true;

        // 1. Ausholen: Körper & Faust lehnen sich nach hinten, Axt geht hoch (0.26 Sek)
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3.8f;
            float ease = Mathf.SmoothStep(0f, 1f, t);

            transform.localRotation = bodyStartRot * Quaternion.Euler(0, 0, Mathf.Lerp(0f, bodyLeanBack, ease));
            transform.localPosition = Vector3.Lerp(bodyStartPos, bodyStartPos + new Vector3(-0.15f, 0f, 0f), ease);

            if (axeTransform != null)
                axeTransform.localRotation = axeStartRot * Quaternion.Euler(0, 0, Mathf.Lerp(0f, windupAngle, ease));
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        // 2. Zuschlagen: Körper + Faust schnellen nach vorne und kippen wuchtig in den Hieb! (0.10 Sek)
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 10f;
            float slam = t * t;

            transform.localRotation = bodyStartRot * Quaternion.Euler(0, 0, Mathf.Lerp(bodyLeanBack, bodySmashLean, slam));
            transform.localPosition = Vector3.Lerp(bodyStartPos + new Vector3(-0.15f, 0f, 0f), bodyStartPos + new Vector3(lungeDistance, 0f, 0f), slam);

            if (axeTransform != null)
                axeTransform.localRotation = axeStartRot * Quaternion.Euler(0, 0, Mathf.Lerp(windupAngle, smashAngle, slam));
            yield return null;
        }

        StartCoroutine(CameraImpactShake(0.18f, 0.14f));
        yield return new WaitForSeconds(0.22f);

        // 3. Zurück in die Kampfhaltung (0.25 Sek)
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            float ease = Mathf.SmoothStep(0f, 1f, t);

            transform.localRotation = bodyStartRot * Quaternion.Euler(0, 0, Mathf.Lerp(bodySmashLean, 0f, ease));
            transform.localPosition = Vector3.Lerp(bodyStartPos + new Vector3(lungeDistance, 0f, 0f), bodyStartPos, ease);

            if (axeTransform != null)
                axeTransform.localRotation = axeStartRot * Quaternion.Euler(0, 0, Mathf.Lerp(smashAngle, 0f, ease));
            yield return null;
        }

        isBusy = false;
    }

    private IEnumerator ShieldBlockRoutine()
    {
        isBusy = true;
        var shieldSr = shieldTransform != null ? shieldTransform.GetComponent<SpriteRenderer>() : null;
        Color origColor = shieldSr != null ? shieldSr.color : Color.white;
        Vector3 targetPos = shieldStartPos + shieldBlockOffset;

        // Schild gleitet vor die Brust + Körper stemmt sich leicht dagegen
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 8f;
            float ease = Mathf.SmoothStep(0f, 1f, t);
            transform.localRotation = bodyStartRot * Quaternion.Euler(0, 0, Mathf.Lerp(0f, 4f, ease));
            if (shieldTransform != null)
                shieldTransform.localPosition = Vector3.Lerp(shieldStartPos, targetPos, ease);
            if (shieldSr != null)
                shieldSr.color = Color.Lerp(origColor, new Color(0.4f, 0.85f, 1f, 1f), ease);
            yield return null;
        }

        yield return new WaitForSeconds(0.28f);

        // Zurück in die Ausgangsposition
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            float ease = Mathf.SmoothStep(0f, 1f, t);
            transform.localRotation = bodyStartRot * Quaternion.Euler(0, 0, Mathf.Lerp(4f, 0f, ease));
            if (shieldTransform != null)
                shieldTransform.localPosition = Vector3.Lerp(targetPos, shieldStartPos, ease);
            if (shieldSr != null)
                shieldSr.color = Color.Lerp(new Color(0.4f, 0.85f, 1f, 1f), origColor, ease);
            yield return null;
        }

        isBusy = false;
    }

    private IEnumerator CameraImpactShake(float duration, float magnitude)
    {
        if (Camera.main == null) yield break;
        Vector3 originalPos = Camera.main.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Camera.main.transform.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = originalPos;
    }
}

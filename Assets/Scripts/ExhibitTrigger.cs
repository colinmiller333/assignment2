using System.Collections;
using UnityEngine;

/// Put on each exhibit root together with a trigger Collider (Is Trigger = true).
/// When the player enters: fades in the info panel, turns on a spotlight, plays audio,
/// starts an animation, and registers the visit with MuseumManager.
[RequireComponent(typeof(Collider))]
public class ExhibitTrigger : MonoBehaviour
{
    [Header("Identity")]
    public string exhibitName = "Exhibit";

    [Header("Proximity effects")]
    public CanvasGroup infoPanel;          // world-space canvas with CanvasGroup
    public Light spotlight;                // optional
    public AudioSource narration;          // optional
    public Animator exhibitAnimator;       // optional (Bonus 2)
    public string animatorBoolName = "Active";

    [Header("Tuning")]
    public float fadeTime = 0.4f;
    public float dimIntensity = 0.5f;
    public float brightIntensity = 6f;
    public bool panelFacesPlayer = true;

    Transform player;
    Coroutine fadeRoutine;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void Start()
    {
        if (infoPanel != null) { infoPanel.alpha = 0f; infoPanel.gameObject.SetActive(false); }
        if (spotlight != null) spotlight.intensity = dimIntensity;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        player = other.transform;
        SetActive(true);
        if (MuseumManager.Instance != null) MuseumManager.Instance.RegisterVisit(exhibitName);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        SetActive(false);
        player = null;
    }

    void Update()
    {
        if (!panelFacesPlayer || player == null || infoPanel == null) return;
        Vector3 dir = infoPanel.transform.position - Camera.main.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            infoPanel.transform.rotation = Quaternion.LookRotation(dir);
    }

    void SetActive(bool on)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Fade(on));

        if (narration != null)
        {
            if (on) narration.Play(); else narration.Stop();
        }
        if (exhibitAnimator != null) exhibitAnimator.SetBool(animatorBoolName, on);
    }

    IEnumerator Fade(bool on)
    {
        if (on && infoPanel != null) infoPanel.gameObject.SetActive(true);

        float startAlpha = infoPanel != null ? infoPanel.alpha : 0f;
        float startLight = spotlight != null ? spotlight.intensity : 0f;
        float endAlpha = on ? 1f : 0f;
        float endLight = on ? brightIntensity : dimIntensity;

        for (float t = 0f; t < fadeTime; t += Time.deltaTime)
        {
            float k = t / fadeTime;
            if (infoPanel != null) infoPanel.alpha = Mathf.Lerp(startAlpha, endAlpha, k);
            if (spotlight != null) spotlight.intensity = Mathf.Lerp(startLight, endLight, k);
            yield return null;
        }

        if (infoPanel != null)
        {
            infoPanel.alpha = endAlpha;
            if (!on) infoPanel.gameObject.SetActive(false);
        }
        if (spotlight != null) spotlight.intensity = endLight;
    }
}

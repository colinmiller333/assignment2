using System.Collections;
using TMPro;
using UnityEngine;

/// Put on each exhibit root with a trigger Collider (Is Trigger = true).
/// The headset camera activates the information panel, spotlight, and optional narration nearby.
[RequireComponent(typeof(Collider))]
public class ExhibitTrigger : MonoBehaviour
{
    [Header("Identity")]
    public string exhibitName = "Exhibit";
    public string exhibitYear;
    [TextArea(3, 6)] public string exhibitDescription;

    [Header("Proximity effects")]
    public CanvasGroup infoPanel;          // world-space canvas with CanvasGroup
    public TMP_Text titleText;
    public TMP_Text yearText;
    public TMP_Text descriptionText;
    public Light spotlight;                // optional
    public AudioSource narration;          // optional

    [Header("Tuning")]
    public float fadeTime = 0.4f;
    public float dimIntensity = 0.5f;
    public float brightIntensity = 6f;
    public bool panelFacesPlayer = true;

    Transform viewer;
    Collider proximityCollider;
    Coroutine fadeRoutine;
    bool isActive;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Start()
    {
        proximityCollider = GetComponent<Collider>();
        PopulateExhibitPanel();
        if (infoPanel != null) { infoPanel.alpha = 0f; infoPanel.gameObject.SetActive(false); }
        if (spotlight != null) spotlight.intensity = dimIntensity;
    }

    void Update()
    {
        Camera viewerCamera = Camera.main;
        if (viewerCamera == null || proximityCollider == null) return;

        viewer = viewerCamera.transform;
        Vector3 closestPoint = proximityCollider.ClosestPoint(viewer.position);
        bool isWithinRange = (viewer.position - closestPoint).sqrMagnitude <= 0.0025f;
        if (isWithinRange != isActive) SetActive(isWithinRange);

        if (!panelFacesPlayer || !isActive || infoPanel == null) return;
        Vector3 dir = infoPanel.transform.position - viewer.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            infoPanel.transform.rotation = Quaternion.LookRotation(dir);
    }

    void PopulateExhibitPanel()
    {
        if (string.IsNullOrWhiteSpace(exhibitName) || exhibitName == "Exhibit")
            exhibitName = transform.parent != null ? transform.parent.name : "Space Exhibit";

        if (string.IsNullOrWhiteSpace(exhibitDescription))
        {
            switch (exhibitName)
            {
                case "Sputnik 1":
                    exhibitYear = "October 4, 1957";
                    exhibitDescription = "The Soviet Union's Sputnik 1 was the first artificial satellite. About 58 cm across and 189 lb, its launch began the Space Age and the U.S.-Soviet Space Race, and helped spur the creation of NASA in 1958.";
                    break;
                case "Vostok 1":
                    exhibitYear = "April 12, 1961";
                    exhibitDescription = "Soviet cosmonaut Yuri Gagarin became the first person to orbit Earth. Vostok 1 completed one orbit, reached 327 km altitude, and returned after a 108-minute flight.";
                    break;
                case "Apollo 11 LM":
                    exhibitYear = "July 16, 1969";
                    exhibitDescription = "Launched atop a Saturn V, lunar module Eagle carried Neil Armstrong and Buzz Aldrin to the Moon. It landed at Tranquility Base on July 20, the first crewed lunar landing; its descent stage remains on the Moon.";
                    break;
                case "Saturn V":
                    exhibitYear = "1967-1973";
                    exhibitDescription = "NASA developed this three-stage, liquid-fueled heavy-lift rocket for human lunar exploration. It launched 13 times from Kennedy Space Center; nine crewed launches carried 24 astronauts. Apollo 6 was its one partial failure.";
                    break;
                case "James Webb Telescope":
                    exhibitYear = "December 25, 2021";
                    exhibitDescription = "A NASA, ESA, and CSA observatory, Webb orbits the Sun about 1.5 million km from Earth. Its 6.5 m mirror observes red and infrared light; a five-layer sunshield keeps it near -223 C.";
                    break;
                case "Hubble":
                    exhibitYear = "April 24, 1990";
                    exhibitDescription = "Hubble launched into low Earth orbit aboard Space Shuttle Discovery on mission STS-31. Its 2.4 m mirror observes ultraviolet, visible, and near-infrared light; servicing missions helped extend its scientific life.";
                    break;
            }
        }

        if (titleText != null) titleText.text = exhibitName;
        if (yearText != null) yearText.text = exhibitYear;
        if (descriptionText != null) descriptionText.text = exhibitDescription;
    }

    void SetActive(bool on)
    {
        isActive = on;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Fade(on));

        if (narration != null)
        {
            if (on) narration.Play(); else narration.Stop();
        }
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

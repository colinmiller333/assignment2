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
                    exhibitDescription = "The USSR launched Sputnik 1 on October 4, 1957. The first artificial satellite was about 58 cm wide and weighed 189 lb. The idea grew from a 1954 proposal for satellite mapping during the International Geophysical Year. Its launch began the Space Age and the U.S.-Soviet space race; Congress created NASA in 1958. A month later, Sputnik 2 carried Laika.";
                    break;
                case "Vostok 1":
                    exhibitYear = "April 12, 1961";
                    exhibitDescription = "Cosmonaut Yuri Gagarin became the first person to orbit Earth. Vostok 1 completed one orbit in 108 minutes, reaching 327 km at its highest and 181 km at its lowest. Gagarin ejected from the capsule and parachuted to Earth. The flight capped the Soviet program competing with the U.S. Project Mercury.";
                    break;
                case "Apollo 11 LM":
                    exhibitYear = "July 16, 1969";
                    exhibitDescription = "Launched atop a Saturn V, Eagle traveled to lunar orbit with command module Columbia. Neil Armstrong and Buzz Aldrin landed at Tranquility Base on July 20, the first crewed Moon landing. They left Eagle's descent stage at the site and returned to Columbia in the ascent stage; the descent stage's present location is unknown.";
                    break;
                case "Saturn V":
                    exhibitYear = "1967-1973";
                    exhibitDescription = "NASA developed this three-stage, liquid-fueled super-heavy rocket for human lunar exploration. All 13 launches began at Kennedy Space Center Launch Complex 39. Nine crewed flights carried 24 astronauts. The Saturn V had 12 successful launches and one partial failure: Apollo 6.";
                    break;
                case "James Webb Telescope":
                    exhibitYear = "December 25, 2021";
                    exhibitDescription = "Named for NASA administrator James E. Webb, this NASA, ESA, and CSA observatory launched on December 25, 2021. It orbits the Sun about 1.5 million km from Earth at the second Lagrange point. Its 6.5 m mirror is 2.7 times Hubble's diameter and observes red and infrared light. A five-layer sunshield keeps the telescope near -223 C.";
                    break;
                case "Hubble":
                    exhibitYear = "April 24, 1990";
                    exhibitDescription = "Hubble launched into low Earth orbit aboard Space Shuttle Discovery on mission STS-31. Its 2.4 m mirror and five instruments observe ultraviolet, visible, and near-infrared light. The 1986 Challenger disaster delayed launch for several years, during which engineers tested and improved the telescope. Shuttle servicing missions later repaired and upgraded it.";
                    break;
            }
        }

        SetPanelText(titleText, exhibitName);
        SetPanelText(yearText, exhibitYear);
        SetPanelText(descriptionText, exhibitDescription);
    }

    static void SetPanelText(TMP_Text textComponent, string value)
    {
        if (textComponent == null) return;

        TMP_InputField inputField = textComponent.GetComponentInParent<TMP_InputField>();
        if (inputField != null)
        {
            inputField.readOnly = true;
            inputField.text = value;
        }
        else
        {
            textComponent.text = value;
        }
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

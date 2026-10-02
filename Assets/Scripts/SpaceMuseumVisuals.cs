using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
public class SpaceMuseumVisuals : MonoBehaviour
{
    const string DisplayRootName = "Generated_Space_Exhibits";
    static readonly string[] ExhibitNames =
    {
        "Sputnik 1", "Vostok 1", "Apollo 11 LM", "Saturn V", "James Webb Telescope", "Hubble"
    };
    static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    void OnEnable() => BuildDisplays();
    void Start() => BuildDisplays();

    public void BuildDisplays()
    {
        if (!gameObject.scene.IsValid()) return;

        Transform displayRoot = transform.Find(DisplayRootName);
        bool changed = false;
        if (displayRoot == null)
        {
            GameObject root = new GameObject(DisplayRootName);
            root.transform.SetParent(transform, false);
            displayRoot = root.transform;
            changed = true;
        }

        Dictionary<string, Transform> stations = FindStations(gameObject.scene);
        foreach (string exhibitName in ExhibitNames)
        {
            if (!stations.TryGetValue(exhibitName, out Transform station)) continue;
            Transform template = FindDescendant(station, "Exhibit_Template");
            Transform exhibitFrame = template != null ? template : station;
            string displayName = exhibitName + " Display";
            Transform display = null;
            for (int index = displayRoot.childCount - 1; index >= 0; index--)
            {
                Transform child = displayRoot.GetChild(index);
                if (child.name != displayName) continue;
                if (display == null)
                {
                    display = child;
                    continue;
                }

                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
                changed = true;
            }

            if (display == null)
            {
                GameObject item = new GameObject(displayName);
                item.transform.SetParent(displayRoot, false);
                display = item.transform;
                BuildExhibit(display, exhibitName);
                changed = true;
            }

            if ((display.position - exhibitFrame.position).sqrMagnitude > 0.0001f ||
                Quaternion.Angle(display.rotation, exhibitFrame.rotation) > 0.1f)
            {
                display.SetPositionAndRotation(exhibitFrame.position, exhibitFrame.rotation);
                changed = true;
            }

            ApplyMaterials(display);
            PositionStationElements(station);
        }

#if UNITY_EDITOR
        if (changed && !Application.isPlaying)
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    Dictionary<string, Transform> FindStations(Scene scene)
    {
        var stations = new Dictionary<string, Transform>();
        foreach (GameObject sceneRoot in scene.GetRootGameObjects())
        foreach (Transform candidate in sceneRoot.GetComponentsInChildren<Transform>(true))
        foreach (string exhibitName in ExhibitNames)
            if (candidate.name == exhibitName) stations[exhibitName] = candidate;
        return stations;
    }

    void PositionStationElements(Transform station)
    {
        Transform template = FindDescendant(station, "Exhibit_Template");
        if (template == null) return;
        Transform pedestal = FindDescendant(template, "Pedestal");
        if (pedestal != null) pedestal.localPosition = new Vector3(0.85f, 0.5f, 0f);
        if (FindDescendant(template, "Info_Panel") is RectTransform panel)
            panel.anchoredPosition = new Vector2(-0.85f, 1.5f);
    }

    static Transform FindDescendant(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            if (child != parent && child.name == childName) return child;
        return null;
    }

    void BuildExhibit(Transform parent, string name)
    {
        Part(parent, PrimitiveType.Cylinder, "Base_Glow | cyan", new Vector3(0.85f, 0.03f, 0f), new Vector3(0.7f, 0.025f, 0.7f));
        Transform model = new GameObject("Display Model").transform;
        model.SetParent(parent, false);
        model.localPosition = new Vector3(0.85f, 1f, 0f);

        switch (name)
        {
            case "Sputnik 1":
                Part(model, PrimitiveType.Sphere, "Satellite | silver", Vector3.zero, Vector3.one * 0.42f);
                Rod(model, "Antenna A | gold", new Vector3(-0.12f, 0.12f, 0f), new Vector3(-0.55f, 0.8f, 0f), 0.025f);
                Rod(model, "Antenna B | gold", new Vector3(0.12f, 0.12f, 0f), new Vector3(0.55f, 0.8f, 0f), 0.025f);
                Rod(model, "Antenna C | gold", new Vector3(0f, 0.12f, -0.12f), new Vector3(0f, 0.8f, -0.55f), 0.025f);
                Rod(model, "Antenna D | gold", new Vector3(0f, 0.12f, 0.12f), new Vector3(0f, 0.8f, 0.55f), 0.025f);
                break;
            case "Vostok 1":
                Part(model, PrimitiveType.Capsule, "Return Capsule | ivory", new Vector3(0f, 0.3f, 0f), new Vector3(0.43f, 0.42f, 0.43f));
                Part(model, PrimitiveType.Cylinder, "Service Module | silver", new Vector3(0f, -0.22f, 0f), new Vector3(0.38f, 0.18f, 0.38f));
                Part(model, PrimitiveType.Sphere, "Porthole | dark", new Vector3(0f, 0.36f, -0.21f), new Vector3(0.12f, 0.12f, 0.06f));
                break;
            case "Apollo 11 LM":
                Part(model, PrimitiveType.Cube, "Eagle Ascent Stage | gold", new Vector3(0f, 0.46f, 0f), new Vector3(0.52f, 0.42f, 0.48f));
                Part(model, PrimitiveType.Cube, "Eagle Descent Stage | silver", new Vector3(0f, 0.06f, 0f), new Vector3(0.76f, 0.25f, 0.72f));
                Part(model, PrimitiveType.Cylinder, "Docking Port | ivory", new Vector3(0f, 0.77f, 0f), Vector3.one * 0.18f);
                Rod(model, "Landing Leg A | gold", new Vector3(-0.2f, 0f, -0.2f), new Vector3(-0.48f, -0.45f, -0.48f), 0.035f);
                Rod(model, "Landing Leg B | gold", new Vector3(0.2f, 0f, -0.2f), new Vector3(0.48f, -0.45f, -0.48f), 0.035f);
                Rod(model, "Landing Leg C | gold", new Vector3(-0.2f, 0f, 0.2f), new Vector3(-0.48f, -0.45f, 0.48f), 0.035f);
                Rod(model, "Landing Leg D | gold", new Vector3(0.2f, 0f, 0.2f), new Vector3(0.48f, -0.45f, 0.48f), 0.035f);
                break;
            case "Saturn V":
                Part(model, PrimitiveType.Cylinder, "First Stage | ivory", new Vector3(0f, 0.03f, 0f), new Vector3(0.22f, 0.54f, 0.22f));
                Part(model, PrimitiveType.Cylinder, "Second Stage | silver", new Vector3(0f, 0.98f, 0f), new Vector3(0.2f, 0.34f, 0.2f));
                Part(model, PrimitiveType.Cylinder, "Instrument Unit | gold", new Vector3(0f, 1.65f, 0f), new Vector3(0.16f, 0.12f, 0.16f));
                Part(model, PrimitiveType.Capsule, "Apollo Spacecraft | ivory", new Vector3(0f, 1.95f, 0f), new Vector3(0.14f, 0.16f, 0.14f));
                Part(model, PrimitiveType.Cylinder, "Stage Band | dark", new Vector3(0f, 0.48f, 0f), new Vector3(0.224f, 0.04f, 0.224f));
                for (int i = 0; i < 4; i++)
                {
                    float angle = i * 90f;
                    Vector3 finPosition = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, -0.37f, 0.24f);
                    Part(model, PrimitiveType.Cube, "Fin " + i + " | dark", finPosition, new Vector3(0.08f, 0.38f, 0.16f), new Vector3(0f, angle, 0f));
                }
                break;
            case "James Webb Telescope":
                for (int i = 0; i < 5; i++)
                {
                    float size = 1.05f - i * 0.12f;
                    Part(model, PrimitiveType.Cube, "Sunshield Layer " + i + (i % 2 == 0 ? " | shield" : " | silver"),
                        new Vector3(0f, -0.34f + i * 0.055f, 0f), new Vector3(size, 0.018f, size * 0.64f), new Vector3(0f, 0f, i % 2 == 0 ? -7f : 7f));
                }
                Part(model, PrimitiveType.Cylinder, "Telescope Support | dark", new Vector3(0f, 0.25f, 0f), new Vector3(0.055f, 0.48f, 0.055f));
                Part(model, PrimitiveType.Cylinder, "Mirror Back | gold", new Vector3(0f, 0.69f, 0f), new Vector3(0.46f, 0.045f, 0.46f));
                Part(model, PrimitiveType.Sphere, "Mirror Center | gold", new Vector3(0f, 0.76f, 0f), new Vector3(0.18f, 0.07f, 0.18f));
                for (int i = 0; i < 6; i++)
                {
                    float angle = i * 60f * Mathf.Deg2Rad;
                    Vector3 segment = new Vector3(Mathf.Cos(angle) * 0.28f, 0.76f, Mathf.Sin(angle) * 0.28f);
                    Part(model, PrimitiveType.Sphere, "Mirror Segment " + i + " | gold", segment, new Vector3(0.18f, 0.07f, 0.18f));
                }
                Part(model, PrimitiveType.Cube, "Solar Array | blue", new Vector3(0.68f, 0.15f, 0f), new Vector3(0.38f, 0.035f, 0.3f));
                break;
            case "Hubble":
                Part(model, PrimitiveType.Cylinder, "Telescope Tube | ivory", new Vector3(0f, 0.4f, 0f), new Vector3(0.25f, 0.65f, 0.25f), new Vector3(90f, 0f, 0f));
                Part(model, PrimitiveType.Cylinder, "Aperture | dark", new Vector3(0f, 0.4f, -0.64f), new Vector3(0.23f, 0.035f, 0.23f), new Vector3(90f, 0f, 0f));
                Part(model, PrimitiveType.Cube, "Solar Array Left | blue", new Vector3(-0.05f, 0.48f, 0.62f), new Vector3(0.65f, 0.04f, 0.34f));
                Part(model, PrimitiveType.Cube, "Solar Array Right | blue", new Vector3(-0.05f, 0.48f, -0.62f), new Vector3(0.65f, 0.04f, 0.34f));
                Part(model, PrimitiveType.Cylinder, "Antenna | gold", new Vector3(0.18f, 0.8f, 0.25f), new Vector3(0.025f, 0.18f, 0.025f), new Vector3(20f, 0f, 25f));
                break;
        }
    }

    void Rod(Transform parent, string name, Vector3 start, Vector3 end, float radius)
    {
        Vector3 direction = end - start;
        Part(parent, PrimitiveType.Cylinder, name, (start + end) * 0.5f,
            new Vector3(radius, direction.magnitude * 0.5f, radius), Quaternion.FromToRotation(Vector3.up, direction.normalized).eulerAngles);
    }

    void Part(Transform parent, PrimitiveType primitive, string name, Vector3 position, Vector3 scale, Vector3 rotation = default(Vector3))
    {
        GameObject piece = GameObject.CreatePrimitive(primitive);
        piece.name = name;
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = Quaternion.Euler(rotation);
        piece.transform.localScale = scale;
        Collider pieceCollider = piece.GetComponent<Collider>();
        if (pieceCollider != null)
        {
            if (Application.isPlaying) Destroy(pieceCollider);
            else DestroyImmediate(pieceCollider);
        }
    }

    void ApplyMaterials(Transform root)
    {
        foreach (Renderer item in root.GetComponentsInChildren<Renderer>(true))
        {
            string name = item.gameObject.name;
            int separator = name.LastIndexOf(" | ");
            if (separator >= 0) item.sharedMaterial = GetMaterial(name.Substring(separator + 3));
        }

        Transform station = FindStation(root.name.Replace(" Display", string.Empty));
        Transform pedestal = station != null ? FindDescendant(station, "Pedestal") : null;
        if (pedestal != null && pedestal.TryGetComponent(out MeshRenderer pedestalRenderer))
            pedestalRenderer.sharedMaterial = GetMaterial("plinth");
    }

    Transform FindStation(string name)
    {
        Dictionary<string, Transform> stations = FindStations(gameObject.scene);
        return stations.TryGetValue(name, out Transform station) ? station : null;
    }

    static Material GetMaterial(string key)
    {
        if (Materials.TryGetValue(key, out Material cached)) return cached;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader) { name = "Museum " + key };
        Color color = key switch
        {
            "gold" => new Color(0.93f, 0.59f, 0.16f),
            "silver" => new Color(0.62f, 0.73f, 0.78f),
            "dark" => new Color(0.055f, 0.12f, 0.18f),
            "blue" => new Color(0.08f, 0.32f, 0.56f),
            "shield" => new Color(0.28f, 0.62f, 0.68f),
            "cyan" => new Color(0.13f, 0.82f, 0.92f),
            "plinth" => new Color(0.06f, 0.14f, 0.2f),
            _ => new Color(0.86f, 0.88f, 0.84f)
        };
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        else material.color = color;
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", key == "gold" || key == "silver" ? 0.55f : 0.18f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.55f);
        Materials[key] = material;
        return material;
    }
}
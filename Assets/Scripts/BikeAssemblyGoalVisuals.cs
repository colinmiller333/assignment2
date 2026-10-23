using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public sealed class BikeAssemblyGoalVisuals : MonoBehaviour
{
    [SerializeField, Range(0.05f, 0.6f)] float opacity = 0.2f;
    [SerializeField] Color tint = new Color(0.35f, 0.82f, 1f, 1f);

    readonly List<Material> ghostMaterials = new List<Material>();

    void Awake()
    {
        foreach (Renderer partRenderer in GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = partRenderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material source = materials[i];
                if (source == null) continue;

                Material ghost = new Material(source)
                {
                    name = source.name + " (Assembly Goal Ghost)"
                };
                SetGhostAppearance(ghost);
                materials[i] = ghost;
                ghostMaterials.Add(ghost);
            }

            partRenderer.sharedMaterials = materials;
            partRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }

    void OnDestroy()
    {
        foreach (Material ghost in ghostMaterials)
        {
            if (ghost != null) Destroy(ghost);
        }
        ghostMaterials.Clear();
    }

    void SetGhostAppearance(Material material)
    {
        Color baseColor = GetBaseColor(material);
        baseColor = Color.Lerp(baseColor, tint, 0.3f);
        baseColor.a = opacity;
        SetColor(material, "baseColorFactor", baseColor);
        SetColor(material, "_BaseColor", baseColor);
        SetColor(material, "_Color", baseColor);

        SetFloat(material, "_Surface", 1f);
        SetFloat(material, "_BUILTIN_Surface", 1f);
        SetFloat(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
        SetFloat(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        SetFloat(material, "_AlphaSrcBlend", (float)BlendMode.One);
        SetFloat(material, "_AlphaDstBlend", (float)BlendMode.OneMinusSrcAlpha);
        SetFloat(material, "_ZWrite", 0f);
        SetFloat(material, "_BUILTIN_ZWrite", 0f);
        SetFloat(material, "_AlphaClip", 0f);
        SetFloat(material, "_BUILTIN_AlphaClip", 0f);

        if (material.HasProperty("_Surface"))
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        if (material.HasProperty("_BUILTIN_Surface"))
            material.EnableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");
        if (material.HasProperty("baseColorFactor"))
            material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHATEST_ON");
        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)RenderQueue.Transparent;
    }

    static Color GetBaseColor(Material material)
    {
        if (material.HasProperty("baseColorFactor")) return material.GetColor("baseColorFactor");
        if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
        if (material.HasProperty("_Color")) return material.GetColor("_Color");
        return Color.white;
    }

    static void SetColor(Material material, string property, Color color)
    {
        if (material.HasProperty(property))
            material.SetColor(property, color);
    }

    static void SetFloat(Material material, string property, float value)
    {
        if (material.HasProperty(property))
            material.SetFloat(property, value);
    }
}

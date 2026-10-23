using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class BikeAssemblyLayoutBuilder
{
    const string ScenePath = "Assets/Scenes/assignment3.unity";
    const string BikeAssetPath = "Assets/Prefabs/childrens_bike.glb";

    static readonly string[][] PartMeshes =
    {
        new[] { "Object_11", "Object_30", "Object_34" },
        new[] { "Object_6", "Object_7" },
        new[] { "Object_4", "Object_9", "Object_13", "Object_15" },
        new[] { "Object_17", "Object_18" },
        new[] { "Object_20", "Object_36" },
        new[] { "Object_54", "Object_56" },
        new[] { "Object_32", "Object_38", "Object_39", "Object_40", "Object_42", "Object_44" },
        new[] { "Object_22", "Object_24", "Object_26", "Object_28", "Object_46", "Object_48", "Object_50", "Object_52" }
    };

    static readonly string[] PartNames =
    {
        "01 Frame",
        "02 Rear Wheel",
        "03 Front Fork",
        "04 Front Wheel",
        "05 Seat Assembly",
        "06 Handlebars",
        "07 Pedals and Drive",
        "08 Brakes and Fittings"
    };

    [InitializeOnLoadMethod]
    static void Initialize()
    {
        EditorApplication.delayCall += BuildOnce;
    }

    static void BuildOnce()
    {
        try
        {
            BuildLayout();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    [MenuItem("Tools/VR Assembly/Arrange Bike into Eight Parts")]
    static void ArrangeFromMenu()
    {
        BuildOnce();
    }

    static void BuildLayout()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.isDirty)
        {
            Debug.LogError("Bike layout was not changed because the active scene has unsaved changes. Save it and run the setup again.");
            return;
        }

        if (scene.path != ScenePath)
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        if (FindSceneObject(scene, "Bike Assembly Parts") != null)
        {
            Debug.Log("Bike assembly layout is already present; no changes made.");
            return;
        }

        GameObject bike = FindSceneObject(scene, "childrens_bike");
        GameObject table = FindSceneObject(scene, "Table_smpl");
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(BikeAssetPath);
        if (bike == null || table == null || model == null)
            throw new InvalidOperationException("The assignment scene must contain childrens_bike and Table_smpl, and the bike model asset must be available.");

        Renderer[] tableRenderers = table.GetComponentsInChildren<Renderer>(true);
        if (tableRenderers.Length == 0)
            throw new InvalidOperationException("Table_smpl has no renderers, so its surface could not be located.");

        Bounds tableBounds = GetCombinedBounds(tableRenderers);
        Vector3 bikePosition = bike.transform.position;
        Quaternion bikeRotation = bike.transform.rotation;
        Vector3 bikeScale = bike.transform.localScale;
        var meshNames = new HashSet<string>(PartMeshes.SelectMany(names => names));
        if (meshNames.Count != PartMeshes.Sum(names => names.Length))
            throw new InvalidOperationException("A bike mesh is assigned to more than one assembly part.");
        Renderer[] bikeRenderers = bike.GetComponentsInChildren<Renderer>(true);
        var sourceMeshNames = new HashSet<string>(bikeRenderers.Select(renderer => renderer.gameObject.name));
        if (sourceMeshNames.Count != bikeRenderers.Length || !meshNames.SetEquals(sourceMeshNames))
            throw new InvalidOperationException("The eight part groups do not cover every unique bike mesh.");

        var layout = new GameObject("Bike Assembly Parts");
        SceneManager.MoveGameObjectToScene(layout, scene);
        var placedParts = new List<GameObject>();

        try
        {
            for (int i = 0; i < PartMeshes.Length; i++)
            {
                GameObject part = PrefabUtility.InstantiatePrefab(model, scene) as GameObject;
                if (part == null)
                    throw new InvalidOperationException("Unity could not create a scene instance of the bike model.");

                placedParts.Add(part);
                part.transform.SetPositionAndRotation(bikePosition, bikeRotation);
                part.transform.localScale = bikeScale;
                PrefabUtility.UnpackPrefabInstance(part, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                Renderer[] renderers = part.GetComponentsInChildren<Renderer>(true);
                var selected = new HashSet<string>(PartMeshes[i]);
                var found = new HashSet<string>();
                foreach (Renderer renderer in renderers)
                {
                    if (selected.Contains(renderer.gameObject.name))
                        found.Add(renderer.gameObject.name);
                    else
                        UnityEngine.Object.DestroyImmediate(renderer.gameObject);
                }

                if (!selected.SetEquals(found))
                    throw new InvalidOperationException(
                        "Bike model mesh names did not match the expected parts. Missing: " +
                        string.Join(", ", selected.Except(found)));

                RemoveEmptyNodes(part.transform);
                part.name = PartNames[i];
                part.transform.SetParent(layout.transform, true);
            }

            for (int i = 0; i < placedParts.Count; i++)
            {
                Bounds partBounds = GetCombinedBounds(placedParts[i].GetComponentsInChildren<Renderer>(true));
                int column = i % 4;
                int row = i / 4;
                float x = Mathf.Lerp(
                    tableBounds.min.x + partBounds.extents.x,
                    tableBounds.max.x - partBounds.extents.x,
                    (column + 0.5f) / 4f);
                float z = Mathf.Lerp(
                    tableBounds.min.z + partBounds.extents.z,
                    tableBounds.max.z - partBounds.extents.z,
                    (row + 0.5f) / 2f);
                float y = tableBounds.max.y + partBounds.extents.y + 0.015f;

                placedParts[i].transform.position += new Vector3(
                    x - partBounds.center.x,
                    y - partBounds.center.y,
                    z - partBounds.center.z);
            }

            UnityEngine.Object.DestroyImmediate(bike);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("Unity could not save the updated assignment scene.");

            Debug.Log("Created eight separated bike assemblies on Table_smpl and saved assignment3.unity.");
        }
        catch
        {
            foreach (GameObject part in placedParts)
                if (part != null) UnityEngine.Object.DestroyImmediate(part);
            if (layout != null) UnityEngine.Object.DestroyImmediate(layout);
            throw;
        }
    }

    static GameObject FindSceneObject(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name) return root;
            Transform child = root.transform.Find(name);
            if (child != null) return child.gameObject;
        }
        return null;
    }

    static Bounds GetCombinedBounds(Renderer[] renderers)
    {
        if (renderers.Length == 0)
            throw new InvalidOperationException("An assembly part has no visible mesh renderers.");

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    static void RemoveEmptyNodes(Transform root)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true)
            .OrderByDescending(transform => transform.GetComponentsInChildren<Transform>(true).Length)
            .ToArray();

        foreach (Transform transform in transforms)
        {
            if (transform == root || transform.childCount != 0) continue;
            if (transform.GetComponent<Renderer>() == null && transform.GetComponent<MeshFilter>() == null)
                UnityEngine.Object.DestroyImmediate(transform.gameObject);
        }
    }
}

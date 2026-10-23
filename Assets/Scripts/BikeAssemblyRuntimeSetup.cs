using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Interaction;

static class BikeAssemblyRuntimeSetup
{
    const string GoalRootName = "Bike Assembly Goal";
    const string PartsRootName = "Bike Assembly Parts";

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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded += ConfigureScene;
        ConfigureScene(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static void ConfigureScene(Scene scene, LoadSceneMode mode)
    {
        Transform goalRoot = FindInScene(scene, GoalRootName);
        if (goalRoot == null) return;

        Transform partsRoot = FindInScene(scene, PartsRootName);
        if (partsRoot == null)
        {
            Debug.LogError("Bike assembly scene is missing the Bike Assembly Parts object.");
            return;
        }

        if (goalRoot.GetComponent<BikeAssemblyGoalVisuals>() == null)
            goalRoot.gameObject.AddComponent<BikeAssemblyGoalVisuals>();

        ItemPlacementManager placementManager = FindPlacementManager(scene);
        if (placementManager == null)
            placementManager = partsRoot.gameObject.AddComponent<ItemPlacementManager>();

        placementManager.ConfigureSequence(PartNames);
        BikeAssemblyProgressScreen progressScreen = FindInstructionsPanel(scene);
        if (progressScreen != null)
            progressScreen.Initialize(placementManager);
        else
            Debug.LogError("Bike assembly scene is missing the Bike Assembly Instructions panel (BikeAssemblyProgressScreen).");

        for (int i = 0; i < PartNames.Length; i++)
        {
            Transform sourcePart = partsRoot.Find(PartNames[i]);
            Transform goalPart = goalRoot.Find(PartNames[i]);
            if (sourcePart == null || goalPart == null)
            {
                Debug.LogError($"Bike assembly is missing '{PartNames[i]}' under its parts or goal object.");
                continue;
            }

            ConfigureSourcePart(sourcePart, goalPart, i + 1);
        }
    }

    static void ConfigureSourcePart(Transform sourcePart, Transform goalPart, int order)
    {
        Rigidbody body = sourcePart.GetComponent<Rigidbody>();
        if (body == null)
        {
            body = sourcePart.gameObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        BoxCollider boxCollider = sourcePart.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = sourcePart.gameObject.AddComponent<BoxCollider>();
            SetColliderBounds(boxCollider, sourcePart);
        }
        boxCollider.enabled = true;

        GrabInteractable grabInteractable = sourcePart.GetComponent<GrabInteractable>();
        if (grabInteractable == null)
            grabInteractable = sourcePart.gameObject.AddComponent<GrabInteractable>();
        grabInteractable.InjectAllGrabInteractable(body);

        Grabbable grabbable = sourcePart.GetComponent<Grabbable>();
        if (grabbable == null)
            grabbable = sourcePart.gameObject.AddComponent<Grabbable>();
        grabbable.InjectOptionalTargetTransform(sourcePart);
        grabbable.InjectOptionalRigidbody(body);
        grabInteractable.InjectOptionalPointableElement(grabbable);

        CheckTricycle placement = sourcePart.GetComponent<CheckTricycle>();
        if (placement == null)
            placement = sourcePart.gameObject.AddComponent<CheckTricycle>();
        placement.targetItem = goalPart;
        placement.itemOrder = order;

        if (sourcePart.GetComponent<AudioSource>() == null)
            sourcePart.gameObject.AddComponent<AudioSource>();
    }

    static void SetColliderBounds(BoxCollider collider, Transform part)
    {
        Renderer[] renderers = part.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            Debug.LogError($"Bike part '{part.name}' has no renderers to size its grab collider.");
            return;
        }

        Bounds worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            worldBounds.Encapsulate(renderers[i].bounds);

        Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        Vector3 center = worldBounds.center;
        Vector3 extents = worldBounds.extents;
        for (int x = -1; x <= 1; x += 2)
        for (int y = -1; y <= 1; y += 2)
        for (int z = -1; z <= 1; z += 2)
        {
            Vector3 corner = center + Vector3.Scale(extents, new Vector3(x, y, z));
            Vector3 localCorner = part.InverseTransformPoint(corner);
            min = Vector3.Min(min, localCorner);
            max = Vector3.Max(max, localCorner);
        }

        collider.center = (min + max) * 0.5f;
        collider.size = Vector3.Max(max - min, Vector3.one * 0.05f);
    }

    static ItemPlacementManager FindPlacementManager(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            ItemPlacementManager manager = root.GetComponentInChildren<ItemPlacementManager>(true);
            if (manager != null) return manager;
        }
        return null;
    }

    static BikeAssemblyProgressScreen FindInstructionsPanel(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            BikeAssemblyProgressScreen panel = root.GetComponentInChildren<BikeAssemblyProgressScreen>(true);
            if (panel != null) return panel;
        }
        return null;
    }

    static Transform FindInScene(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == objectName) return root.transform;
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == objectName) return child;
            }
        }
        return null;
    }
}

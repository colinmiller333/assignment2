using UnityEngine;
using UnityEngine.XR;

public class MuseumPlayModeBootstrap : MonoBehaviour
{
    [SerializeField] GameObject desktopPlayerPrefab;
    [SerializeField] GameObject xrRig;
    [SerializeField] Vector3 desktopSpawnPosition = new Vector3(0f, 0.05f, 0f);

    void Start()
    {
        if (XRSettings.isDeviceActive) return;
        if (desktopPlayerPrefab == null)
        {
            Debug.LogError("Museum desktop preview is missing the Starter Assets PlayerCapsule prefab.");
            return;
        }

        if (xrRig != null) xrRig.SetActive(false);

        GameObject cameraObject = new GameObject("Desktop Test Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = desktopSpawnPosition + Vector3.up * 1.375f;
        cameraObject.transform.rotation = Quaternion.identity;
        Camera playerCamera = cameraObject.GetComponent<Camera>();
        playerCamera.clearFlags = CameraClearFlags.Skybox;
        playerCamera.nearClipPlane = 0.1f;
        playerCamera.farClipPlane = 150f;
        playerCamera.fieldOfView = 70f;

        GameObject player = Instantiate(desktopPlayerPrefab, desktopSpawnPosition, Quaternion.identity);
        player.name = "Desktop Test Player";
        Transform cameraTarget = FindDescendant(player.transform, "PlayerCameraRoot");
        if (cameraTarget == null)
        {
            Debug.LogError("Starter Assets PlayerCapsule is missing PlayerCameraRoot.");
            Destroy(cameraObject);
            Destroy(player);
            if (xrRig != null) xrRig.SetActive(true);
            return;
        }

        cameraObject.transform.SetParent(cameraTarget, false);
        cameraObject.transform.localPosition = Vector3.zero;
        cameraObject.transform.localRotation = Quaternion.identity;

        Transform capsule = FindDescendant(player.transform, "Capsule");
        if (capsule != null && capsule.TryGetComponent(out MeshRenderer capsuleRenderer))
            capsuleRenderer.enabled = false;
    }

    static Transform FindDescendant(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            if (child != parent && child.name == childName) return child;
        return null;
    }
}
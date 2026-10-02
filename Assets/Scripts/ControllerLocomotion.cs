using UnityEngine;
using UnityEngine.XR;

[DisallowMultipleComponent]
public class ControllerLocomotion : MonoBehaviour
{
    [SerializeField] float movementSpeed = 1.5f;
    [SerializeField] float snapTurnAngle = 45f;
    [SerializeField] float snapTurnCooldown = 0.25f;
    [SerializeField, Range(0f, 1f)] float stickDeadzone = 0.2f;

    Transform head;
    float nextSnapTurnTime;

    void Update()
    {
        if (head == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;
            head = mainCamera.transform;
        }

        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftController.isValid &&
            leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 movement) &&
            movement.sqrMagnitude > stickDeadzone * stickDeadzone)
        {
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(head.right, Vector3.up).normalized;
            transform.position += (forward * movement.y + right * movement.x) * movementSpeed * Time.deltaTime;
        }

        InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (Time.time >= nextSnapTurnTime && rightController.isValid &&
            rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 turn) &&
            Mathf.Abs(turn.x) > stickDeadzone)
        {
            transform.RotateAround(head.position, Vector3.up, -Mathf.Sign(turn.x) * snapTurnAngle);
            nextSnapTurnTime = Time.time + snapTurnCooldown;
        }
    }
}
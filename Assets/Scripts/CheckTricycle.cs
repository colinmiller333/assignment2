using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine;

public class CheckTricycle : MonoBehaviour
{
    public Transform targetItem;
    public int itemOrder = 1;

    public bool isSolved;

    private AudioSource audioSource;
    private Rigidbody rigidBody;
    private BoxCollider boxCollider;
    private GrabInteractable grabInteractable;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        grabInteractable = GetComponent<GrabInteractable>();

        if (targetItem == null)
            Debug.LogError($"Bike part '{name}' has no assembly goal target.", this);
    }

    void Update()
    {
        if (isSolved || targetItem == null || ItemPlacementManager.Instance == null) return;
        if (!ItemPlacementManager.Instance.CanPlaceItem(itemOrder) ||
            Vector3.Distance(transform.position, targetItem.position) >= 0.05f)
            return;

        if (grabInteractable == null)
        {
            Debug.LogError($"Bike part '{name}' is missing its grab interactable.", this);
            return;
        }

        var interactors = new List<GrabInteractor>(grabInteractable.Interactors);
        foreach (GrabInteractor interactor in interactors)
            interactor.Unselect();

        transform.SetPositionAndRotation(targetItem.position, targetItem.rotation);
        targetItem.gameObject.SetActive(false);

        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.isKinematic = true;
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }

        if (boxCollider != null)
            boxCollider.enabled = false;

        grabInteractable.enabled = false;
        if (audioSource != null)
            audioSource.Play();

        isSolved = true;
        ItemPlacementManager.Instance.CompletePlacement();
    }
}

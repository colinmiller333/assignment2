using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

// Updates the text on the "Bike Assembly Instructions" panel in the scene.
// Layout, position, colors, and fonts are all edited on the scene objects.
[DisallowMultipleComponent]
public sealed class BikeAssemblyProgressScreen : MonoBehaviour
{
    [Header("Scene text objects")]
    [SerializeField] TMP_Text headingText;
    [SerializeField] TMP_Text partText;
    [SerializeField] TMP_Text progressText;

    [Header("Wording")]
    [SerializeField] string nextPartHeading = "NEXT PART TO PLACE";
    [Tooltip("{0} = current step, {1} = total steps")]
    [SerializeField] string progressFormat = "Step {0} of {1} - place it on the see-through bike";
    [SerializeField] string completeHeading = "ASSEMBLY COMPLETE";
    [SerializeField] string completeMessage = "Bike complete!";
    [Tooltip("{0} = total steps")]
    [SerializeField] string completeProgressFormat = "All {0} parts assembled.";

    ItemPlacementManager placementManager;

    public void Initialize(ItemPlacementManager manager)
    {
        if (manager == null)
        {
            Debug.LogError("Bike assembly instructions panel requires an ItemPlacementManager.", this);
            return;
        }

        if (headingText == null || partText == null || progressText == null)
        {
            Debug.LogError("Assign the Heading, Part, and Progress text objects on the bike assembly instructions panel.", this);
            return;
        }

        if (placementManager != null)
            placementManager.ProgressChanged -= UpdateProgress;

        placementManager = manager;
        placementManager.ProgressChanged += UpdateProgress;
        UpdateProgress(placementManager.currentOrder);
    }

    void OnDestroy()
    {
        if (placementManager != null)
            placementManager.ProgressChanged -= UpdateProgress;
    }

    void UpdateProgress(int currentOrder)
    {
        int totalParts = placementManager.TotalParts;
        if (currentOrder > totalParts)
        {
            headingText.text = completeHeading;
            partText.text = completeMessage;
            progressText.text = string.Format(completeProgressFormat, totalParts);
            return;
        }

        string partName = placementManager.GetCurrentPartName();
        if (string.IsNullOrEmpty(partName))
        {
            Debug.LogError($"No assembly part is configured for step {currentOrder}.", this);
            return;
        }

        headingText.text = nextPartHeading;
        partText.text = Regex.Replace(partName, @"^\d+\s+", "");
        progressText.text = string.Format(progressFormat, currentOrder, totalParts);
    }
}

using System;
using UnityEngine;

public class ItemPlacementManager : MonoBehaviour
{
    public static ItemPlacementManager Instance;
    public int currentOrder = 1;

    string[] partNames = Array.Empty<string>();
    public event Action<int> ProgressChanged;

    private void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ConfigureSequence(string[] sequence)
    {
        if (sequence == null || sequence.Length == 0)
        {
            Debug.LogError("Bike assembly sequence must contain at least one part.", this);
            return;
        }

        partNames = (string[])sequence.Clone();
        currentOrder = Mathf.Clamp(currentOrder, 1, partNames.Length + 1);
        ProgressChanged?.Invoke(currentOrder);
    }

    public int TotalParts => partNames.Length;

    public string GetCurrentPartName()
    {
        if (currentOrder < 1 || currentOrder > partNames.Length)
            return null;
        return partNames[currentOrder - 1];
    }

    public bool CanPlaceItem(int itemOrder)
    {
        return itemOrder == currentOrder;
    }

    public void CompletePlacement()
    {
        if (partNames.Length > 0 && currentOrder > partNames.Length)
            return;

        currentOrder++;
        ProgressChanged?.Invoke(currentOrder);
    }
}

using UnityEngine;

public class ItemPlacementManager : MonoBehaviour
{
    public static ItemPlacementManager Instance;
    public int currentOrder = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Awake()
    {
        Instance = this;
    }

    public bool CanPlaceItem(int itemOrder)
    {
        return itemOrder == currentOrder;
    }

    public void CompletePlacement()
    {
        currentOrder++;
    }
}

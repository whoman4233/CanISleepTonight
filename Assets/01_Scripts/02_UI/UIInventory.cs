using UnityEngine;

public class UIInventory : MonoBehaviour
{
    [Header("인벤토리 UI")]
    [SerializeField] private GameObject inventoryPanel;

    public bool IsInventoryOpen => inventoryPanel.activeInHierarchy;

    public void ToggleInventory()
    {
        if (IsInventoryOpen) 
            CloseInventory();
        else 
            OpenInventory();
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }
}

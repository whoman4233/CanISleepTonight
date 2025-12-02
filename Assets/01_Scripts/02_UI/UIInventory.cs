using System.Collections.Generic;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    [Header("인벤토리 UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotParent;

    private List<UISlot> slots = new List<UISlot>();
    private Item selectedItem;

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

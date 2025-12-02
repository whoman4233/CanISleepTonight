using System.Collections.Generic;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    [Header("인벤토리 UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private UIItemDetail detailPanel;
    [SerializeField] private Transform slotParent;

    private List<UISlot> slots = new List<UISlot>();
    private Item selectedItem;

    public bool IsInventoryOpen => inventoryPanel.activeInHierarchy;

    private void Start()
    {
        // 1) 슬롯 자동 수집
        slots.AddRange(slotParent.GetComponentsInChildren<UISlot>());

        // 2) 슬롯 초기화
        foreach (var slot in slots)
            slot.Init(this);

        detailPanel.gameObject.SetActive(false);
    }

    public void ToggleInventory()
    {
        if (IsInventoryOpen) 
            CloseInventory();
        else 
            OpenInventory();
    }

    public void OpenInventory()
    {
        detailPanel.gameObject.SetActive(false);
        inventoryPanel.SetActive(true);
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }

    public void OnSlotSelected(UISlot slot)
    {
        if (slot.currentItem == null) return;

        detailPanel.Show(slot.currentItem);
    }

    public void OnBackClicked()
    {
        if (detailPanel.gameObject.activeInHierarchy)
            detailPanel.gameObject.SetActive(false);
        else
            CloseInventory();
    }
}

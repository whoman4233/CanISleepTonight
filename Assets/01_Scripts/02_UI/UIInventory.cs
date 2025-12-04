using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    [Header("인벤토리 UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private UIItemDetail detailPanel;
    [SerializeField] private Transform slotParent;
    [SerializeField] private TextMeshProUGUI balanceText;

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
        UpdateSlotUI();
        detailPanel.gameObject.SetActive(false);
        inventoryPanel.SetActive(true);
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        UIManager.Instance.ToggleCursor();
    }

    public void OnSlotSelected(UISlot slot)
    {
        if (slot.currentItem == null) return;

        detailPanel.Show(slot.currentItem);
    }

    public void OnBackClicked()
    {
        // 아이템 설명 패널이 켜져있을 경우 => 아이템 설명 패널만 off
        if (detailPanel.gameObject.activeInHierarchy)
        {
            detailPanel.gameObject.SetActive(false);
            UpdateSlotUI();
        }

        // 아이템 설명 패널이 켜져있지 않을 경우 (아이템 슬롯만 켜져있을 경우) => 인벤토리창 자체를 off
        else
            CloseInventory();
    }

    public void UpdateBalanceUI(int balance)
    {
        balanceText.text = $"통장 잔고 : {balance}\u20a9";
    }

    private void UpdateSlotUI()
    {
        foreach (var slot in slots)
            slot.UpdateSlot();
    }
}

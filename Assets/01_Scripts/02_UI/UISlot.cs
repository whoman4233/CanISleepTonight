using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISlot : MonoBehaviour
{
    [SerializeField] private Image lockIcon;
    [SerializeField] private Button slotButton;

    [SerializeField] private ItemData itemData;

    public int slotIndex;
    public Item currentItem;

    private UIInventory uiInventory;

    public void Init(UIInventory inv)
    {
        uiInventory = inv;
        currentItem = new Item(itemData);

        lockIcon.gameObject.SetActive(currentItem.IsLocked);
    }

    public void OnClickSlot()
    {
        uiInventory.OnSlotSelected(this);
    }

    public void UpdateSlot()
    {
        if (currentItem.IsLocked)
            lockIcon.gameObject.SetActive(true);
        else
            lockIcon.gameObject.SetActive(false);
    }
}

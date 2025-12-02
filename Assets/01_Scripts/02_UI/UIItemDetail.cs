using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemDetail : MonoBehaviour
{
    [Header("Item Detail UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;

    [Header("EquipButton")]
    [SerializeField] private Button EquipButton;
    [SerializeField] private TextMeshProUGUI equipButtonText;

    [Header("PurchaseButton")]
    [SerializeField] private Button PurchaseButton;
    [SerializeField] private TextMeshProUGUI purchaseButtonText;

    public void Show(Item item)
    {
        iconImage.sprite = item.ItemData.itemIcon;
        nameText.text = item.ItemData.itemName;
        descText.text = item.ItemData.itemDescription;

        if (item.IsLocked)
            SetPurchaseButton(item);
        else
            SetEquipButton(item);

        gameObject.SetActive(true);
    }

    private void SetEquipButton(Item item)
    {
        if (item.IsEquipped)
            equipButtonText.text = "해제";
        else
            equipButtonText.text = "장착";

        EquipButton.gameObject.SetActive(true);
        PurchaseButton.gameObject.SetActive(false);
    }

    private void SetPurchaseButton(Item item)
    {
        purchaseButtonText.text = $"구매 : {item.ItemData.itemPrice}\u20a9";

        PurchaseButton.gameObject.SetActive(true);
        EquipButton.gameObject.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnEquipButtonClicked()
    {

    }

    public void OnPurchaseButtonClicked()
    {

    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemDetail : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;

    public void Show(Item item)
    {
        iconImage.sprite = item.ItemData.itemIcon;
        nameText.text = item.ItemData.itemName;
        descText.text = item.ItemData.itemDescription;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

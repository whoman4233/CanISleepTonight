using UnityEngine;

/// <summary>
/// 아이템 정적 데이터 (런타임에 변하지 않는 정보들)
/// </summary>
[CreateAssetMenu(fileName = "ItemData_", menuName = "ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Display Info")]
    public string itemName;
    public string itemDescription;
    public int itemPrice;
    public Sprite itemIcon;
}

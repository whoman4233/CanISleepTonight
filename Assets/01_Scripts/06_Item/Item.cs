using UnityEngine;

/// <summary>
/// 아이템 동적 데이터 (런타임에 변할 수 있는 정보들)
/// </summary>
public class Item
{
    public ItemData ItemData { get; private set; }

    public bool IsUnlocked { get; private set; }    // 해금되었다면? true

    public bool IsEquipped { get; private set; }

    public Item(ItemData data)
    {
        ItemData = data;
        IsUnlocked = true;
        IsEquipped = false;
    }
}

using UnityEngine;

/// <summary>
/// 아이템 동적 데이터 (런타임에 변할 수 있는 정보들)
/// </summary>
public class Item
{
    public ItemData ItemData { get; private set; }

    public bool IsLocked { get; private set; }    // 해금되었다면? false

    public bool IsEquipped { get; private set; }

    public Item(ItemData data)
    {
        ItemData = data;
        IsLocked = true;
        IsEquipped = false;
    }

    public void UnlockItem()
    {
        IsLocked = false;
    }

    public void EquipItem()
    {
        IsEquipped = true;
    }

    public void UnEquipItem()
    {
        IsEquipped = false;
    }
}

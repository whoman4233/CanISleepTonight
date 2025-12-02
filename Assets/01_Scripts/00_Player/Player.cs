using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Player : MonoBehaviour
{
    public PlayerController controller;
    public PlayerCondition condition;
    //public Equipment equip;

    //public ItemData itemData;
    public Action addItem;

    public Transform dropPosition;

    private List<Item> inventory = new List<Item>();
    public List<Item> Inventory {  get { return inventory; } }

    private int balance = 30000;

    public int Balance { get { return balance; } }

    private void Awake()
    {
        PlayerManager.Instance.Player = this;
        controller = GetComponent<PlayerController>();
        condition = GetComponent<PlayerCondition>();
        //equip = GetComponent<Equipment>();
    }

    public Item GetEquippedItem()
    {
        return inventory.FirstOrDefault(item => item.IsEquipped);
    }

    public void PuchaseItem(Item item)
    {
        if (balance < item.ItemData.itemPrice) return;  // 잔액 부족 => 구매 불가

        balance -= item.ItemData.itemPrice;
        UIManager.Instance.UIInventory.UpdateBalanceUI(balance);

        item.UnlockItem();
        UIManager.Instance.UIItemDetail.UpdateButtonUI();

        inventory.Add(item);
    }


}
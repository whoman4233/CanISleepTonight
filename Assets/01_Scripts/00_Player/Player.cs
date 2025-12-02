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
}
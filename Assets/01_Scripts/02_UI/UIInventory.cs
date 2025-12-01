using UnityEngine;

public class UIInventory : MonoBehaviour
{
    [Header("인벤토리 UI 오브젝트")]
    [SerializeField] private GameObject testInventory;

    private PlayerController controller;

    void Start()
    {
        controller = PlayerManager.Instance.Player.controller;
        controller.inventory += ToggleInventory;

        testInventory.SetActive(false);
    }

    void ToggleInventory()
    {
        if (IsInventoryOpen())
            testInventory.SetActive(false);
        else
            testInventory.SetActive(true);
    }

    public bool IsInventoryOpen()
    {
        return testInventory.activeInHierarchy;
    }
}

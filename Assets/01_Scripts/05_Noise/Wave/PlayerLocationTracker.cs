using UnityEngine;

public class PlayerLocationTracker : MonoBehaviour
{
    [Header("Runtime State")]
    public int currentFloor = 0;

    [Tooltip("집 안일 경우 houseSlotId, 복도일 경우 빈 문자열")]
    public string currentHouseSlotId = "";

    public bool IsInsideHouse => !string.IsNullOrEmpty(currentHouseSlotId);

    // Player가 집 내부에 들어왔을 때
    public void EnterHouse(string houseSlotId)
    {
        currentHouseSlotId = houseSlotId;
    }

    // Player가 집 내부에서 나갔을 때
    public void ExitHouse()
    {
        currentHouseSlotId = "";
    }

    // Player가 층 이동(계단/엘리베이터/전이 등)
    public void SetFloor(int floor)
    {
        currentFloor = floor;
    }
}

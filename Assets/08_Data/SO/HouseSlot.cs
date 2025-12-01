using UnityEngine;

public class HouseSlot : MonoBehaviour
{
    [Header("Identification")]
    [Tooltip("호수 ID (예: 303, 201 등)")]
    public string houseSlotId;

    [Tooltip("층 정보 (예: 3층이면 3)")]
    public int floor;

    [Header("Prefab Placement")]
    [Tooltip("집 인테리어가 인스턴스될 기준 Transform")]
    public Transform interiorRoot;

    [Tooltip("복도 기준 문 위치 (파동 표시용)")]
    public Transform doorPoint;

    public Transform InteriorRoot => interiorRoot;
    public Transform DoorPoint => doorPoint;
}

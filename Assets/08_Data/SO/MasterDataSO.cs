using UnityEngine;

[CreateAssetMenu(fileName = "MasterGameData", menuName = "GameData/Master Game Data")]
public class MasterGameDataSO : ScriptableObject
{
    [Header("Core Tables")]
    public NeighborTableSO neighborTable;
    public DistractionTableSO distractionTable;

    [Header("Layout / Prefab Mapping")]
    public HouseLayoutTableSO houseLayoutTable;

    public PlaceTableSO placeTable;

    // 이후 필요시 확장용 (지금은 비워두고, 타입만 확보)
    [Header("Optional (Later)")]
    public ScriptableObject entityTable;
    public ScriptableObject dayConfigTable;
}
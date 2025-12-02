// NeighborRuntime 쪽 (필드 하나 추가)
using System.Collections.Generic;
using UnityEngine;

public class NeighborRuntime
{
    public readonly NeighborDataRow data;

    public string Id => data.neighborId;

    public HouseSlot houseSlot;
    public GameObject houseInstance;

    // 대표 위치
    public string placeId;   // 이웃의 "집" 대표 PlaceID (예: 그 집 문/거실)

    public bool isAlive = true;
    public bool isActiveToday = false;

    public List<DistractionRuntime> distractions = new();

    public NeighborRuntime(NeighborDataRow row)
    {
        data = row;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

// ============================
// Neighbor
// ============================

[Serializable]
public class NeighborDataRow
{
    [Tooltip("Neighbor ID (예: N_001)")]
    public string neighborId;

    [Tooltip("표시용 이름 (예: 근육뇌)")]
    public string displayName;

    [Tooltip("기획서에 정의된 LayoutID (예: L_01)")]
    public string layoutId;

    [TextArea]
    public string description;
}

[CreateAssetMenu(fileName = "NeighborTable", menuName = "GameData/Neighbor Table")]
public class NeighborTableSO : ScriptableObject
{
    public List<NeighborDataRow> neighbors = new List<NeighborDataRow>();

    public NeighborDataRow GetById(string id)
    {
        return neighbors.Find(n => n.neighborId == id);
    }
}

// ============================
// Distraction
// ============================

[Serializable]
public class DistractionDataRow
{
    [Tooltip("Distraction ID (예: D_N001_A)")]
    public string distractionId;

    [Tooltip("주인 Neighbor ID (예: N_001)")]
    public string ownerId;

    [Tooltip("실제 소음원을 대표하는 ID (Neighbor or Entity ID 등)")]
    public string sourceId;

    [Tooltip("태그 (예: sound 등)")]
    public string tag;

    [Tooltip("강도 (1~6)")]
    public int intensity;

    [Tooltip("효과음 ID (예: S_001)")]
    public string sfxId;

    [Tooltip("기본 위치 PlaceID (예: P_303). 프리팹에서 Override 가능")]
    public string placeId;

    [TextArea]
    public string description;
}

[CreateAssetMenu(fileName = "DistractionTable", menuName = "GameData/Distraction Table")]
public class DistractionTableSO : ScriptableObject
{
    public List<DistractionDataRow> distractions = new List<DistractionDataRow>();

    public DistractionDataRow GetById(string id)
    {
        return distractions.Find(d => d.distractionId == id);
    }

    public IEnumerable<DistractionDataRow> GetByOwnerId(string ownerId)
    {
        return distractions.FindAll(d => d.ownerId == ownerId);
    }
}
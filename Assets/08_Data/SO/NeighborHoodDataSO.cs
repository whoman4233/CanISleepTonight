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
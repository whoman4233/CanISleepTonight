using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HouseLayoutRow
{
    [Tooltip("레이아웃 ID (예: L_01)")]
    public string layoutId;

    [Tooltip("집/방 인테리어 프리팹 (DistractionAnchor 포함)")]
    public GameObject housePrefab;

    [TextArea]
    public string description;
}
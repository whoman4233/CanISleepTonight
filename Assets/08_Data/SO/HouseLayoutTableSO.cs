using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HouseLayoutTable", menuName = "GameData/House Layout Table")]
public class HouseLayoutTableSO : ScriptableObject
{
    public List<HouseLayoutRow> layouts = new List<HouseLayoutRow>();

    public HouseLayoutRow GetById(string layoutId)
    {
        return this.layouts.Find(l => l.layoutId == layoutId);
    }
}
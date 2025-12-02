using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameData/InteractionTable")]
public class InteractionTableSO : ScriptableObject
{
    public List<InteractionRow> rows = new();
}
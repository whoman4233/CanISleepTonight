using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "GameData/InteractionListTable")]
public class InteractionListTableSO : ScriptableObject
{
    public List<InteractionListRow> rows = new();
}

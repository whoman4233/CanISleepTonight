using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlaceDataRow
{
    [Tooltip("장소 ID (예: P_303, P_202 등)")]
    public string placeId;

    [Tooltip("이 장소가 속한 층 (3층, 4층 등 / 필요 없으면 0으로 두고 무시해도 됨)")]
    public int floor;

    [Tooltip("소음 계산용 거리 레벨 (0 = 가장 가깝고, 값이 클수록 멀어짐)")]
    public int distanceLevel;
}

[CreateAssetMenu(fileName = "PlaceTableSO", menuName = "GameData/PlaceTable")]
public class PlaceTableSO : ScriptableObject
{
    public List<PlaceDataRow> places = new List<PlaceDataRow>();

    // 런타임 캐시
    private Dictionary<string, PlaceDataRow> _map;

    private void BuildCache()
    {
        if (_map != null)
            return;

        _map = new Dictionary<string, PlaceDataRow>();

        for (int i = 0; i < places.Count; i++)
        {
            var row = places[i];
            if (row == null || string.IsNullOrEmpty(row.placeId))
                continue;

            if (_map.ContainsKey(row.placeId))
            {
                Debug.LogWarning($"[PlaceTableSO] Duplicate placeId '{row.placeId}' at index {i}");
                continue;
            }

            _map.Add(row.placeId, row);
        }
    }

    public PlaceDataRow GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        BuildCache();

        PlaceDataRow row;
        return _map != null && _map.TryGetValue(id, out row) ? row : null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 에디터에서 값 바뀔 때마다 캐시 리빌드 되도록
        _map = null;
    }
#endif
}

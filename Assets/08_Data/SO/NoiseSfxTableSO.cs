using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NoiseSfxTable", menuName = "GameData/NoiseSfxTable")]
public class NoiseSfxTableSO : ScriptableObject
{
    public List<NoiseSfxEntry> entries = new List<NoiseSfxEntry>();

    private Dictionary<string, NoiseSfxEntry> _map;

    private void OnEnable()
    {
        BuildMap();
    }

    private void BuildMap()
    {
        _map = new Dictionary<string, NoiseSfxEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in entries)
        {
            if (e == null) continue;
            if (string.IsNullOrWhiteSpace(e.sfxId)) continue;

            var key = e.sfxId.Trim();
            if (_map.ContainsKey(key))
            {
                Debug.LogWarning($"[NoiseSfxTableSO] Duplicate SFXID '{key}'");
                continue;
            }

            _map[key] = e;
        }
    }

    public NoiseSfxEntry GetById(string sfxId)
    {
        if (string.IsNullOrWhiteSpace(sfxId))
            return null;

        if (_map == null || _map.Count == 0)
            BuildMap();

        _map.TryGetValue(sfxId.Trim(), out var entry);
        return entry;
    }
}

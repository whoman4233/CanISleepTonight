using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    [Header("Managers")]
    public NeighborManager neighborManager;

    [Header("Update Settings")]
    [Tooltip("소음 계산 틱 간격 (초)")]
    public float tickInterval = 0.1f;

    [Header("Runtime Noise Value")]
    [Range(0, 100f)]
    public float currentNoise;

    private float _tickTimer;

    // 거리레벨 → 거리계수 하드코딩 (프로토타입 기준)
    private static readonly Dictionary<int, float> distanceCoef = new Dictionary<int, float>()
    {
        {0, 1.5f},
        {1, 1.2f},
        {2, 0.8f},
        {3, 0.4f},
        {4, 0.2f},
        {5, 0.1f},
    };

    // 임시 PlaceID→DistanceLevel 매핑 (나중에 PlaceTableSO로 대체)
    private static int GetDistanceLevel(string placeId)
    {
        if (string.IsNullOrEmpty(placeId))
            return 5;

        if (placeId == "P_303") return 0;
        if (placeId.StartsWith("P_2")) return 2;
        if (placeId.StartsWith("P_3")) return 1;
        if (placeId.StartsWith("P_4")) return 2;
        if (placeId.StartsWith("P_5")) return 3;

        return 5;
    }

    private void Update()
    {
        _tickTimer += Time.deltaTime;
        if (_tickTimer >= tickInterval)
        {
            _tickTimer = 0f;
            CalculateNoise();
        }
    }

    private void CalculateNoise()
    {
        if (neighborManager == null)
            return;

        float totalNoise = 0f;
        var list = neighborManager.ActiveDistractionsToday;

        for (int i = 0; i < list.Count; i++)
        {
            var d = list[i];
            if (!d.isAlive || !d.isActiveToday)
                continue;

            int level = GetDistanceLevel(d.placeId);
            float coef = distanceCoef.ContainsKey(level) ? distanceCoef[level] : 0.1f;

            float noise = d.data.intensity * coef;
            totalNoise += noise;
        }

        currentNoise = Mathf.Clamp(totalNoise, 0f, 100f);
    }

    public float GetCurrentNoise()
    {
        return currentNoise;
    }
}

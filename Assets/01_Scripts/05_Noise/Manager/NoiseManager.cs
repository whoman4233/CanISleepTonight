using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    [Header("Managers")]
    public NeighborManager neighborManager;

    [Header("Debug")]
    [SerializeField] private bool noiseVerboseLog = true;

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
        {
            if (noiseVerboseLog)
                Debug.LogWarning("[NoiseDebug] neighborManager 가 비어있어서 계산을 건너뜁니다.");
            return;
        }

        var list = neighborManager.ActiveDistractionsToday;
        if (list == null || list.Count == 0)
        {
            if (noiseVerboseLog)
                Debug.Log("[NoiseDebug] ActiveDistractionsToday 가 비어있습니다. totalNoise = 0");
            currentNoise = 0f;
            return;
        }

        float totalNoise = 0f;
        int usedCount = 0;

        for (int i = 0; i < list.Count; i++)
        {
            var d = list[i];
            if (d == null)
            {
                if (noiseVerboseLog)
                    Debug.LogWarning($"[NoiseDebug] index={i} Distraction 가 null 입니다.");
                continue;
            }

            // 1) 죽은 소음원
            if (!d.isAlive)
            {
                if (noiseVerboseLog)
                    Debug.Log($"[NoiseDebug] {d.Id} skip: isAlive=false");
                continue;
            }

            // 2) 오늘 비활성 소음원
            if (!d.isActiveToday)
            {
                if (noiseVerboseLog)
                    Debug.Log($"[NoiseDebug] {d.Id} skip: isActiveToday=false");
                continue;
            }

            // 3) 위치/PlaceId 체크
            string placeId = string.IsNullOrEmpty(d.placeId) ? "null-place" : d.placeId;
            int level = GetDistanceLevel(d.placeId);
            if (!distanceCoef.TryGetValue(level, out float coef))
            {
                coef = 0.1f;
            }

            float intensity = d.data != null ? d.data.intensity : 0f;
            float noise = intensity * coef;
            totalNoise += noise;
            usedCount++;

            if (noiseVerboseLog)
            {
                string ownerId = d.owner != null ? d.owner.Id : "null-owner";
                Debug.Log($"[NoiseDebug] + {d.Id} (owner={ownerId}) " +
                          $"alive={d.isAlive}, today={d.isActiveToday}, " +
                          $"place={placeId}, level={level}, intensity={intensity}, coef={coef}, add={noise}");
            }
        }

        currentNoise = Mathf.Clamp(totalNoise, 0f, 100f);

        if (noiseVerboseLog)
        {
            Debug.Log($"[NoiseDebug] === Tick Done: used={usedCount}/{list.Count}, totalNoise={totalNoise}, clamped={currentNoise} ===");
        }
    }

    public float GetCurrentNoise()
    {
        return currentNoise;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    [Header("Managers")]
    public NeighborManager neighborManager;
    [SerializeField] private bool noiseVerboseLog = true;

    [Header("Update Settings")]
    [Tooltip("소음 계산 틱 간격 (초)")]
    public float tickInterval = 0.1f;

    [Header("Runtime Noise Value")]
    [Range(0, 100f)]
    public float currentNoise;

    private float _tickTimer;

    private static readonly Dictionary<int, float> distanceCoef = new Dictionary<int, float>()
    {
        {0, 1.5f},
        {1, 1.2f},
        {2, 0.8f},
        {3, 0.4f},
        {4, 0.2f},
        {5, 0.1f},
    };

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

        if (list == null || list.Count == 0)
        {
            currentNoise = 0f;
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            var d = list[i];

            // 1) 런타임 상태 체크
            if (!d.isAlive || !d.isActiveToday)
            {
                if (noiseVerboseLog)
                {
                    Debug.Log($"[NoiseManager] SKIP {d.Id} alive={d.isAlive}, activeToday={d.isActiveToday}");
                }

                // 혹시 앵커가 있으면, 여기서도 사운드 끄기
                if (d.anchor != null)
                    d.anchor.EnsureAudioForToday(verbose: noiseVerboseLog);

                continue;
            }

            // 2) 소음 계산
            int level = GetDistanceLevel(d.placeId);
            float coef = distanceCoef.ContainsKey(level) ? distanceCoef[level] : 0.1f;

            float noise = d.data.level * coef;
            totalNoise += noise;

            // 3) 소리 재생 보장 + 디버그
            if (d.anchor != null)
            {
                d.anchor.EnsureAudioForToday(verbose: noiseVerboseLog);
            }
            else if (noiseVerboseLog)
            {
                Debug.LogWarning($"[NoiseManager] {d.Id} 에 연결된 DistractionAnchor 없음 (anchor == null)");
            }

            if (noiseVerboseLog)
            {
                Debug.Log(
                    $"[NoiseManager] {d.Id} place={d.placeId}, level={level}, coef={coef}, " +
                    $"intensity={d.data.level}, noise={noise}");
            }
        }

        currentNoise = Mathf.Clamp(totalNoise, 0f, 100f);

        if (noiseVerboseLog)
        {
            Debug.Log($"[NoiseManager] TOTAL={currentNoise}");
        }
    }

    public float GetCurrentNoise()
    {
        return currentNoise;
    }
}

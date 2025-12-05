using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    [Header("Managers")]
    public NeighborManager neighborManager;

    [SerializeField]
    private bool noiseVerboseLog = true;

    [Header("Update Settings")]
    [Tooltip("소음 계산 틱 간격 (초)")]
    public float tickInterval = 0.1f;

    [Header("Runtime Noise Value")]
    [Range(0, 100f)]
    public float currentNoise;

    private float _tickTimer;

    // 거리 레벨 → 거리 계수 (임시 하드코딩)
    private static readonly Dictionary<int, float> distanceCoef = new Dictionary<int, float>()
    {
        {0, 1.5f},
        {1, 1.2f},
        {2, 0.8f},
        {3, 0.4f},
        {4, 0.2f},
        {5, 0.1f},
    };

    // PlaceID → 거리 레벨 (임시 규칙)
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

        var list = neighborManager.ActiveDistractionsToday;
        if (list == null || list.Count == 0)
        {
            currentNoise = 0f;
            if (noiseVerboseLog)
                Debug.Log("[NoiseManager] No active distractions today.");
            return;
        }

        float totalNoise = 0f;

        for (int i = 0; i < list.Count; i++)
        {
            var d = list[i];
            if (d == null)
                continue;

            // 오늘 실제로 소음 내는지 여부
            bool isEmitting =
                d.isAlive &&
                d.isActiveToday &&
                !d.isSilencedToday;

            if (!isEmitting)
            {
                if (noiseVerboseLog)
                {
                    Debug.Log(
                        $"[NoiseManager] SKIP {d.Id} " +
                        $"alive={d.isAlive}, activeToday={d.isActiveToday}, silencedToday={d.isSilencedToday}");
                }

                // 소음 안 내는 애는 오디오도 꺼두기
                if (d.anchor != null)
                    d.anchor.StopAudioForToday(noiseVerboseLog);

                continue;
            }

            // 거리 계수 계산
            int distLevel = GetDistanceLevel(d.placeId);
            float coef = distanceCoef.TryGetValue(distLevel, out var c) ? c : 0.1f;

            // CSV Level 값을 기본 소음 세기로 사용
            float noise = d.data.level * coef;
            d.cachedNoiseContribution = noise;
            totalNoise += noise;

            // 소음 내는 애는 오디오가 켜져 있는지 보장
            if (d.anchor != null)
            {
                d.anchor.EnsureAudioForToday(noiseVerboseLog);
            }
            else if (noiseVerboseLog)
            {
                Debug.LogWarning($"[NoiseManager] {d.Id} 에 연결된 DistractionAnchor 없음 (anchor == null)");
            }

            if (noiseVerboseLog)
            {
                Debug.Log(
                    $"[NoiseManager] {d.Id} place={d.placeId}, distLevel={distLevel}, coef={coef}, " +
                    $"level={d.data.level}, noise={noise}");
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

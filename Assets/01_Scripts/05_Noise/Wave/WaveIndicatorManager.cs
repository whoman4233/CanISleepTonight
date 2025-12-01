using System.Collections.Generic;
using UnityEngine;
using static WaveObject;

public class WaveIndicatorManager : MonoBehaviour
{
    [Header("Managers")]
    public NeighborManager neighborManager;
    public PlayerLocationTracker playerLocation;

    [Header("Update")]
    public float tickInterval = 0.2f;
    private float tickTimer;

    [Header("Debug Mode")]
    public WaveDebugMode debugMode = WaveDebugMode.Production;

    [Header("Wave Pool")]
    public WaveObject wavePrefab;
    public int poolSize = 20;

    [Header("Rendering")]
    [Tooltip("파동 전용 레이어 이름 (Project Settings > Tags and Layers에서 생성)")]
    public string waveLayerName = "Wave";

    [Header("Color Settings")]
    [Tooltip("이 거리 이내로 가까이 오면 빨간색으로 수렴")]
    public float maxColorDistance = 15f;

    [Header("Debug")]
    public bool enableDebugLog = false;
    public bool drawDebugGizmos = true;

    private readonly List<WaveObject> pool = new List<WaveObject>();
    private int poolIndex = 0;

    private readonly List<WaveObject> activeWaves = new List<WaveObject>();

    private Dictionary<string, HouseSlot> houseSlotMap = new Dictionary<string, HouseSlot>();

    private int _waveLayer = -1;

    // 디버그용: 마지막 틱에서 스폰된 위치들
    private readonly List<Vector3> _lastSpawnPositions = new List<Vector3>();

    private void Awake()
    {
        _waveLayer = LayerMask.NameToLayer(waveLayerName);
        if (_waveLayer == -1)
        {
            Debug.LogWarning($"[WaveIndicatorManager] Layer '{waveLayerName}' not found. " +
                             $"Create it in Project Settings > Tags and Layers.");
        }

        BuildPool();
        CacheHouseSlots();
    }

    private void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(wavePrefab, transform);
            if (_waveLayer >= 0)
            {
                obj.InitLayer(_waveLayer);
            }
            obj.Hide();
            pool.Add(obj);
        }

        if (enableDebugLog)
        {
            Debug.Log($"[WaveIndicatorManager] Pool built. size={poolSize}, waveLayer={_waveLayer}");
        }
    }

    private void CacheHouseSlots()
    {
        var slots = FindObjectsOfType<HouseSlot>();
        foreach (var s in slots)
        {
            if (!houseSlotMap.ContainsKey(s.houseSlotId))
                houseSlotMap[s.houseSlotId] = s;
        }
    }

    private WaveObject SpawnWave(Vector3 pos, float strength01, WaveDebugMode mode)
    {
        var wo = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Count;

        wo.Show(pos, strength01, mode);
        activeWaves.Add(wo);

        _lastSpawnPositions.Add(wo.transform.position);

        if (enableDebugLog)
        {
            var parent = wo.transform.parent;
            Vector3 localToParent = parent != null
                ? parent.InverseTransformPoint(wo.transform.position)
                : wo.transform.position;

            Debug.Log(
        $"[WaveIndicatorManager] SpawnWave() mode={mode}, strength={strength01:F2}, " +
        $"spawnPos={pos}"
    );
        }

        return wo;
    }

    private void ClearWaves()
    {
        for (int i = 0; i < activeWaves.Count; i++)
            activeWaves[i].Hide();

        activeWaves.Clear();
        _lastSpawnPositions.Clear();
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0;
            UpdateWaves();
        }
    }

    private void UpdateWaves()
    {
        if (neighborManager == null || playerLocation == null)
            return;

        ClearWaves();

        var activeList = neighborManager.ActiveDistractionsToday;
        if (activeList == null || activeList.Count == 0)
            return;

        Vector3 playerPos = playerLocation.transform.position;
        int playerFloor = playerLocation.currentFloor;
        bool insideHouse = playerLocation.IsInsideHouse;
        string insideHouseId = playerLocation.currentHouseSlotId;

        // 1) RAW SOURCES ONLY: 실제 소음 위치에 직접 파동
        if (debugMode == WaveDebugMode.RawSources)
        {
            foreach (var d in activeList)
            {
                if (d.worldTransform == null) continue;

                float dist = Vector3.Distance(playerPos, d.worldTransform.position);
                float strength = 1f - Mathf.Clamp01(dist / maxColorDistance); // 가까울수록 1에 가까움

                SpawnWave(d.worldTransform.position, strength, WaveDebugMode.RawSources);
            }
            return;
        }

        // 2) COMBINED: Raw + Production
        if (debugMode == WaveDebugMode.Combined)
        {
            foreach (var d in activeList)
            {
                if (d.worldTransform == null) continue;

                float dist = Vector3.Distance(playerPos, d.worldTransform.position);
                float strength = 1f - Mathf.Clamp01(dist / maxColorDistance);

                SpawnWave(d.worldTransform.position, strength, WaveDebugMode.RawSources);
            }
            // 아래에서 Production 로직도 추가 실행
        }

        // 3) PRODUCTION: 기획서 A6 규칙 (문/위/아래 집계)
        RunProductionWaveLogic(activeList, playerFloor, insideHouse, insideHouseId);
    }

    // ============================================================
    // PRODUCTION MODE LOGIC
    // ============================================================
    private void RunProductionWaveLogic(
        IReadOnlyList<DistractionRuntime> activeList,
        int playerFloor,
        bool insideHouse,
        string insideHouseId)
    {
        bool hasUpper = false;
        bool hasLower = false;

        // 같은 층
        HashSet<string> sameFloorHouseIds = new HashSet<string>();
        List<Transform> insideDistractions = new List<Transform>();

        // 분석
        foreach (var d in activeList)
        {
            var owner = d.owner;
            if (owner == null || owner.houseSlot == null)
                continue;

            var slot = owner.houseSlot;
            int floor = slot.floor;

            if (floor > playerFloor)
                hasUpper = true;
            else if (floor < playerFloor)
                hasLower = true;
            else
            {
                // 같은 층
                if (insideHouse && insideHouseId == slot.houseSlotId)
                {
                    // 내부 방해요소
                    if (d.worldTransform != null)
                        insideDistractions.Add(d.worldTransform);
                }
                else
                {
                    // 같은 층 but 복도/다른 집
                    sameFloorHouseIds.Add(slot.houseSlotId);
                }
            }
        }

        // ---------------------------
        // 위층 / 아래층
        // ---------------------------
        if (hasUpper)
        {
            Vector3 pos = playerLocation.transform.position + Vector3.up * 1.5f;
            // 힌트용이니까 강도는 일단 0.5 고정 (노랑 근처)
            SpawnWave(pos, 0.5f, WaveDebugMode.Production);
        }

        if (hasLower)
        {
            Vector3 pos = playerLocation.transform.position + Vector3.down * 1.5f;
            SpawnWave(pos, 0.5f, WaveDebugMode.Production);
        }

        // ---------------------------
        // 같은 층
        // ---------------------------
        if (insideHouse)
        {
            // 내부 방해요소: 각자 1개씩
            for (int i = 0; i < insideDistractions.Count; i++)
            {
                SpawnWave(insideDistractions[i].position, 0.5f, WaveDebugMode.Production);
            }

            // 문 안쪽 1개
            if (houseSlotMap.TryGetValue(insideHouseId, out var slot))
            {
                if (slot.doorPoint != null)
                {
                    Vector3 insideDoor = slot.doorPoint.position + (-slot.doorPoint.forward * 0.3f);
                    SpawnWave(insideDoor, 0.5f, WaveDebugMode.Production);
                }
            }
        }
        else
        {
            // 복도 → 방해요소가 있는 집 문마다 1개
            foreach (var id in sameFloorHouseIds)
            {
                if (houseSlotMap.TryGetValue(id, out var s))
                {
                    if (s.doorPoint != null)
                        SpawnWave(s.doorPoint.position, 0.5f, WaveDebugMode.Production);
                }
            }
        }
    }

    // ============================================================
    // Gizmo 디버그 표시
    // ============================================================
    private void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < _lastSpawnPositions.Count; i++)
        {
            Gizmos.DrawWireSphere(_lastSpawnPositions[i], 0.3f);
        }
    }
}

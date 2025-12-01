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

    private readonly List<WaveObject> pool = new List<WaveObject>();
    private int poolIndex = 0;

    private readonly List<WaveObject> activeWaves = new List<WaveObject>();

    private Dictionary<string, HouseSlot> houseSlotMap = new Dictionary<string, HouseSlot>();

    private void Awake()
    {
        BuildPool();
        CacheHouseSlots();
    }

    private void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(wavePrefab, transform);
            obj.Hide();
            pool.Add(obj);
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

    private WaveObject SpawnWave(Vector3 pos, WaveDebugMode mode)
    {
        var wo = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Count;

        wo.Show(pos, mode);
        activeWaves.Add(wo);
        return wo;
    }

    private void ClearWaves()
    {
        for (int i = 0; i < activeWaves.Count; i++)
            activeWaves[i].Hide();

        activeWaves.Clear();
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
        int playerFloor = playerLocation.currentFloor;
        bool insideHouse = playerLocation.IsInsideHouse;
        string insideHouseId = playerLocation.currentHouseSlotId;

        // ------------------------------
        // 1) RAW SOURCES ONLY
        // ------------------------------
        if (debugMode == WaveDebugMode.RawSources)
        {
            foreach (var d in activeList)
            {
                if (d.worldTransform != null)
                    SpawnWave(d.worldTransform.position, WaveDebugMode.RawSources);
            }
            return;
        }

        // ------------------------------
        // 2) COMBINED → raw 먼저 표시
        // ------------------------------
        if (debugMode == WaveDebugMode.Combined)
        {
            foreach (var d in activeList)
            {
                if (d.worldTransform != null)
                    SpawnWave(d.worldTransform.position, WaveDebugMode.RawSources);
            }
            // 그 뒤에 Production 로직도 표시한다 → 아래에서 추가 실행
        }

        // ------------------------------
        // 3) PRODUCTION MODE (기획 규칙)
        // ------------------------------
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
            SpawnWave(pos, WaveDebugMode.Production);
        }

        if (hasLower)
        {
            Vector3 pos = playerLocation.transform.position + Vector3.down * 1.5f;
            SpawnWave(pos, WaveDebugMode.Production);
        }

        // ---------------------------
        // 같은 층
        // ---------------------------
        if (insideHouse)
        {
            // 내부 방해요소 1개씩
            for (int i = 0; i < insideDistractions.Count; i++)
                SpawnWave(insideDistractions[i].position, WaveDebugMode.Production);

            // 문 안쪽 1개
            if (houseSlotMap.TryGetValue(insideHouseId, out var slot))
            {
                if (slot.doorPoint != null)
                {
                    Vector3 insideDoor = slot.doorPoint.position + (-slot.doorPoint.forward * 0.3f);
                    SpawnWave(insideDoor, WaveDebugMode.Production);
                }
            }
        }
        else
        {
            // 복도 → 집 문마다 1개
            foreach (var id in sameFloorHouseIds)
            {
                if (houseSlotMap.TryGetValue(id, out var s))
                {
                    if (s.doorPoint != null)
                        SpawnWave(s.doorPoint.position, WaveDebugMode.Production);
                }
            }
        }
    }
}

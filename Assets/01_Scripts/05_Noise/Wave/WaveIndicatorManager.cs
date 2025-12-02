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

    [Header("Verbose Debug Log")]
    public bool verboseLogging;

    private readonly List<WaveObject> pool = new List<WaveObject>();
    private int poolIndex = 0;

    private readonly List<WaveObject> activeWaves = new List<WaveObject>();

    private Dictionary<string, HouseSlot> houseSlotMap = new Dictionary<string, HouseSlot>();

    private int _waveLayer = -1;

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

    /// <summary>
    /// 공통 스폰 + 로그 출력
    /// </summary>
    private WaveObject SpawnWave(Vector3 pos, float strength01, WaveDebugMode mode, string reason)
    {
        var wo = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Count;

        if (verboseLogging)
        {
            Debug.Log(
                $"[WaveSpawn] mode={mode}, strength={strength01:F2}, pos={pos}, " +
                $"reason={reason}, parent={transform.name}");
        }

        wo.Show(pos, strength01, mode);
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
        if (activeList == null || activeList.Count == 0)
            return;

        Vector3 playerPos = playerLocation.transform.position;
        int playerFloor = playerLocation.currentFloor;
        bool insideHouse = playerLocation.IsInsideHouse;
        string insideHouseId = playerLocation.currentHouseSlotId;

        // 1) RAW SOURCES ONLY
        if (debugMode == WaveDebugMode.RawSources)
        {
            foreach (var d in activeList)
            {
                if (d.worldTransform == null) continue;

                float dist = Vector3.Distance(playerPos, d.worldTransform.position);
                float strength = 1f - Mathf.Clamp01(dist / maxColorDistance);

                string ownerId = d.owner != null ? d.owner.Id : "null-owner";
                int floor = (d.owner != null && d.owner.houseSlot != null)
                                    ? d.owner.houseSlot.floor : -1;
                string houseSlot = (d.owner != null && d.owner.houseSlot != null)
                                    ? d.owner.houseSlot.houseSlotId : "null-house";
                Transform anchorTr = d.anchor != null ? d.anchor.transform : d.worldTransform;
                string anchorPath = GetHierarchyPath(anchorTr);

                SpawnWave(
                    d.worldTransform.position,
                    strength,
                    WaveDebugMode.RawSources,
                    $"RAW DistractionId={d.Id}, Owner={ownerId}, Floor={floor}, HouseSlot={houseSlot}, AnchorPath={anchorPath}"
                );
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

                string ownerId = d.owner != null ? d.owner.Id : "null-owner";
                int floor = (d.owner != null && d.owner.houseSlot != null)
                                    ? d.owner.houseSlot.floor : -1;
                string houseSlot = (d.owner != null && d.owner.houseSlot != null)
                                    ? d.owner.houseSlot.houseSlotId : "null-house";
                Transform anchorTr = d.anchor != null ? d.anchor.transform : d.worldTransform;
                string anchorPath = GetHierarchyPath(anchorTr);

                SpawnWave(
                    d.worldTransform.position,
                    strength,
                    WaveDebugMode.RawSources,
                    $"COMBINED_RAW DistractionId={d.Id}, Owner={ownerId}, Floor={floor}, HouseSlot={houseSlot}, AnchorPath={anchorPath}"
                );
            }
            // 아래에서 Production 로직 계속
        }


        // 3) PRODUCTION: 기획서 A6 규칙 (문/위/아래 집계)
        RunProductionWaveLogic(activeList, playerFloor, insideHouse, insideHouseId, playerPos);
    }

    private string GetHierarchyPath(Transform t)
    {
        if (t == null) return "null";

        var names = new System.Collections.Generic.List<string>();
        while (t != null)
        {
            names.Add(t.name);
            t = t.parent;
        }
        names.Reverse();
        return string.Join("/", names);
    }


    // ============================================================
    // PRODUCTION MODE LOGIC
    // ============================================================
    private void RunProductionWaveLogic(
        IReadOnlyList<DistractionRuntime> activeList,
        int playerFloor,
        bool insideHouse,
        string insideHouseId,
        Vector3 playerPos)
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
        // 위층 / 아래층 (힌트용)
        // ---------------------------
        if (hasUpper)
        {
            Vector3 pos = playerPos + Vector3.up * 1.5f;
            SpawnWave(pos, 0.5f, WaveDebugMode.Production, "PROD_UPPER_HINT");
        }

        if (hasLower)
        {
            Vector3 pos = playerPos + Vector3.down * 1.5f;
            SpawnWave(pos, 0.5f, WaveDebugMode.Production, "PROD_LOWER_HINT");
        }

        // ---------------------------
        // 같은 층
        // ---------------------------
        if (insideHouse)
        {
            // 내부 방해요소: 각자 1개씩
            for (int i = 0; i < insideDistractions.Count; i++)
            {
                var tr = insideDistractions[i];
                SpawnWave(
                    tr.position,
                    0.5f,
                    WaveDebugMode.Production,
                    $"PROD_INSIDE DistractionAnchor={tr.name}, House={insideHouseId}"
                );
            }

            // 문 안쪽 1개
            if (houseSlotMap.TryGetValue(insideHouseId, out var slot))
            {
                if (slot.doorPoint != null)
                {
                    Vector3 insideDoor = slot.doorPoint.position + (-slot.doorPoint.forward * 0.3f);
                    SpawnWave(
                        insideDoor,
                        0.5f,
                        WaveDebugMode.Production,
                        $"PROD_DOOR_INSIDE House={insideHouseId}, Door={slot.doorPoint.name}"
                    );
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
                    {
                        SpawnWave(
                            s.doorPoint.position,
                            0.5f,
                            WaveDebugMode.Production,
                            $"PROD_DOOR_HALL House={id}, Door={s.doorPoint.name}"
                        );
                    }
                }
            }
        }
    }
}

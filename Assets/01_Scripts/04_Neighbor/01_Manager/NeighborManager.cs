using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class NeighborManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private MasterGameDataSO masterData;

    [Header("Scene References")]
    [SerializeField] private List<HouseSlot> houseSlots = new List<HouseSlot>();

    [Header("Day Config")]
    [SerializeField] private int minActiveNeighbors = 2;
    [SerializeField] private int maxActiveNeighbors = 4;


    public PlayerLocationTracker locationTracker;

    // 런타임 데이터
    private readonly List<NeighborRuntime> _neighbors = new();
    private readonly Dictionary<string, NeighborRuntime> _neighborsById = new();

    private readonly List<DistractionRuntime> _allDistractions = new();
    private readonly Dictionary<string, DistractionRuntime> _distractionsById = new();

    // 오늘 활성 이웃/방해요소 캐시
    private readonly List<NeighborRuntime> _activeNeighborsToday = new();
    private readonly List<DistractionRuntime> _activeDistractionsToday = new();

    public IReadOnlyList<NeighborRuntime> Neighbors => _neighbors;
    public IReadOnlyList<DistractionRuntime> ActiveDistractionsToday => _activeDistractionsToday;
    public IReadOnlyList<NeighborRuntime> ActiveNeighborsToday => _activeNeighborsToday;

    private bool _initialized = false;

    [Header("Debug")]
    [SerializeField] private bool noiseDebugLog = true;


    private void Awake()
    {
        // 필요하면 싱글톤 패턴 등 여기서 처리
        if (houseSlots == null || houseSlots.Count == 0)
        {
            // 씬에서 자동 수집 (명시적으로 넣고 싶으면 인스펙터에서 설정)
            houseSlots = FindObjectsOfType<HouseSlot>().ToList();
        }

        if (locationTracker == null)
            locationTracker = FindObjectOfType<PlayerLocationTracker>();
    }

    // GameManager에서 주기적으로 호출해줄 진입점들만 공개로 열어둔다.
    public void InitializeWeek()
    {
        if (_initialized) return;
        _initialized = true;

        ClearAllRuntime();
        BuildRuntimeFromData();
        AssignHouseSlotsAndInstantiateHouses();
        LinkDistractionAnchors();
    }


    public void SetupDay(int dayIndex)
    {
        _activeNeighborsToday.Clear();
        _activeDistractionsToday.Clear();

        foreach (var n in _neighbors)
        {
            n.isActiveToday = false;
            foreach (var d in n.distractions)
            {
                d.isActiveToday = false;
            }
        }

        // 집이 실제로 배정된 이웃만 오늘 후보
        var aliveNeighbors = _neighbors
            .Where(n => n.isAlive && n.houseSlot != null)
            .ToList();

        if (aliveNeighbors.Count == 0)
            return;

        int minCount = Mathf.Clamp(minActiveNeighbors, 1, aliveNeighbors.Count);
        int maxCount = Mathf.Clamp(maxActiveNeighbors, minCount, aliveNeighbors.Count);
        int targetCount = Random.Range(minCount, maxCount + 1);

        Shuffle(aliveNeighbors);
        var selected = aliveNeighbors.Take(targetCount).ToList();

        foreach (var neighbor in selected)
        {
            neighbor.isActiveToday = true;
            _activeNeighborsToday.Add(neighbor);

            foreach (var d in neighbor.distractions)
            {
                if (!d.isAlive) continue;

                d.isActiveToday = true;
                _activeDistractionsToday.Add(d);
            }
        }

        // ★ 오늘자 소음 후보 디버그 덤프
        if (noiseDebugLog)
        {
            DumpTodayNoiseState($"SetupDay({dayIndex})");
        }
    }


    public void EndDay()
    {
        // 하루 종료 시 오늘자 활성 플래그 정리 (원하면 여기서만 해도 되고, SetupDay에서 덮어써도 됨)
        _activeNeighborsToday.Clear();
        _activeDistractionsToday.Clear();

        foreach (var n in _neighbors)
        {
            n.isActiveToday = false;
            foreach (var d in n.distractions)
            {
                d.isActiveToday = false;
            }
        }
    }

    // 특정 Distraction을 Dead 처리 (상호작용 시스템에서 호출)
    public void SetDistractionDead(string distractionId)
    {
        if (!_distractionsById.TryGetValue(distractionId, out var runtime))
            return;

        runtime.wasHitToday = true;
        runtime.isSilencedToday = true;
        runtime.isActiveToday = false;

        // 캐시 리스트에서도 제거
        _activeDistractionsToday.Remove(runtime);

        // 이웃의 모든 Distraction이 Dead이면, 이웃도 Dead 처리할 여지
        var owner = runtime.owner;
        if (owner != null && owner.distractions.All(d => !d.isAlive))
        {
            SetNeighborDead(owner.Id);
        }
    }

    public void SetNeighborDead(string neighborId)
    {
        if (!_neighborsById.TryGetValue(neighborId, out var runtime))
            return;

        runtime.isAlive = false;
        runtime.isActiveToday = false;
        _activeNeighborsToday.Remove(runtime);

        // 해당 이웃의 방해 요소도 전부 Dead 처리
        foreach (var d in runtime.distractions)
        {
            d.isAlive = false;
            d.isActiveToday = false;
            _activeDistractionsToday.Remove(d);
        }
    }

    public NeighborRuntime GetNeighbor(string neighborId)
        => _neighborsById.TryGetValue(neighborId, out var r) ? r : null;

    public DistractionRuntime GetDistraction(string distractionId)
        => _distractionsById.TryGetValue(distractionId, out var r) ? r : null;


    public void KillDistraction(string distractionId)
    {
        var d = GetDistraction(distractionId);
        if (d == null) return;

        d.SetDead();
    }

    // ---------------------------------------
    // 내부 빌드/유틸
    // ---------------------------------------

    private void ClearAllRuntime()
    {
        _neighbors.Clear();
        _neighborsById.Clear();
        _allDistractions.Clear();
        _distractionsById.Clear();
        _activeNeighborsToday.Clear();
        _activeDistractionsToday.Clear();
    }

    private void BuildRuntimeFromData()
    {
        if (masterData == null || masterData.neighborTable == null || masterData.distractionTable == null)
        {
            Debug.LogError("[NeighborManager] MasterGameDataSO or tables not assigned.");
            return;
        }

        // NeighborRuntime 생성
        foreach (var row in masterData.neighborTable.neighbors)
        {
            var runtime = new NeighborRuntime(row);
            _neighbors.Add(runtime);
            _neighborsById[row.neighborId] = runtime;
        }

        // DistractionRuntime 생성 + 이웃에 연결
        foreach (var row in masterData.distractionTable.distractions)
        {
            var dr = new DistractionRuntime(row);

            _allDistractions.Add(dr);
            _distractionsById[row.distractionId] = dr;

            if (_neighborsById.TryGetValue(row.ownerId, out var owner))
            {
                dr.owner = owner;
                owner.distractions.Add(dr);
            }
            else
            {
                Debug.LogWarning($"[NeighborManager] Distraction '{row.distractionId}' has unknown ownerId '{row.ownerId}'.");
            }
        }
    }

    private void AssignHouseSlotsAndInstantiateHouses()
    {
        if (masterData.houseLayoutTable == null)
        {
            Debug.LogWarning("[NeighborManager] HouseLayoutTable is null. Houses will not be instantiated.");
            return;
        }

        if (houseSlots == null || houseSlots.Count == 0)
        {
            Debug.LogWarning("[NeighborManager] No HouseSlots assigned.");
            return;
        }

        // 0) EMPTY 레이아웃 미리 확보
        var emptyRow = masterData.houseLayoutTable.GetById("EMPTY");
        if (emptyRow == null || emptyRow.housePrefab == null)
        {
            Debug.LogWarning("[NeighborManager] EMPTY layout not found. Empty slots remain blank.");
        }

        // 1) 플레이어 방(303호) 슬롯 분리
        HouseSlot playerSlot = null;
        var candidateSlots = new List<HouseSlot>();

        foreach (var slot in houseSlots)
        {
            if (!string.IsNullOrEmpty(locationTracker.currentHouseSlotId) &&
                slot.placeId == locationTracker.currentHouseSlotId)       // ex) P_303
            {
                playerSlot = slot;
            }
            else
            {
                candidateSlots.Add(slot);
            }
        }

        // 303호 슬롯을 못 찾았더라도 나머지 로직은 돌아가도록
        if (playerSlot == null)
        {
            Debug.LogWarning($"[NeighborManager] Player house slot (placeId={locationTracker.currentHouseSlotId}) not found. " +
                             "모든 슬롯이 일반 이웃 배치 대상이 됩니다.");
        }

        // 2) 플레이어 방을 제외한 슬롯만 셔플
        Shuffle(candidateSlots);

        int neighborCount = _neighbors.Count;
        int slotCount = candidateSlots.Count;
        int usedCount = Mathf.Min(neighborCount, slotCount);

        // 3) 이웃 → 랜덤 슬롯 배정
        for (int i = 0; i < usedCount; i++)
        {
            var neighbor = _neighbors[i];
            var slot = candidateSlots[i];

            neighbor.houseSlot = slot;

            if (!string.IsNullOrEmpty(slot.placeId))
            {
                neighbor.placeId = slot.placeId;
            }

            var layoutRow = masterData.houseLayoutTable.GetById(neighbor.data.layoutId);
            if (layoutRow == null || layoutRow.housePrefab == null)
            {
                Debug.LogWarning($"[NeighborManager] No housePrefab for layoutId '{neighbor.data.layoutId}' (NeighborId={neighbor.Id})");
                continue;
            }

            var instance = Instantiate(layoutRow.housePrefab, slot.InteriorRoot, false);
            InitTransform(instance.transform);

            neighbor.houseInstance = instance;

            Debug.Log($"[NeighborManager] Neighbor={neighbor.Id} → Slot={slot.houseSlotId} (placeId={slot.placeId})");
        }

        // 4) 남은 일반 슬롯들 = EMPTY 채우기
        for (int i = usedCount; i < slotCount; i++)
        {
            var slot = candidateSlots[i];

            if (emptyRow == null || emptyRow.housePrefab == null)
                continue;

            var instance = Instantiate(emptyRow.housePrefab, slot.InteriorRoot, false);
            InitTransform(instance.transform);

            Debug.Log($"[NeighborManager] EMPTY house spawned at Slot={slot.houseSlotId} (placeId={slot.placeId})");
        }

        // 5) 플레이어 슬롯(303호)에도 항상 EMPTY 배치
        if (playerSlot != null && emptyRow != null && emptyRow.housePrefab != null)
        {
            var instance = Instantiate(emptyRow.housePrefab, playerSlot.InteriorRoot, false);
            InitTransform(instance.transform);

            Debug.Log($"[NeighborManager] Player house EMPTY spawned at Slot={playerSlot.houseSlotId} (placeId={playerSlot.placeId})");
        }
    }


    private void InitTransform(Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.Euler(0f, 0f, 0f);
        t.localScale = Vector3.one;
    }




    private void LinkDistractionAnchors()
    {
        foreach (var neighbor in _neighbors)
        {
            if (neighbor.houseInstance == null)
                continue;

            var anchors = neighbor.houseInstance.GetComponentsInChildren<DistractionAnchor>(true);
            foreach (var anchor in anchors)
            {
                if (string.IsNullOrEmpty(anchor.DistractionId))
                    continue;

                if (!_distractionsById.TryGetValue(anchor.DistractionId, out var dr))
                {
                    Debug.LogWarning($"[NeighborManager] DistractionAnchor id '{anchor.DistractionId}' not found in runtime map.");
                    continue;
                }

                // 여기서 Anchor 연결
                dr.anchor = anchor;
                dr.worldTransform = anchor.transform;

                // 앵커 placeId가 있으면 우선
                if (!string.IsNullOrEmpty(anchor.PlaceId))
                    dr.placeId = anchor.PlaceId;
            }
        }

        // 최종 placeId 확정
        foreach (var d in _allDistractions)
        {
            d.FinalizePlaceId();
        }
    }




    private void Shuffle<T>(IList<T> list)
    {
        // 간단한 Fisher?Yates 셔플
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void DumpTodayNoiseState(string tag)
    {
        Debug.Log($"[NoiseDebug] ===== {tag} : ActiveNeighborsToday / ActiveDistractionsToday =====");

        // 1) 오늘 활성 이웃
        foreach (var n in _activeNeighborsToday)
        {
            string slotId = n.houseSlot != null ? n.houseSlot.houseSlotId : "null-slot";
            string placeId = string.IsNullOrEmpty(n.placeId) ? "null-place" : n.placeId;

            Debug.Log($"[NoiseDebug] Neighbor={n.Id}  slot={slotId}  place={placeId}  alive={n.isAlive}  today={n.isActiveToday}");
        }

        // 2) 오늘 활성 소음 후보
        foreach (var d in _activeDistractionsToday)
        {
            string ownerId = d.owner != null ? d.owner.Id : "null-owner";
            string placeId = string.IsNullOrEmpty(d.placeId) ? "null-place" : d.placeId;
            Vector3 pos = d.worldTransform != null ? d.worldTransform.position : Vector3.zero;

            Debug.Log($"[NoiseDebug]   Distraction={d.Id}  owner={ownerId}  alive={d.isAlive}  today={d.isActiveToday}  place={placeId}  pos={pos}");
        }
    }

}

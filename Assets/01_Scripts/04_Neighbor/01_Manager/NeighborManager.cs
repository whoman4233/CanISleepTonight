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

    private void Awake()
    {
        // 필요하면 싱글톤 패턴 등 여기서 처리
        if (houseSlots == null || houseSlots.Count == 0)
        {
            // 씬에서 자동 수집 (명시적으로 넣고 싶으면 인스펙터에서 설정)
            houseSlots = FindObjectsOfType<HouseSlot>().ToList();
        }
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
        // 오늘자 활성 상태 리셋
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

        // 살아 있는 이웃 중에서 후보 추출
        var aliveNeighbors = _neighbors.Where(n => n.isAlive).ToList();
        if (aliveNeighbors.Count == 0)
            return;

        // 오늘 활성 이웃 수 결정
        int minCount = Mathf.Clamp(minActiveNeighbors, 1, aliveNeighbors.Count);
        int maxCount = Mathf.Clamp(maxActiveNeighbors, minCount, aliveNeighbors.Count);
        int targetCount = Random.Range(minCount, maxCount + 1);

        // 랜덤 셔플 후 상위 targetCount만 사용
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

        runtime.isAlive = false;
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

        // 슬롯과 이웃 수 중 작은 쪽 기준으로 배정
        var shuffledSlots = new List<HouseSlot>(houseSlots);
        Shuffle(shuffledSlots);

        int count = Mathf.Min(_neighbors.Count, shuffledSlots.Count);

        for (int i = 0; i < count; i++)
        {
            var neighbor = _neighbors[i];
            var slot = shuffledSlots[i];

            neighbor.houseSlot = slot;

            // layoutId로 프리팹 찾기
            var layoutRow = masterData.houseLayoutTable.GetById(neighbor.data.layoutId);
            if (layoutRow == null || layoutRow.housePrefab == null)
            {
                Debug.LogWarning($"[NeighborManager] No housePrefab for layoutId '{neighbor.data.layoutId}' (NeighborId={neighbor.Id})");
                continue;
            }

            // 집 프리팹 인스턴스
            var instance = Instantiate(layoutRow.housePrefab, slot.InteriorRoot, false);
            neighbor.houseInstance = instance;
        }
    }

    private void LinkDistractionAnchors()
    {
        // 모든 이웃 집 내부에서 DistractionAnchor를 찾아, runtime에 연결
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

                // 🔹 Anchor ↔ Runtime 바인딩 (여기 한 줄로 정리)
                anchor.BindRuntime(dr);
            }
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
}

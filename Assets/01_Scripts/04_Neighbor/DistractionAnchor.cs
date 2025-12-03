using UnityEngine;

/// <summary>
/// DistractionRuntime 와 실제 씬 오브젝트(콜라이더/모델)를 연결하는 앵커
/// - IHittable을 구현해서, 타격 시 "오늘 소음만" 비활성화
/// - 다음 날에는 NeighborManager.SetupDay(...) 쪽에서 Runtime 플래그를 다시 세팅하고,
///   (프리팹 재생성하지 않는 경우) ResetForNewDay() 를 호출해주면 된다.
/// </summary>
public class DistractionAnchor : MonoBehaviour, IHittable
{
    [Header("Runtime ID 연결")]
    [SerializeField] private string distractionId;   // D_N003_A 등
    [SerializeField] private string placeId;         // 필요 없으면 비워도 됨

    [Header("히트 판정용 콜라이더들")]
    [SerializeField] private Collider[] hitColliders;

    [Header("디버그")]
    [SerializeField] private bool debugLog = true;

    // NeighborManager.LinkDistractionAnchors() 에서 채워줄 런타임 데이터
    public DistractionRuntime Runtime { get; private set; }

    public string DistractionId => distractionId;
    public string PlaceId => placeId;

    // 오늘 이미 맞았는지 여부 (중복 피격 방지)
    private bool _hasBeenHitThisDay = false;

    private void Awake()
    {
        // 콜라이더 자동 수집 (인스펙터에 안 넣어놨다면)
        if (hitColliders == null || hitColliders.Length == 0)
        {
            hitColliders = GetComponentsInChildren<Collider>();
        }
    }

    /// <summary>
    /// NeighborManager.LinkDistractionAnchors() 에서 호출해서
    /// CSV 기반 DistractionRuntime 와 이 앵커를 연결
    /// </summary>
    public void BindRuntime(DistractionRuntime runtime)
    {
        Runtime = runtime;

        if (Runtime == null)
            return;

        // 실제 위치 연결
        Runtime.worldTransform = transform;

        // 앵커에 placeId가 있으면 테이블 기본값보다 우선
        if (!string.IsNullOrWhiteSpace(placeId))
        {
            Runtime.placeId = placeId.Trim();
        }

        // 하루 시작 시 초기화 가정
        _hasBeenHitThisDay = false;
        SetHitColliders(true);

        if (debugLog)
        {
            Debug.Log($"[DistractionAnchor] BindRuntime: id={distractionId}, place={Runtime.placeId}, " +
                      $"owner={(Runtime.owner != null ? Runtime.owner.Id : "null")}");
        }
    }

    /// <summary>
    /// 외부에서 콜라이더 켜고/끄는 용도
    /// (하루 리셋 / 히트 후 재사용 방지 등)
    /// </summary>
    public void SetHitColliders(bool enabled)
    {
        if (hitColliders == null) return;

        foreach (var col in hitColliders)
        {
            if (col != null)
                col.enabled = enabled;
        }
    }

    /// <summary>
    /// 무기에 맞았을 때 호출 (IHittable)
    /// </summary>
    public void OnHit()
    {
        // 동일 날짜에 중복 피격 방지
        if (_hasBeenHitThisDay)
        {
            if (debugLog)
                Debug.Log($"[DistractionAnchor] OnHit 무시: 이미 오늘 맞은 소음원 (id={distractionId})");
            return;
        }

        if (Runtime == null)
        {
            Debug.LogWarning($"[DistractionAnchor] Runtime이 연결되지 않은 상태에서 OnHit 호출됨. (id={distractionId})");
            return;
        }

        _hasBeenHitThisDay = true;

        // 오늘 소음 비활성화:
        // - NoiseManager는 ActiveDistractionsToday 를 돌면서
        //   isActiveToday == true 인 것만 더하고 있으므로,
        //   여기서 isActiveToday 를 false 로 꺼 준다.
        Runtime.isActiveToday = false;

        // 필요하면 "오늘 소음 꺼졌음" 같은 추가 플래그를 따로 두어도 된다.
        // ex) Runtime.isNoiseKilledToday = true;

        // 히트 후에는 더 이상 때려도 반응 안 하도록 콜라이더 비활성화
        SetHitColliders(false);

        if (debugLog)
        {
            var ownerId = Runtime.owner != null ? Runtime.owner.Id : "null-owner";
            Debug.Log($"[DistractionAnchor] OnHit → DistractionId={distractionId}, owner={ownerId} " +
                      $"오늘 소음 OFF (isActiveToday=false)");
        }
    }

    /// <summary>
    /// 하루가 리셋될 때 (프리팹 재생성이 아니라, 같은 오브젝트를 재사용한다면)
    /// GameManager 또는 NeighborManager에서 호출해도 되는 헬퍼
    /// </summary>
    public void ResetForNewDay()
    {
        _hasBeenHitThisDay = false;
        SetHitColliders(true);

        if (Runtime != null)
        {
            // 새 날에는 다시 오늘 활동 가능 상태로 돌려놓는다.
            Runtime.isActiveToday = true;
        }

        if (debugLog)
        {
            Debug.Log($"[DistractionAnchor] ResetForNewDay: id={distractionId} 오늘 다시 활성화");
        }
    }
}

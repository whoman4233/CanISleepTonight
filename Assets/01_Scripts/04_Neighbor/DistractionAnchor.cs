using UnityEngine;

/// <summary>
/// DistractionRuntime 와 실제 씬 오브젝트(콜라이더/모델)를 연결하는 앵커
/// - IHittable을 구현해서, 타격 시 오늘 소음을 비활성화
/// </summary>
public class DistractionAnchor : MonoBehaviour, IHittable
{
    [Header("Runtime ID 연결")]
    [SerializeField] private string distractionId;   // D_N003_A 등
    [SerializeField] private string placeId;         // 필요 없으면 비워도 됨

    [Header("히트 판정용 콜라이더들")]
    [SerializeField] private Collider[] hitColliders;

    [Header("매니저 참조 (인스펙터에서 할당 권장)")]
    [SerializeField] private NeighborManager neighborManager;

    // 런타임에서 NeighborManager.LinkDistractionAnchors()에서 채워줄 값
    public DistractionRuntime Runtime { get; private set; }

    public string DistractionId => distractionId;
    public string PlaceId => placeId;

    private bool _hasBeenHitThisDay = false;

    private void Awake()
    {
        // NeighborManager 자동 찾기 (인스펙터에 안 넣었을 때 대비)
        if (neighborManager == null)
        {
            neighborManager = FindObjectOfType<NeighborManager>();
        }

        // 콜라이더 자동 수집 (안 넣어놨다면)
        if (hitColliders == null || hitColliders.Length == 0)
        {
            hitColliders = GetComponentsInChildren<Collider>();
        }
    }

    /// <summary>
    /// NeighborManager.LinkDistractionAnchors()에서 호출해서
    /// CSV 기반 DistractionRuntime와 이 앵커를 연결
    /// </summary>
    public void BindRuntime(DistractionRuntime runtime)
    {
        Runtime = runtime;

        if (Runtime == null)
            return;

        // 위치 연결
        Runtime.worldTransform = transform;

        // 앵커에 placeId가 있으면 테이블 기본값보다 우선
        if (!string.IsNullOrWhiteSpace(placeId))
        {
            Runtime.placeId = placeId.Trim();
        }

        // 하루 시작 시 초기화 가정
        _hasBeenHitThisDay = false;
        SetHitColliders(true);
    }

    /// <summary>
    /// 외부에서 콜라이더 켜고/끄는 용도
    /// (하루 리셋/히트 후 재사용 방지 등)
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
            return;

        if (Runtime == null)
        {
            Debug.LogWarning($"[DistractionAnchor] Runtime이 연결되지 않은 상태에서 OnHit 호출됨. (id={distractionId})");
            return;
        }

        if (neighborManager == null)
        {
            Debug.LogWarning("[DistractionAnchor] NeighborManager 참조 없음.");
            return;
        }

        _hasBeenHitThisDay = true;

        // 오늘 소음 비활성화 플래그 세팅 (매니저에 위임)
        neighborManager.SetDistractionDead(distractionId);

        // 히트 후에는 더 이상 때려도 반응 안 하도록 콜라이더 비활성화
        SetHitColliders(false);

        // 디버그 로그
        Debug.Log($"[DistractionAnchor] OnHit → DistractionId={distractionId} 오늘 소음 OFF 처리");
    }

    /// <summary>
    /// 하루가 리셋될 때 (프리팹 재생성이 아니라, 같은 오브젝트를 재사용한다면)
    /// GameManager 또는 NeighborManager에서 호출해도 되는 헬퍼
    /// </summary>
    public void ResetForNewDay()
    {
        _hasBeenHitThisDay = false;
        SetHitColliders(true);
    }
}

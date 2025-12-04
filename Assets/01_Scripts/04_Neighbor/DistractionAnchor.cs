using UnityEngine;

/// <summary>
/// DistractionRuntime 와 실제 씬 오브젝트(콜라이더/모델)를 연결하는 앵커
/// - IHittable을 구현해서, 타격 시 오늘 소음을 비활성화
/// - NoiseSfxEntry + AudioSource를 이용해서 소리를 재생/정지
/// </summary>
public class DistractionAnchor : MonoBehaviour, IHittable
{
    [Header("Runtime ID 연결")]
    [SerializeField] private string distractionId;   // D_N003_A 등
    [SerializeField] private string placeId;         // 필요 없으면 비워도 됨

    [Header("히트 판정용 콜라이더들")]
    [SerializeField] private Collider[] hitColliders;

    [Header("사운드")]
    [SerializeField] private AudioSource audioSource;     // 집 프리팹 하위에 붙인 AudioSource
    [SerializeField] private NoiseSfxEntry noiseSfx;      // SO에서 넘어온 데이터

    [Header("매니저 참조 (인스펙터에서 할당 권장)")]
    [SerializeField] private NeighborManager neighborManager;

    // 런타임에서 NeighborManager.LinkDistractionAnchors()에서 채워줄 값
    public DistractionRuntime Runtime { get; private set; }

    public string DistractionId => distractionId;
    public string PlaceId => placeId;

    private bool _hasBeenHitThisDay = false;

    private void Awake()
    {
        if (neighborManager == null)
            neighborManager = FindObjectOfType<NeighborManager>();

        if (hitColliders == null || hitColliders.Length == 0)
            hitColliders = GetComponentsInChildren<Collider>();

        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();
    }

    /// <summary>
    /// NeighborManager.LinkDistractionAnchors()에서 호출해서
    /// CSV 기반 DistractionRuntime와 이 앵커를 연결 + SFX 세팅
    /// </summary>
    public void BindRuntime(DistractionRuntime runtime, NoiseSfxEntry sfxEntry)
    {
        Runtime = runtime;
        noiseSfx = sfxEntry;

        if (Runtime == null)
            return;

        // 위치 연결
        Runtime.worldTransform = transform;

        // 앵커에 placeId가 있으면 테이블 기본값보다 우선
        if (!string.IsNullOrWhiteSpace(placeId))
        {
            Runtime.placeId = placeId.Trim();
        }

        // 사운드 세팅
        if (audioSource != null && noiseSfx != null)
        {
            audioSource.clip = noiseSfx.clip;
            audioSource.volume = noiseSfx.baseVolume;
            audioSource.loop = true;   // 소음은 보통 루프
        }

        // 하루 시작 시 초기화 가정
        _hasBeenHitThisDay = false;
        SetHitColliders(true);
    }

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

        // 오늘 소음 비활성화 플래그 세팅
        neighborManager.SetDistractionDead(distractionId);

        // 사운드 정지
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // 히트 후에는 더 이상 때려도 반응 안 하도록 콜라이더 비활성화
        SetHitColliders(false);

        Debug.Log($"[DistractionAnchor] OnHit → DistractionId={distractionId} 오늘 소음 OFF + SFX Stop");
    }

    public void ResetForNewDay()
    {
        _hasBeenHitThisDay = false;
        SetHitColliders(true);

        // 다음 날 다시 소음을 켜고 싶으면 여기서 Play
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }
}

using UnityEngine;

public class RagdollSetting : MonoBehaviour
{
    [Header("기본 컴포넌트")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody mainRigidbody;   // 루트 몸통
    [SerializeField] private Collider mainCollider;     // 루트 콜라이더

    [Header("레그돌 힘 세팅")]
    [SerializeField] private float hitForce = 10f;              // ActivateRagdoll()에서 사용
    [SerializeField] private float impactForceMultiplier = 0.2f; // 속도 → 힘 배율
    [SerializeField] private float minImpactForce = 5f;          // 최소 힘

    [Header("디버그용 고정 힘")]
    [SerializeField] private bool useDebugForce = true;   // 지금은 true 유지
    [SerializeField] private float debugForce = 500f;     // 디버그용 고정 힘

    [Header("위쪽으로 튕겨올리는 정도")]
    [SerializeField] private float upwardBiasAmount = 0.5f;

    [Header("Hit Sounds")]
    [SerializeField] private AudioClip[] hittingSounds;

    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private bool isRagdoll = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (mainRigidbody == null)
            mainRigidbody = GetComponent<Rigidbody>();

        if (mainCollider == null)
            mainCollider = GetComponent<Collider>();

        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        // 시작할 때 애니메이션 상태 유지 (레그돌 비활성화)
        SetRagdollActive(false);
    }

    /// <summary>
    /// 레그돌 on/off 토글
    /// </summary>
    private void SetRagdollActive(bool useRagdoll)
    {
        isRagdoll = useRagdoll;

        // 애니메이터 on/off
        if (animator != null)
            animator.enabled = !useRagdoll;

        // 루트 Rigidbody는 항상 kinematic 유지
        if (mainRigidbody != null)
            mainRigidbody.isKinematic = true;

        // 루트 콜라이더는 레그돌일 때 끈다
        if (mainCollider != null)
            mainCollider.enabled = !useRagdoll;

        // 자식 뼈대들만 실제 물리 사용
        foreach (var rb in ragdollRigidbodies)
        {
            if (rb == mainRigidbody) continue;
            rb.isKinematic = !useRagdoll;
        }

        foreach (var col in ragdollColliders)
        {
            if (col == mainCollider) continue;
            col.enabled = useRagdoll;
        }
    }

    /// <summary>
    /// 외부에서 호출 : 단순히 레그돌 활성화 + 기본 힘 적용 (옛 방식)
    /// 지금은 주로 ApplyImpact 를 사용.
    /// </summary>
    public void ActivateRagdoll(Vector3 hitPoint, Vector3 hitDirection)
    {
        if (isRagdoll)
            return;

        SetRagdollActive(true);

        Rigidbody closestBody = null;
        float closestDistance = float.MaxValue;

        foreach (var rb in ragdollRigidbodies)
        {
            if (rb == mainRigidbody) continue;

            float distance = Vector3.Distance(rb.worldCenterOfMass, hitPoint);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBody = rb;
            }
        }

        if (closestBody != null)
        {
            Vector3 force = hitDirection.normalized * hitForce;
            closestBody.AddForce(force, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// 피격 사운드 재생
    /// </summary>
    private void PlayRandomHittingSFX()
    {
        if (hittingSounds == null || hittingSounds.Length == 0)
            return;

        int rand = Random.Range(0, hittingSounds.Length);
        AudioClip hitSFX = hittingSounds[rand];

        AudioManager.Instance.PlaySFX(hitSFX);
    }

    /// <summary>
    /// 무기 속도/방향 기반으로 실제 물리 힘을 적용
    /// </summary>
    public void ApplyImpact(Vector3 hitPoint, Vector3 direction, float strength)
    {
        PlayRandomHittingSFX();

        if (!isRagdoll)
            SetRagdollActive(true);

        // 맞은 위치와 가장 가까운 뼈를 찾는다.
        Rigidbody closestBody = null;
        float closestDist = float.MaxValue;

        foreach (var rb in ragdollRigidbodies)
        {
            if (rb == mainRigidbody) continue;

            float dist = Vector3.Distance(rb.worldCenterOfMass, hitPoint);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestBody = rb;
            }
        }

        if (closestBody == null)
            return;

        // 힘 크기 계산
        float forceMagnitude;
        if (useDebugForce)
        {
            // 디버그: 항상 같은 큰 힘 사용
            forceMagnitude = debugForce;
        }
        else
        {
            // 실제 게임용: 무기 속도 기반
            forceMagnitude = Mathf.Max(
                strength * impactForceMultiplier,
                minImpactForce
            );
        }

        // 방향이 거의 0이면 기본적으로 캐릭터 앞 방향 반대쪽으로 튕기기
        Vector3 forceDirection = direction.sqrMagnitude > 0.0001f
            ? direction.normalized
            : -transform.forward;

        // 약간 위로 튕겨 올라가게 bias 추가
        Vector3 upwardBias = Vector3.up * upwardBiasAmount;
        forceDirection = (forceDirection + upwardBias).normalized;

        // 최종 힘 적용
        closestBody.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
    }
}
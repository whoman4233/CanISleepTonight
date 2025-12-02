using UnityEngine;

public class RagdollSetting : MonoBehaviour
{
    [Header("기본 컴포넌트")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody mainRigidbody;   // 루트 몸통
    [SerializeField] private Collider mainCollider;     // 루트 콜라이더

    [Header("레그돌 설정")]
    [SerializeField] private float hitForce = 5f;       // 맞았을 때 밀리는 힘

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

        // 시작할 때 애니메이션 상태 유지
        SetRagdollActive(false);
    }

    // 레그돌 on/off 토글
    private void SetRagdollActive(bool useRagdoll)
    {
        isRagdoll = useRagdoll;

        // 애니메이터 on/off
        if (animator != null)
            animator.enabled = !useRagdoll;

        // 루트 Rigidbody/Collider는 레그돌일 때 끄고,
        // 애니메이션 상태일 때만 사용
        if (mainRigidbody != null)
            mainRigidbody.isKinematic = useRagdoll;

        if (mainCollider != null)
            mainCollider.enabled = !useRagdoll;

        // 자식 뼈들 Rigidbody/Collider는 반대로 처리
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
    /// 외부에서 호출 : 맞았을 때 레그돌 활성화 + 힘 주기
    /// </summary>
    public void ActivateRagdoll(Vector3 hitPoint, Vector3 hitDirection)
    {
        if (isRagdoll)
            return;

        SetRagdollActive(true);

        // 맞은 위치와 가장 가까운 뼈를 찾아서 힘 가하기 (선택 사항)
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
}
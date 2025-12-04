using UnityEngine;

public class EquipItem : MonoBehaviour
{
    [Header("공격 세팅")]
    [SerializeField] private float attackRate = 1.0f;
    [SerializeField] private float attackDistance = 2.0f;
    [SerializeField] private LayerMask hitLayerMask;

    [Header("히트 판정 세팅")]
    [SerializeField] private float screenCenterX = 0.5f;   // 화면 중앙 X (0~1 비율)
    [SerializeField] private float screenCenterY = 0.5f;   // 화면 중앙 Y (0~1 비율)
    [SerializeField] private float minWeaponVelocitySqr = 0.01f; // 최소 속도 제곱
    [SerializeField] private float fallbackImpactScale = 3f;     // 속도 너무 낮을 때 보정 세기

    private Camera mainCamera;
    private float nextAttackTime = 0f;

    private ItemData curEquipItem;
    public ItemData CurEquipItem => curEquipItem;

    [Header("무기 속도 트래커")]
    public WeaponVelocityTracker weaponVelocity;

    private void Start()
    {
        mainCamera = Camera.main;

        if (weaponVelocity == null)
        {
            weaponVelocity = GetComponent<WeaponVelocityTracker>();
        }
    }

    /// <summary>
    /// 공격 입력이 들어왔을 때 호출 (쿨타임 체크)
    /// 실제 타격은 애니메이션 이벤트에서 OnHit() 호출
    /// </summary>
    public void OnUse()
    {
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackRate;

        // 여기서 애니메이션 트리거를 쏘는 식으로 확장하면 됨.
    }

    /// <summary>
    /// Attack 애니메이션 이벤트에서 호출되는 실제 타격 처리
    /// </summary>
    public void OnHit()
    {
        if (mainCamera == null)
            return;

        if (weaponVelocity == null)
            return;

        // 화면 중앙에서 레이 발사
        Vector3 screenCenter = new Vector3(
            Screen.width * screenCenterX,
            Screen.height * screenCenterY,
            0f);

        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        // 레이캐스트로 맞은 대상 찾기
        if (Physics.Raycast(ray, out RaycastHit hit, attackDistance, hitLayerMask))
        {
            // 1) 레그돌 물리 적용
            RagdollSetting ragdoll = hit.collider.GetComponentInParent<RagdollSetting>();
            if (ragdoll != null)
            {
                Vector3 impactDir = weaponVelocity.Velocity;

                // 무기 속도가 너무 낮으면 레이 방향을 사용해서 기본 힘을 준다
                if (impactDir.sqrMagnitude < minWeaponVelocitySqr)
                {
                    impactDir = ray.direction * fallbackImpactScale;
                }

                float impactStrength = impactDir.magnitude;
                ragdoll.ApplyImpact(hit.point, impactDir.normalized, impactStrength);
            }

            // 2) IHittable 인터페이스 (체력 감소 등 다른 처리)
            IHittable hittable = hit.collider.GetComponentInParent<IHittable>();
            if (hittable != null)
            {
                hittable.OnHit();
            }
        }
    }
}
using UnityEngine;


public class EquipItem : MonoBehaviour
{
    [Header("공격 세팅")]
    [SerializeField] private float attackRate = 1.0f;
    [SerializeField] private float attackDistance = 2.0f;
    [SerializeField] private LayerMask hitLayerMask;

    private Camera mainCamera;
    private float nextAttackTime = 0f;

    private ItemData curEquipItem;
    public ItemData CurEquipItem => curEquipItem;


    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void OnUse()
    {
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackRate;

        Debug.Log("공격 시작");
    }

    // TODO : Attack 애니메이션에서 이벤트로 호출
    public void OnHit()
    {
        if (mainCamera == null)
            return;

        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit hit, attackDistance, hitLayerMask))
        {
            Debug.Log($"무기 Hit : {hit.collider.name}");

            // 이웃 레그돌 처리
            RagdollSetting ragdoll = hit.collider.GetComponentInParent<RagdollSetting>();
            if (ragdoll != null)
            {
                ragdoll.ActivateRagdoll(hit.point, ray.direction);
            }

            // 기존 소음 오브젝트(DistractionAnchor 등) 처리 ----
            // IHittable 활용
            IHittable hittable = hit.collider.GetComponentInParent<IHittable>();
            if (hittable != null)
            {
                hittable.OnHit();
            }
        }
    }
}

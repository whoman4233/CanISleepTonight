using UnityEngine;


public class EquipItem : MonoBehaviour
{
    [Header("공격 세팅")]
    [SerializeField] private float attackRate = 1.0f;
    [SerializeField] private float attackDistance = 2.0f;

    private Camera mainCamera;

    private ItemData curEquipItem;
    public ItemData CurEquipItem => curEquipItem;


    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void OnUse()
    {
        Debug.Log("공격 중,,,");
        // TODO : Attack 애니메이션 trigger 로 호출
    }

    // TODO : Attack 애니메이션에서 이벤트로 호출
    public void OnHit()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, attackDistance))
        {
            
        }
    }
}

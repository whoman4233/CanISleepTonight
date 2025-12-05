using UnityEngine;

// Player 오브젝트의 장착 관리 컴포넌트
public class Equipment : MonoBehaviour
{
    [SerializeField] private EquipItem curEquip;
    public EquipItem CurEquip { get { return curEquip; } }

    [SerializeField] private Transform equipParent;     // EquipCamera

    private void Start()
    {
        Instantiate(CurEquip.gameObject, equipParent);
    }

    public void EquipNew()
    {
        // TODO : 아이템 데이터 작성 후, 로직 추가
    }

    public void UnEquip()
    {
        if (curEquip != null)
        {
            Destroy(curEquip.gameObject);
            curEquip = null;
        }
    }

    public void OnAttack()
    {
        if (curEquip == null)
        {
            Debug.LogWarning("장착된 아이템이 없습니다!");
            return;
        }

        curEquip.OnUse();
    }
}

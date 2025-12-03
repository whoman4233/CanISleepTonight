using UnityEngine;

public class AttackSFXRelay : MonoBehaviour
{
    [Header("Weapon Root (손 뼈 또는 무기 부모)")]
    [SerializeField] private Transform weaponRoot;

    private WeaponSFX currentWeaponSfx;

    private void Awake()
    {
        CacheCurrentWeaponSfx();
    }

    // 무기가 바뀌었을 때 다시 호출
    public void CacheCurrentWeaponSfx()
    {
        currentWeaponSfx = null;

        if (weaponRoot == null)
        {
            return;
        }

        currentWeaponSfx = weaponRoot.GetComponentInChildren<WeaponSFX>();
    }

    /// <summary>
    /// Attack 애니메이션 이벤트에서 호출할 함수
    /// </summary>
    public void PlayWeaponSwingSFX()
    {
        if (currentWeaponSfx == null && weaponRoot != null)
        {
            // 혹시 아직 못찾았으면 한 번 더 시도
            currentWeaponSfx = weaponRoot.GetComponentInChildren<WeaponSFX>();
        }

        if (currentWeaponSfx != null)
        {
            currentWeaponSfx.PlayRandomWeaponSFX();
        }
        else
        {
            Debug.Log("장착된 WeaponSFX 를 찾지 못했습니다.");
        }
    }
}
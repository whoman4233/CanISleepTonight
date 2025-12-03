using UnityEngine;

public class WeaponSFX : MonoBehaviour
{
    [Header("Weapon Sound List")]
    [SerializeField] private AudioClip[] weaponClips;

    private AudioManager audioManager;

    private void Awake()
    {
        audioManager = AudioManager.Instance;
    }

    /// <summary>
    /// 공격 애니메이션 이벤트에서 호출
    /// </summary>
    public void PlayRandomWeaponSFX()
    {
        if (weaponClips == null || weaponClips.Length == 0)
        {
            return;
        }

        int index = Random.Range(0, weaponClips.Length);
        AudioClip clip = weaponClips[index];

        audioManager.PlaySFX(clip);
    }
}
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    // 볼륨 값 (0 ~ 1)
    private float masterVolume = 1f;
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    // 음소거 상태 확인용 변수
    private bool isMuted = false;

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ApplyVolumeSettings();
    }

    // 마스터 볼륨 설정
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        SaveVolumeSettings();
    }

    // BGM 볼륨 설정
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        SaveVolumeSettings();
    }

    // SFX 볼륨 설정
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        SaveVolumeSettings();
    }

    // 음소거 토글
    public void ToggleMute()
    {
        isMuted = !isMuted;
        ApplyVolumeSettings();
        SaveVolumeSettings();
    }

    // 음소거 설정
    public void SetMute(bool mute)
    {
        isMuted = mute;
        ApplyVolumeSettings();
        SaveVolumeSettings();
    }

    // 실제 AudioSource에 볼륨 적용
    private void ApplyVolumeSettings()
    {
        float actualMasterVolume = isMuted ? 0f : masterVolume;

        if (bgmSource != null)
            bgmSource.volume = actualMasterVolume * bgmVolume;

        if (sfxSource != null)
            sfxSource.volume = actualMasterVolume * sfxVolume;
    }

    // 볼륨 설정 저장 (PlayerPrefs 사용)
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 볼륨 설정 로드
    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
    }

    // Getter 메서드들
    public float GetMasterVolume() => masterVolume;
    public float GetBGMVolume() => bgmVolume;
    public float GetSFXVolume() => sfxVolume;
    public bool IsMuted() => isMuted;

    // BGM 재생
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (bgmSource != null && clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
    }

    // SFX 재생
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}


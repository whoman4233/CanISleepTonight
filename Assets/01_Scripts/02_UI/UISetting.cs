using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISetting : MonoBehaviour
{
    [Header("설정창 UI")]
    [SerializeField] private GameObject settingPanel;

    [Header("음소거 버튼")]
    [SerializeField] private Button muteButton;
    [SerializeField] private Image muteButtonImage;
    [SerializeField] private Sprite soundOnIcon;    // 소리 켜짐 아이콘
    [SerializeField] private Sprite soundOffIcon;   // 음소거 아이콘

    [Header("볼륨 슬라이더")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("슬라이더 색상 설정")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.6f, 0.8f);      // 일반 상태 색상
    [SerializeField] private Color mutedColor = new Color(0.5f, 0.5f, 0.5f);       // 음소거 상태 색상 (회색)
    [SerializeField] private Color handleNormalColor = new Color(0.8f, 0.8f, 0.8f); // 핸들 일반 색상
    [SerializeField] private Color handleMutedColor = new Color(0.4f, 0.4f, 0.4f);  // 핸들 음소거 색상

    // 슬라이더 Fill과 Handle 이미지 캐싱
    private Image masterFillImage;
    private Image masterHandleImage;
    private Image bgmFillImage;
    private Image bgmHandleImage;
    private Image sfxFillImage;
    private Image sfxHandleImage;

    public bool IsSettingOpen => settingPanel.activeInHierarchy;

    private void Start()
    {
        // 슬라이더 컴포넌트 캐싱
        CacheSliderImages();

        // 슬라이더 이벤트 리스너 등록
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // 음소거 버튼 이벤트 리스너 등록
        if (muteButton != null)
            muteButton.onClick.AddListener(OnMuteButtonClicked);

        // 저장된 볼륨 값으로 슬라이더 초기화
        LoadSliderValues();

        // 음소거 상태 UI 업데이트
        UpdateMuteUI();
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위해 리스너 제거
        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

        if (muteButton != null)
            muteButton.onClick.RemoveListener(OnMuteButtonClicked);
    }

    // 슬라이더의 Fill과 Handle 이미지 컴포넌트를 미리 찾아서 캐싱
    private void CacheSliderImages()
    {
        if (masterSlider != null)
        {
            masterFillImage = masterSlider.fillRect?.GetComponent<Image>();
            masterHandleImage = masterSlider.handleRect?.GetComponent<Image>();
        }

        if (bgmSlider != null)
        {
            bgmFillImage = bgmSlider.fillRect?.GetComponent<Image>();
            bgmHandleImage = bgmSlider.handleRect?.GetComponent<Image>();
        }

        if (sfxSlider != null)
        {
            sfxFillImage = sfxSlider.fillRect?.GetComponent<Image>();
            sfxHandleImage = sfxSlider.handleRect?.GetComponent<Image>();
        }
    }

    public void ToggleSetting()
    {
        if (IsSettingOpen) 
            CloseSetting();
        else 
            OpenSetting();
    }

    public void OpenSetting()
    {
        settingPanel.SetActive(true);
        Time.timeScale = 0f;    // 설정창 열면, 게임 일시 정지

        // 설정창 열 때마다 슬라이더 값 동기화
        LoadSliderValues();
        UpdateMuteUI();
    }

    public void CloseSetting()
    {
        settingPanel.SetActive(false);
        Time.timeScale = 1f;    // 설정창 닫으면, 게임 다시 진행
    }

    // 저장된 볼륨 값으로 슬라이더 초기화
    private void LoadSliderValues()
    {
        if (AudioManager.Instance != null)
        {
            if (masterSlider != null)
                masterSlider.value = AudioManager.Instance.GetMasterVolume();

            if (bgmSlider != null)
                bgmSlider.value = AudioManager.Instance.GetBGMVolume();

            if (sfxSlider != null)
                sfxSlider.value = AudioManager.Instance.GetSFXVolume();
        }
    }

    // 슬라이더 값 변경 이벤트 핸들러
    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    // 음소거 버튼 클릭 이벤트
    private void OnMuteButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
            UpdateMuteUI();
        }
    }

    // 음소거 상태에 따라 UI 업데이트
    private void UpdateMuteUI()
    {
        if (AudioManager.Instance == null) return;

        bool isMuted = AudioManager.Instance.IsMuted();

        // 음소거 버튼 아이콘 변경
        if (muteButtonImage != null)
        {
            if (isMuted && soundOffIcon != null)
                muteButtonImage.sprite = soundOffIcon;
            else if (!isMuted && soundOnIcon != null)
                muteButtonImage.sprite = soundOnIcon;
        }

        // 슬라이더 색상 변경
        Color fillColor = isMuted ? mutedColor : normalColor;
        Color handleColor = isMuted ? handleMutedColor : handleNormalColor;

        // Master 슬라이더
        if (masterFillImage != null)
            masterFillImage.color = fillColor;
        if (masterHandleImage != null)
            masterHandleImage.color = handleColor;

        // BGM 슬라이더
        if (bgmFillImage != null)
            bgmFillImage.color = fillColor;
        if (bgmHandleImage != null)
            bgmHandleImage.color = handleColor;

        // SFX 슬라이더
        if (sfxFillImage != null)
            sfxFillImage.color = fillColor;
        if (sfxHandleImage != null)
            sfxHandleImage.color = handleColor;

        // 슬라이더 인터랙션 활성화/비활성화 (선택사항)
        if (masterSlider != null)
            masterSlider.interactable = !isMuted;
        if (bgmSlider != null)
            bgmSlider.interactable = !isMuted;
        if (sfxSlider != null)
            sfxSlider.interactable = !isMuted;
    }

    // Quit 버튼 이벤트
    public void OnQuitButtonClicked()
    {
        CloseSetting();

        // TODO : IntroScene 과 연결
        // Application.Quit();
    }

    // Retry 버튼 이벤트
    public void OnRetryButtonClicked()
    {
        // 재시작 로직
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
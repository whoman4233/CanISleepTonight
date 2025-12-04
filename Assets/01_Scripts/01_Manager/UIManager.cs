using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD UI")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image stressBar;
    [SerializeField] private TextMeshProUGUI stressText;
    [SerializeField] private Image fatigueBar;
    [SerializeField] private TextMeshProUGUI fatigueText;

    public TextMeshProUGUI PromptText => promptText;

    [Header("UI References")]
    [SerializeField] private UIInventory uiInventory;
    [SerializeField] private UIItemDetail uiItemDetail;
    [SerializeField] private UISetting uiSetting;
    [SerializeField] private UITimer uiTimer;
    [SerializeField] private UINoisePanel uiNoisePanel;

    public UIInventory UIInventory => uiInventory;
    public UIItemDetail UIItemDetail => uiItemDetail;

    public bool IsInventoryOpen => uiInventory != null && uiInventory.IsInventoryOpen;
    public bool IsSettingOpen => uiSetting != null && uiSetting.IsSettingOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // UI Open/Close 요청 함수들
    public void ToggleInventory()
    {
        if (IsSettingOpen) return;   // 설정창 열려있으면 인벤토리 못 열게

        uiInventory.ToggleInventory();
    }

    public void ToggleSetting()
    {
        // 인벤토리 열려있으면 인벤토리 닫기
        if (IsInventoryOpen)
            uiInventory.CloseInventory();

        uiSetting.ToggleSetting();
    }

    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene("PlayScene_JAR");
    }

    public void OnEndButtonClicked()
    {
        UnityEditor.EditorApplication.isPlaying = false;
    }

    public void OnQuitButtonClicked()
    {
        SceneManager.LoadScene("IntroScene_JAR");
    }

    // Retry 버튼 이벤트
    public void OnRetryButtonClicked()
    {
        // 재시작 로직
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 타이머 패널 (DayText, TimeText) 갱신
    public void UpdateTimer(int currentDay, float remainingSeconds)
    {
        uiTimer.UpdateTimer(currentDay, remainingSeconds);
    }

    // 소음 UI 표시/숨김
    public void ShowNoiseUI(bool show)
    {
        if (uiNoisePanel != null)
        {
            uiNoisePanel.gameObject.SetActive(show);
        }
    }

    public void UpdateStressUI(float stress, float maxValue)
    {
        stressBar.fillAmount = stress / maxValue;
        stressText.text = $"스트레스 {stress} / {maxValue}";
    }

    public void UpdateFatigueUI(float fatigue, float maxValue)
    {
        fatigueBar.fillAmount = fatigue / maxValue;
        fatigueText.text = $"피로도 {fatigue} / {maxValue}";
    }

    // 소음 수치 업데이트
    public void UpdateNoiseLevel(float noiseValue)
    {
        if (uiNoisePanel != null)
        {
            uiNoisePanel.UpdateNoiseLevel(noiseValue);
        }
    }
}

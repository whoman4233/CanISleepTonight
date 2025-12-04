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
    [SerializeField] private UIEndingPanel uiEndingPanel;

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

    void ToggleCursor()
    {
        bool isLocked = Cursor.lockState == CursorLockMode.Locked;
        bool canLook = PlayerManager.Instance.Player.GetComponent<PlayerController>().canLook;

        if (UIManager.Instance.IsInventoryOpen || UIManager.Instance.IsSettingOpen)
        {
            // ▶ 인벤토리 OR 설정창 열림 상태 : 커서 보이게 + 카메라 회전 끔
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            canLook = false;
        }
        else
        {
            // ▶ 인벤토리 OR 설정창 닫힘 상태 : 커서 숨기고 + 다시 카메라 회전 켬
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canLook = true;
        }
    }

    // UI Open/Close 요청 함수들
    public void ToggleInventory()
    {
        if (IsSettingOpen) return;   // 설정창 열려있으면 인벤토리 못 열게

        uiInventory.ToggleInventory();
        ToggleCursor();
    }

    public void ToggleSetting()
    {
        // 인벤토리 열려있으면 인벤토리 닫기
        if (IsInventoryOpen)
            uiInventory.CloseInventory();

        uiSetting.ToggleSetting();
        ToggleCursor();
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

    // 엔딩 UI 처리
    public void ShowEndingPanel(string endingType)
    {
        uiEndingPanel.gameObject.SetActive(true);
        ToggleCursor();
    }
}

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
    [SerializeField] private Image fatigueBar;
    
    public Image StressBar => stressBar;
    public Image FatigueBar => fatigueBar;
    public TextMeshProUGUI PromptText => promptText;

    [Header("UI References")]
    [SerializeField] private UIInventory uiInventory;
    [SerializeField] private UIItemDetail uiItemDetail;
    [SerializeField] private UISetting uiSetting;

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
}

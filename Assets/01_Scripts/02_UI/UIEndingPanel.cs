using UnityEngine;
using UnityEngine.UI;

public class UIEndingPanel : MonoBehaviour
{
    [System.Serializable]
    public class EndingScreen
    {
        public string endingType;           // "Normal", "Bad" 등
        public Image screenObject;     // 해당 엔딩 스크린 오브젝트
    }

    [Header("엔딩 스크린 설정")]
    [SerializeField] private EndingScreen[] endingScreens;

    [Header("공통 UI")]
    [SerializeField] private Button retryBtn;

    private void Awake()
    {
        gameObject.SetActive(false);
        
        // 초기에는 모든 엔딩 스크린 비활성화
        HideAllEndingScreens();
    }

    // 특정 엔딩 스크린 표시
    public void ShowEndingScreen(string endingType)
    {
        // 먼저 모든 엔딩 스크린 비활성화
        HideAllEndingScreens();

        // 해당 엔딩 타입의 스크린만 활성화
        bool found = false;
        foreach (var ending in endingScreens)
        {
            if (ending.endingType == endingType && ending.screenObject != null)
            {
                ending.screenObject.gameObject.SetActive(true);
                found = true;
                Debug.Log($"[UIEndingPanel] {endingType} 엔딩 표시");
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"[UIEndingPanel] '{endingType}' 엔딩을 찾을 수 없습니다!");
        }

        // Retry 버튼은 항상 활성화
        if (retryBtn != null)
        {
            retryBtn.gameObject.SetActive(true);
        }
    }

    // 모든 엔딩 스크린 숨기기
    private void HideAllEndingScreens()
    {
        foreach (var ending in endingScreens)
        {
            if (ending.screenObject != null)
            {
                ending.screenObject.gameObject.SetActive(false);
            }
        }
    }

    // Retry 버튼 클릭 시 (Inspector에서 연결)
    public void OnRetryButtonClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnRetryButtonClicked();
        }
    }

    // TODO : 추후, 인트로씬 작업 + Quit 버튼 추가 후, 사용 
    public void OnQuitButtonClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnQuitButtonClicked();
        }
    }
}
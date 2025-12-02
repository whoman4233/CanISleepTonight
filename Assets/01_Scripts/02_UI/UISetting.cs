using UnityEngine;

public class UISetting : MonoBehaviour
{
    [Header("설정창 UI")]
    [SerializeField] private GameObject settingPanel;

    public bool IsSettingOpen => settingPanel.activeInHierarchy;

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
    }

    public void CloseSetting()
    {
        settingPanel.SetActive(false);
        Time.timeScale = 1f;    // 설정창 닫으면, 게임 다시 진행
    }
}
using TMPro;
using UnityEngine;

public class UITimer : MonoBehaviour
{
    [Header("Timer UI Elements")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI timerText;

    void Start()
    {
        // 초기 텍스트 설정
        if (dayText != null)
            dayText.text = "Day 1";

        if (timerText != null)
            timerText.text = "00 : 00";
    }

    // Day 텍스트 업데이트
    public void SetDayText(string text)
    {
        if (dayText != null)
            dayText.text = text;
    }


    // 타이머 텍스트 업데이트
    public void SetTimerText(string text)
    {
        if (timerText != null)
            timerText.text = text;
    }

 
    // 타이머 텍스트 업데이트 (초 단위로 받아서 자동 포맷)
    public void SetTimerFromSeconds(float remainingSeconds)
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
        int seconds = Mathf.FloorToInt(remainingSeconds % 60f);

        timerText.text = $"{minutes:D2} : {seconds:D2}";
    }


    // Day와 Timer를 동시에 업데이트
    public void UpdateTimer(int currentDay, float remainingSeconds)
    {
        SetDayText($"Day {currentDay + 1}");
        SetTimerFromSeconds(remainingSeconds);
    }
}

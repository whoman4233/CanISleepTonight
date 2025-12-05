using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINoisePanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image noiseBar;    // 소음 바
    [SerializeField] private TextMeshProUGUI noiseText;     // 소음 텍스트

    [Header("Color Settings")]
    [SerializeField] private Color greenColor;   // 00~29 (쾌적) : 초록색
    [SerializeField] private Color yellowColor;  // 30~59 (거슬림) : 노란색
    [SerializeField] private Color redColor;     // 60~100 (시끄러움) : 빨간색

    private void Start()
    {
        // 처음에 소음도 UI 숨김
        gameObject.SetActive(false);
    }

    // 소음 수치 업데이트
    public void UpdateNoiseLevel(float noiseValue)
    {
        // 소음 바 업데이트
        if (noiseBar != null)
        {
            noiseBar.fillAmount = Mathf.Clamp01(noiseValue / 100f);
            noiseText.text = $"소음 {(int)noiseValue} / 100";

            // 소음 수치에 따른 색상 변경
            noiseBar.color = GetNoiseColor(noiseValue);
        }
    }

    // 소음 수치에 따른 색상 반환
    private Color GetNoiseColor(float noiseValue)
    {
        if (noiseValue < 30f)
            return greenColor;
        else if (noiseValue < 60f)
            return yellowColor;
        else
            return redColor;
    }
}

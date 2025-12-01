using UnityEngine;

public class WaveObject : MonoBehaviour
{
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    // 색상 및 스타일 정의
    [Header("Colors")]
    public Color productionColor = Color.white;
    public Color rawSourceColor = Color.red;

    public void Show(Vector3 pos, WaveDebugMode mode)
    {
        transform.position = pos;
        gameObject.SetActive(true);

        if (spriteRenderer != null)
        {
            switch (mode)
            {
                case WaveDebugMode.Production:
                    spriteRenderer.color = productionColor;
                    break;

                case WaveDebugMode.RawSources:
                    spriteRenderer.color = rawSourceColor;
                    break;

                case WaveDebugMode.Combined:
                    // Combined에서는 Raw와 Production이 둘 다 표시되므로
                    // Combined 자체는 Production 색상으로 통일
                    spriteRenderer.color = productionColor;
                    break;
            }
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public enum WaveDebugMode
    {
        Production,     // 기획 규칙 기반 (A6)
        RawSources,     // 오늘 활성된 DistractionRuntime 위치를 전부 표시
        Combined        // Raw + Production 파동을 모두 표시
    }

}

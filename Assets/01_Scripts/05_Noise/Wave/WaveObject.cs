using UnityEngine;

public class WaveObject : MonoBehaviour
{
    [Header("Refs")]
    public ParticleSystem particle;

    [Header("Color (초-노-빨)")]
    public Color lowColor = Color.green;   // 멀리 / 약한 소음
    public Color midColor = Color.yellow;  // 중간
    public Color highColor = Color.red;    // 가까이 / 강한 소음

    private int _waveLayer = -1;

    public void InitLayer(int layer)
    {
        _waveLayer = layer;
        SetLayerRecursively(gameObject, _waveLayer);
    }

    public void Show(Vector3 worldPos, float strength01, WaveDebugMode mode)
    {
        // strength 0~1 클램프
        strength01 = Mathf.Clamp01(strength01);

        // 색 보간: 0~0.5 구간은 초 → 노, 0.5~1 구간은 노 → 빨
        Color c;
        if (strength01 <= 0.5f)
        {
            float t = strength01 / 0.5f; // 0~1
            c = Color.Lerp(lowColor, midColor, t);
        }
        else
        {
            float t = (strength01 - 0.5f) / 0.5f; // 0~1
            c = Color.Lerp(midColor, highColor, t);
        }

        // 위치/색 적용
        transform.position = worldPos;

        if (particle != null)
        {
            var main = particle.main;
            main.startColor = c;

            if (!particle.isPlaying)
                particle.Play();
        }

        gameObject.SetActive(true);

        // 위치 디버그 로그
        Debug.Log(
            $"[WaveObject] Show() mode={mode}, strength={strength01:F2}, color={c}, " +
            $"targetPos={worldPos}, actualPos={transform.position}, parent={transform.parent?.name}"
        );
    }

    public void Hide()
    {
        if (particle != null)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        gameObject.SetActive(false);
    }

    // 에디터에서 위치 보이게
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (layer < 0) return;

        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}

public enum WaveDebugMode
{
    Production,     // 기획 규칙 기반 (A6)
    RawSources,     // 오늘 활성된 DistractionRuntime 위치를 전부 표시
    Combined        // Raw + Production 파동을 모두 표시
}

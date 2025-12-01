using UnityEngine;

public class WaveObject : MonoBehaviour
{
    public ParticleSystem particle;

    public void Show(Vector3 pos, WaveDebugMode mode)
    {
        transform.position = pos;
        gameObject.SetActive(true);
        particle.Play();
    }

    public void Hide()
    {
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        gameObject.SetActive(false);
    }
}


public enum WaveDebugMode
{
    Production,     // 기획 규칙 기반 (A6)
    RawSources,     // 오늘 활성된 DistractionRuntime 위치를 전부 표시
    Combined        // Raw + Production 파동을 모두 표시
}

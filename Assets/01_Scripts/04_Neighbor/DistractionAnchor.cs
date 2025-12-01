using UnityEngine;

public class DistractionAnchor : MonoBehaviour
{
    [Header("Data Link")]
    [Tooltip("기획서의 DistractionID (예: D_N001_A)")]
    public string distractionId;

    [Tooltip("PlaceID (예: P_303, P_202 등). 비워두면 데이터 기본값 사용")]
    public string placeId;

    [Header("Noise Source Settings")]
    [Tooltip("이 위치가 실제 소음 발생 위치 여부")]
    public bool isNoiseOrigin = true;

    // 런타임 DistractionRuntime 참조
    [HideInInspector]
    public DistractionRuntime runtime;

    public string DistractionId => distractionId;
    public string PlaceId => placeId;
    public Transform WorldTransform => transform;

    /// <summary>
    /// NeighborManager에서 런타임 객체 연결할 때 호출
    /// </summary>
    public void BindRuntime(DistractionRuntime rt)
    {
        runtime = rt;

        // 월드 위치 연결
        rt.worldTransform = transform;

        // Anchor에 PlaceId가 들어있으면 데이터 기본값보다 우선
        if (!string.IsNullOrEmpty(placeId))
        {
            rt.placeId = placeId;
        }

        // 소음원 여부 반영
        rt.isNoiseSource = isNoiseOrigin;
    }
}

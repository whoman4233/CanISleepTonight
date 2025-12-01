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

    public string DistractionId => distractionId;
    public string PlaceId => placeId;
    public Transform WorldTransform => transform;
}

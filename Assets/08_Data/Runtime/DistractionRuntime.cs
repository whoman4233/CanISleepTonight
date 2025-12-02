using UnityEngine;

public class DistractionRuntime
{
    // 원본 데이터
    public readonly DistractionDataRow data;

    // 식별자 편의 프로퍼티
    public string Id => data.distractionId;
    public string OwnerId => data.ownerId;

    // 소유 이웃
    public NeighborRuntime owner;

    // 상태 플래그
    public bool isAlive = true;
    public bool isActiveToday = false;

    // 월드 상 위치/참조
    public Transform worldTransform;
    public string placeId;

    // 소음/디버그용
    public bool isNoiseSource = true;
    public float cachedNoiseContribution;

    // ★ 추가: 어느 DistractionAnchor에서 온 놈인지
    public DistractionAnchor anchor;

    public void SetDead()
    {
        isAlive = false;
        isActiveToday = false;
        isNoiseSource = false;
    }

    public DistractionRuntime(DistractionDataRow dataRow)
    {
        data = dataRow;
        placeId = dataRow.placeId;
    }
}

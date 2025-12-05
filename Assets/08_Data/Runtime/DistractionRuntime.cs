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
    public bool isAlive = true;        // 퍼즐/상호작용으로 영구적으로 꺼졌는지

    // 월드 상 위치/참조
    public Transform worldTransform;   // DistractionAnchor에서 가져오는 Transform

    // Place 정보
    //  - dataPlaceId : CSV에 적힌 원본
    //  - placeId     : 런타임 최종 값(앵커/하우스 슬롯 등 반영)
    public string dataPlaceId;
    public string placeId;

    // 어디 앵커에 붙어있는지 추적용
    public DistractionAnchor anchor;

    // 옵션: 소음/디버그용 캐시
    public bool isNoiseSource = true;
    public float cachedNoiseContribution;

    public DistractionRuntime(DistractionDataRow dataRow)
    {
        data = dataRow;
        dataPlaceId = dataRow.placeId;
        placeId = dataRow.placeId;     // 기본값은 데이터 기준
    }

    public void SetDead()
    {
        isAlive = false;
        isActiveToday = false;
        isNoiseSource = false;
    }

    /// <summary>
    /// NeighborManager.LinkDistractionAnchors() 이후에
    /// 최종 placeId를 정리해 주기 위한 메서드
    /// </summary>
    public void FinalizePlaceId()
    {
        // 1순위: 앵커에 명시된 PlaceId
        if (anchor != null && !string.IsNullOrWhiteSpace(anchor.PlaceId))
        {
            placeId = anchor.PlaceId.Trim();
            return;
        }

        // 2순위: CSV 데이터
        if (!string.IsNullOrWhiteSpace(dataPlaceId))
        {
            placeId = dataPlaceId.Trim();
            return;
        }

        // 3순위: 오너 이웃이 가지고 있는 placeId (필요하면 사용)
        if (owner != null && !string.IsNullOrWhiteSpace(owner.placeId))
        {
            placeId = owner.placeId.Trim();
        }
    }

    public bool wasHitToday;      // 플레이어가 때렸는지
    public bool isSilencedToday;  // 오늘 소음 OFF인지
    public bool isActiveToday;    // NeighborManager에서 ‘오늘 활성’로 선정된 소음원인지
}

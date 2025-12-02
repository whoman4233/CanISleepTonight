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
    public bool isActiveToday = false; // 오늘 하루 활성화 여부 (준비 페이즈에서 결정)

    // 월드 상 위치/참조
    public Transform worldTransform;   // DistractionAnchor에서 가져오는 Transform
    public string placeId;             // Noise/Wave에서 사용할 위치 ID

    // 옵션: 소음/디버그용 캐시
    public bool isNoiseSource = true;      // 실제 소음원인지 여부(필요하면 사용)
    public float cachedNoiseContribution;  // NoiseManager 계산 결과 캐시

    public void SetDead()
    {
        isAlive = false;
        isActiveToday = false;
        isNoiseSource = false;
    }

    public DistractionRuntime(DistractionDataRow dataRow)
    {
        data = dataRow;
        // 1차 기본값: CSV에서 온 placeId
        placeId = dataRow.placeId;
    }

    /// <summary>
    /// Distraction의 최종 placeId를 확정하는 단계.
    /// 우선순위:
    /// 1) DistractionAnchor.PlaceId
    /// 2) CSV 데이터 placeId
    /// 3) owner.placeId
    /// </summary>
    public void FinalizePlaceId()
    {
        // 1) worldTransform 기준으로 DistractionAnchor 우선
        if (worldTransform != null)
        {
            var anchor = worldTransform.GetComponent<DistractionAnchor>();
            if (anchor != null && !string.IsNullOrEmpty(anchor.PlaceId))
            {
                placeId = anchor.PlaceId;
                return;
            }
        }

        // 2) 생성자에서 dataRow.placeId를 이미 넣어둠
        if (!string.IsNullOrEmpty(placeId))
            return;

        // 3) 데이터/앵커 둘 다 비었으면 owner의 placeId 상속
        if (owner != null && !string.IsNullOrEmpty(owner.placeId))
        {
            placeId = owner.placeId;
        }
    }


}

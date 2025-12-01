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
        placeId = dataRow.placeId; // 기본값은 데이터 기준, 프리팹에서 override 가능
    }
}
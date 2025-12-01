using System.Collections.Generic;
using UnityEngine;

public class NeighborRuntime
{
    // 원본 데이터
    public readonly NeighborDataRow data;

    // 식별자 편의 프로퍼티
    public string Id => data.neighborId;

    // 월드/씬 관련 참조
    //public HouseSlot houseSlot;        // 씬 상의 집 자리 (MonoBehaviour)
    public GameObject houseInstance;   // Instantiate된 집 프리팹 루트

    // (선택) 이웃 캐릭터 뷰
    //public NeighborView neighborView;  // 있으면 연결

    // 상태
    public bool isAlive = true;        // 영구적으로 조용해졌는지(Dead 여부)
    public bool isActiveToday = false; // 오늘 활성 이웃인지

    // (나중 확장용) 의심도
    public int suspicion = 0;

    // 하위 방해 요소들
    public readonly List<DistractionRuntime> distractions = new List<DistractionRuntime>();

    public NeighborRuntime(NeighborDataRow dataRow)
    {
        data = dataRow;
    }
}
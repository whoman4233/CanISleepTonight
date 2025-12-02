// InteractionCategory: 대화 / 설득 / 수리
public enum InteractionCategory
{
    Unknown = 0,
    Talk,       // 대화
    Persuade,   // 설득
    Fix         // 수리
}

// ReqType: 조건 체크 타입
public enum InteractionReqType
{
    None = 0,
    Item,       // 인벤토리 아이템 필요 (ReqValue = Item_Hammer 등)
    State,      // calm/mad 같은 상태
    Stat        // 나중에 stress<80 같은 조건 쓰고 싶으면 확장
}

// ResultType: 상호작용 결과 타입
public enum InteractionResultType
{
    None = 0,

    // 말풍선 / 컷씬 / 엔딩
    ShowText,
    PlayCutScene,
    GameEnd,

    // 상태 변경
    ChangeType,     // N_xxx / D_xxx / E_xxx 의 LifeState 변경 (Idle/Dead 등)

    // 수치 변경
    ModStat,        // stress, Time 등
    VolumeChange    // D_xxx 볼륨 증감
}

// LifeState: 실제 런타임 상태
public enum LifeState
{
    Idle,   // 존재하지만 활동 안 함 (내일 다시 Active 가능)
    Active, // 활동 중
    Dead    // 영구 정지
}

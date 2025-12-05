using UnityEngine;

public class PrototypeGameBootstrap : MonoBehaviour
{
    [Header("Managers")]
    public NeighborManager neighborManager;
    public PlayerLocationTracker playerLocation;

    [Header("Initial Day / Location")]
    [Tooltip("시작할 날짜 인덱스 (0 = 1일차, 1 = 2일차 ...)")]
    public int startDayIndex = 0;

    [Tooltip("시작 층 (예: 3층이면 3)")]
    public int startFloor = 3;

    [Tooltip("시작 시 집 안에서 시작할지 여부")]
    public bool startInsideHouse = true;

    [Tooltip("집 안에서 시작할 경우, PlayerLocationTracker.currentHouseSlotId에 넣을 houseSlotId")]
    public string startHouseSlotId = "303";  // HouseSlot.houseSlotId와 동일하게 맞춰줄 것

    [Header("Debug Controls")]
    [Tooltip("N 키로 다음 날로 넘기는 디버그 기능 사용 여부")]
    public bool enableDebugNextDayKey = true;
    public KeyCode nextDayKey = KeyCode.N;

    private int currentDayIndex;

    private void Awake()
    {
        // 인스펙터에서 안 넣었으면 자동 할당 시도
        if (neighborManager == null)
            neighborManager = FindObjectOfType<NeighborManager>();

        if (playerLocation == null)
            playerLocation = FindObjectOfType<PlayerLocationTracker>();
    }

    private void Start()
    {
        if (neighborManager == null)
        {
            Debug.LogError("[PrototypeGameBootstrap] NeighborManager가 할당되어 있지 않습니다.");
            return;
        }

        // 1. 일주일 런타임 초기화 (한 번만)
        neighborManager.InitializeWeek();

        // 2. 시작 날짜 세팅
        SetupDay(startDayIndex);
    }

    private void SetupDay(int dayIndex)
    {
        if (neighborManager == null)
            return;

        currentDayIndex = dayIndex;

        // 하루 세팅 (오늘 활성 이웃/방해요소 결정)
        neighborManager.SetupDay(dayIndex);

        // 플레이어 위치 초기화
        if (playerLocation != null)
        {
            playerLocation.SetFloor(startFloor);

            if (startInsideHouse && !string.IsNullOrEmpty(startHouseSlotId))
                playerLocation.EnterHouse(startHouseSlotId);
            else
                playerLocation.ExitHouse();
        }

        Debug.Log($"[PrototypeGameBootstrap] Day {dayIndex + 1} setup complete.");
    }

    private void Update()
    {
        if (!enableDebugNextDayKey || neighborManager == null)
            return;

        if (Input.GetKeyDown(nextDayKey))
        {
            // 현재 하루 종료
            neighborManager.EndDay();

            // 다음 날로
            int nextDay = currentDayIndex + 1;
            SetupDay(nextDay);
        }
    }
}
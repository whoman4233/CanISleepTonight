using System;
using System.Collections;
using UnityEngine;

// 게임 상태
public enum GamePhase
{
    NotStarted,   // 인트로 화면
    Preparation,  // 준비 페이즈
    Commute,      // 퇴근 페이즈
    Action,       // 액션 페이즈 (6분 플레이 타임 6분)
    Settlement,   // 정산 페이즈
    GoToWork,     // 출근 페이즈
    GameOver      // 게임 오버
}

public enum SleepQuality
{
    None,         // 수면하지 않음
    DeepSleep,    // 숙면 (소음 < 30)
    LightSleep,   // 일반 수면 (30 <= 소음 < 60)
    Impossible    // 수면 불가 (소음 >= 60)
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    // Bootstrap 참조
    [Header("Bootstrap")]
    [SerializeField] private PrototypeGameBootstrap bootstrap;


    // Bootstrap에서 매니저 가져오기
    private NeighborManager NeighborManager => bootstrap?.neighborManager;
    private PlayerLocationTracker PlayerLocation => bootstrap?.playerLocation;

    [Header("Player")]
    [SerializeField] private PlayerCondition playerCondition;

    
    // 게임 설정
    [Tooltip("액션 페이즈 지속 시간 (초) - 기본 360초 = 6분")]
    private float actionPhaseDuration = 360f;

    [Tooltip("하루 총 일수 (기본 7일)")]
    private int totalDays = 7;

    
    // 소음 기반 수면 설정
    [Tooltip("숙면 가능 최대 소음 (< 30)")]
    private float deepSleepNoiseThreshold = 30f;

    [Tooltip("수면 가능 최대 소음 (< 60)")]
    private float lightSleepNoiseThreshold = 60f;

    [Tooltip("숙면 계수 (초당 피로도 회복) = 0.25")]
    private float deepSleepRecoveryRate = 0.25f;

    [Tooltip("수면 계수 (초당 피로도 회복) = 0.20")]
    private float lightSleepRecoveryRate = 0.20f;

    [Tooltip("소음 계수 (소음 1당 피로도 회복 감소)")]
    private float noisePenaltyCoefficient = 0.01f;
    

    public GamePhase _currentPhase = GamePhase.NotStarted;
    private int _currentDay = 0;


    // 액션 페이즈 타이머
    public float _actionPhaseTimer = 0f;
    private bool _isActionPhaseRunning = false;


    // 수면 데이터
    private bool _isSleeping = false;
    private float _sleepStartTime = -1f;
    private float _noiseAtSleepStart = 0f;
    private SleepQuality _currentSleepQuality = SleepQuality.None;

    private bool _isGameOver = false;

    private bool _canMove = true;
    public bool CanMove => _canMove;

    private float _currentNoise;
    public float CurrentNoise => _currentNoise;

    private bool _isGameEnded = false;
    public bool IsGameEnded => _isGameEnded;


    // 프로퍼티
    public GamePhase CurrentPhase => _currentPhase;
    public int CurrentDay => _currentDay;
    public float ActionPhaseTimer => _actionPhaseTimer;
    public float ActionPhaseDuration => actionPhaseDuration;
    public bool IsGameOver => _isGameOver;
    public bool IsSleeping => _isSleeping;
    public SleepQuality CurrentSleepQuality => _currentSleepQuality;


    // 이벤트
    public event Action<GamePhase> OnPhaseChanged;
    public event Action<int> OnDayChanged;
    public event Action<string> OnGameEnded;
    public event Action<SleepQuality> OnSleepStarted;


    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Bootstrap 없으면, 찾기
        if (bootstrap == null)
            bootstrap = GetComponentInChildren<PrototypeGameBootstrap>();

        // PlayerCondition 없으면, 찾기
        if (playerCondition == null)
            playerCondition = FindObjectOfType<PlayerCondition>();

        ValidateReferences();
    }

    private void Start()
    {
        // TODO : 인트로 씬 구현 후, 버튼과 연결
        StartNewGame();
    }

    private void ValidateReferences()
    {
        if (bootstrap == null)
        {
            Debug.LogError("[GameManager] PrototypeGameBootstrap를 찾을 수 없습니다!");
            return;
        }

        if (NeighborManager == null)
            Debug.LogError("[GameManager] NeighborManager가 Bootstrap에 할당되지 않았습니다!");

        if (playerCondition == null)
            Debug.LogWarning("[GameManager] PlayerCondition을 찾을 수 없습니다!");
    }

    private void Update()
    {
        if (_isGameOver || _currentPhase == GamePhase.NotStarted)
            return;

        // 액션 페이즈 타이머 업데이트
        if (_currentPhase == GamePhase.Action && _isActionPhaseRunning)
        {
            UpdateActionPhase();
        }

        if (Input.GetKeyDown(KeyCode.Equals))
            _currentNoise += 20;
        if (Input.GetKeyDown(KeyCode.Minus))
            _currentNoise -= 20;

        // 피로도 100 체크 (실시간)
        CheckFatigueGameOver();
    }


    // 게임 시작 (외부에서 호출)
    public void StartNewGame()
    {
        if (bootstrap == null || NeighborManager == null)
        {
            //Debug.LogError("[GameManager] Bootstrap 또는 NeighborManager가 없습니다!");
            return;
        }

        Debug.Log("[GameManager] ===== 새 게임 시작 =====");

        // 게임 상태 초기화
        _isGameOver = false;
        _currentDay = 0;
        _isSleeping = false;

        // Bootstrap을 통한 일주일 초기화
        NeighborManager.InitializeWeek();

        // 첫날 시작
        StartDay(0);
    }


    // 하루 시작
    private void StartDay(int dayIndex)
    {
        _currentDay = dayIndex;

        // Bootstrap의 SetupDay 로직 활용
        NeighborManager.ResetHousesForNewDay();

        if (PlayerLocation != null)
        {
            PlayerLocation.SetFloor(bootstrap.startFloor);

            if (bootstrap.startInsideHouse && !string.IsNullOrEmpty(bootstrap.startHouseSlotId))
                PlayerLocation.EnterHouse(bootstrap.startHouseSlotId);
            else
                PlayerLocation.ExitHouse();
        }

        OnDayChanged?.Invoke(_currentDay);
        Debug.Log($"[GameManager] Day {_currentDay + 1} / {totalDays} 시작");

        EnterPhase(GamePhase.Preparation);
    }


    // 페이즈 전환
    private void EnterPhase(GamePhase newPhase)
    {
        _currentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
        Debug.Log($"[GameManager] >>> 페이즈 전환: {newPhase}");

        switch (newPhase)
        {
            case GamePhase.Preparation:
                OnEnterPreparation();
                break;
            case GamePhase.Commute:
                OnEnterCommute();
                break;
            case GamePhase.Action:
                OnEnterAction();
                break;
            case GamePhase.Settlement:
                OnEnterSettlement();
                break;
            case GamePhase.GoToWork:
                OnEnterGoToWork();
                break;
        }
    }


    // 페이즈별 처리
    private void OnEnterPreparation()
    {
        // TODO: Day X 시작 UI 표시
        _canMove = false;
        StartCoroutine(AutoTransitionAfterDelay(2f, GamePhase.Commute));
    }

    private void OnEnterCommute()
    {
        // TODO: 퇴근 연출
        _canMove = false;
        StartCoroutine(AutoTransitionAfterDelay(1f, GamePhase.Action));
    }

    private void OnEnterAction()
    {
        _canMove = true;

        _actionPhaseTimer = actionPhaseDuration;
        _isSleeping = false;
        _sleepStartTime = -1f;
        _noiseAtSleepStart = 0f;
        _currentSleepQuality = SleepQuality.None;
        _isActionPhaseRunning = true;

        // 플레이어를 303호 시작 위치로 리셋
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.ResetToHomePosition();

        Debug.Log($"[GameManager] 액션 페이즈 시작 : 남은 시간 {actionPhaseDuration}초");
    }

    private void UpdateActionPhase()
    {
        _actionPhaseTimer -= Time.deltaTime;

        // UIManager를 통해 타이머 UI 업데이트
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateTimer(_currentDay, _actionPhaseTimer);

        // 시간 종료
        if (_actionPhaseTimer <= 0f)
        {
            _actionPhaseTimer = 0f;
            _isActionPhaseRunning = false;

            Debug.Log("[GameManager] 액션 페이즈 종료 (시간 초과)");
            EnterPhase(GamePhase.Settlement);
        }
    }


    // 소음 계산 시스템
    public float CalculateCurrentNoise()
    {
        if (NeighborManager == null || PlayerLocation == null)
            return 0f;

        float totalNoise = 0f;

        // 오늘 활성화된 방해요소들의 소음 합산
        foreach (var distraction in NeighborManager.ActiveDistractionsToday)
        {
            if (!distraction.isAlive || !distraction.isActiveToday)
                continue;

            // 소음 강도 가져오기
            float noiseIntensity = GetDistractionNoiseIntensity(distraction);
            if (noiseIntensity <= 0f)
                continue;

            // 거리 계수 계산
            float distanceCoefficient = CalculateDistanceCoefficient(distraction);

            // 소음 누적
            totalNoise += noiseIntensity * distanceCoefficient;
        }

        return totalNoise;
    }

    private float GetDistractionNoiseIntensity(DistractionRuntime distraction)
    {
        if (distraction.data != null)
            return distraction.data.level;  

        return 0f;
    }

    // TODO : 실제 구체적인 소음 계산식 적용해야 함
    private float CalculateDistanceCoefficient(DistractionRuntime distraction)
    {
        if (PlayerLocation == null)
            return 0.5f; // 기본값

        string playerHouse = PlayerLocation.currentHouseSlotId;
        string distractionPlace = distraction.placeId;

        // 같은 집 안이면 계수 1.0 (100%)
        if (!string.IsNullOrEmpty(playerHouse) && playerHouse == distractionPlace)
            return 1.0f;

        // TODO: 실제 3D 거리 계산 또는 층/방 번호 기반 계산 구현해야 함
        // 일단 간단히 다른 집이면 0.5 (50%)
        return 0.5f;
    }


    // 침대 상호작용 (수면 시도)
    public bool CanSleep(out string reason)
    {
        if (_currentPhase != GamePhase.Action || !_isActionPhaseRunning)
        {
            reason = "지금은 잠을 잘 수 없습니다.";
            return false;
        }

        if (_isSleeping)
        {
            reason = "이미 수면 중입니다.";
            return false;
        }

        // 현재 소음 확인
        // TODO : 소음 계산식 완성되면, 주석 해제
        //float currentNoise = CalculateCurrentNoise();

        // 소음 >= 60 -> 수면 불가
        if (_currentNoise >= lightSleepNoiseThreshold)
        {
            reason = $"주변이 너무 시끄러워서 잠들 수 없습니다! (소음: {_currentNoise:F1})";
            return false;
        }

        reason = "";
        return true;
    }


    public void OnPlayerStartSleep()
    {
        if (!CanSleep(out string reason))
        {
            Debug.LogWarning($"[GameManager] 수면 불가: {reason}");
            // TODO: UI 메시지 표시
            return;
        }

        // 수면 시작
        _isSleeping = true;
        _sleepStartTime = actionPhaseDuration - _actionPhaseTimer;
        _noiseAtSleepStart = CalculateCurrentNoise();

        // 수면 품질 결정
        if (_noiseAtSleepStart < deepSleepNoiseThreshold)
        {
            _currentSleepQuality = SleepQuality.DeepSleep;
            Debug.Log($"[GameManager] 숙면 시작 (소음: {_noiseAtSleepStart:F1})");
        }
        else
        {
            _currentSleepQuality = SleepQuality.LightSleep;
            Debug.Log($"[GameManager] 일반 수면 시작 (소음: {_noiseAtSleepStart:F1})");
        }

        OnSleepStarted?.Invoke(_currentSleepQuality);

        // TODO: 수면 UI/애니메이션 전환

        // 즉시 정산 페이즈로 이동 (남은 시간 전부 수면)
        _isActionPhaseRunning = false;
        StartCoroutine(TransitionToSettlementAfterSleep());
    }


    // 수면 후, 즉시 정산페이즈로
    private IEnumerator TransitionToSettlementAfterSleep()
    {
        // 수면 연출 대기 (페이드 아웃 등)
        yield return new WaitForSeconds(1f);

        // 정산 페이즈로 전환
        EnterPhase(GamePhase.Settlement);
    }


    // 정산 페이즈
    private void OnEnterSettlement()
    {
        _canMove = false;

        _isActionPhaseRunning = false;
        _isSleeping = false;

        // 수면 시간 및 회복량 계산
        float sleepTime = CalculateSleepTime();
        float fatigueRecovery = CalculateFatigueRecovery(sleepTime);

        Debug.Log($"[GameManager] ===== 정산 페이즈 =====");
        Debug.Log($"  수면 시간: {sleepTime:F1}초 ({sleepTime / 60f:F1}분)");
        Debug.Log($"  수면 품질: {_currentSleepQuality}");
        Debug.Log($"  피로도 회복: {fatigueRecovery:F1}");

        // 피로도 회복 적용
        if (playerCondition != null && fatigueRecovery > 0f)
        {
            playerCondition.AddFatigue(-fatigueRecovery);
        }

        // TODO: 정산 UI 표시

        // 7일차 종료 체크
        if (_currentDay >= totalDays - 1)
        {
            // 노멀 엔딩 (7일 생존 성공)
            ShowEnding("Normal");
        }
        else
        {
            StartCoroutine(AutoTransitionAfterDelay(3f, GamePhase.GoToWork));
        }
    }

    private float CalculateSleepTime()
    {
        if (_sleepStartTime < 0f)
            return 0f;

        return Mathf.Max(0f, actionPhaseDuration - _sleepStartTime);
    }

    private float CalculateFatigueRecovery(float sleepTime)
    {
        if (sleepTime <= 0f)
            return 0f;

        float recovery = 0f;

        if (_currentSleepQuality == SleepQuality.DeepSleep)
        {
            // 숙면: (남은 시간) × 0.25
            recovery = sleepTime * deepSleepRecoveryRate;
        }
        else if (_currentSleepQuality == SleepQuality.LightSleep)
        {
            // 수면: (남은 시간) × 0.20 - (잔존 소음) × 0.01
            float baseRecovery = sleepTime * lightSleepRecoveryRate;
            float noisePenalty = _noiseAtSleepStart * noisePenaltyCoefficient;
            recovery = Mathf.Max(0f, baseRecovery - noisePenalty);
        }

        return recovery;
    }

    private void OnEnterGoToWork()
    {
        _canMove = false;

        // Bootstrap의 EndDay 재활용
        NeighborManager.EndDay();

        // TODO: 출근 연출
        StartCoroutine(AutoTransitionAfterDelay(1f, () =>
        {
            StartDay(_currentDay + 1);
        }));
    }


    // 배드 엔딩 처리
    private void CheckFatigueGameOver()
    {
        if (_isGameOver || playerCondition == null)
            return;

        if (playerCondition.Fatigue >= 100f)
        {
            _isGameOver = true;
            _currentPhase = GamePhase.GameOver;
            ShowEnding("Bad");
            _canMove = false;
        }
    }

    public void TriggerBadEnding(string reason)
    {
        if (_isGameOver)
            return;

        _isGameOver = true;
        _currentPhase = GamePhase.GameOver;
        ShowEnding($"Bad_{reason}");
    }

    private void ShowEnding(string endingType)
    {
        _isGameEnded = true;

        Debug.Log($"[GameManager] =============================");
        Debug.Log($"[GameManager]    {endingType} 엔딩");
        Debug.Log($"[GameManager] =============================");

        if (playerCondition != null)
        {
            Debug.Log($"  최종 피로도: {playerCondition.Fatigue:F1}");
            Debug.Log($"  최종 스트레스: {playerCondition.Stress:F1}");
        }

        OnGameEnded?.Invoke(endingType);

        // 엔딩 UI 표시
        UIManager.Instance?.ShowEndingPanel(endingType);
    }


    // 유틸리티
    private IEnumerator AutoTransitionAfterDelay(float delay, GamePhase nextPhase)
    {
        yield return new WaitForSeconds(delay);
        EnterPhase(nextPhase);
    }

    private IEnumerator AutoTransitionAfterDelay(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }


    // 디버그 (에디터 전용)
#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool showDebugGUI = true;

    private void OnGUI()
    {
        if (!showDebugGUI)
            return;

        /*GUILayout.BeginArea(new Rect(10, 10, 350, 180));
        GUILayout.Box("=== GAME MANAGER DEBUG ===");
        GUILayout.Label($"Phase: {_currentPhase}");
        GUILayout.Label($"Day: {_currentDay + 1} / {totalDays}");
        GUILayout.Label($"Timer: {_actionPhaseTimer:F1}s / {actionPhaseDuration}s");
        GUILayout.Label($"Current Noise: {CalculateCurrentNoise():F1}");
        GUILayout.Label($"Sleeping: {_isSleeping} ({_currentSleepQuality})");

        if (playerCondition != null)
        {
            GUILayout.Label($"Fatigue: {playerCondition.Fatigue:F1} / 100");
            GUILayout.Label($"Stress: {playerCondition.Stress:F1} / 100");
        }

        GUILayout.Label("");
        GUILayout.Label("Keys: [F]+20피로 [Z]수면 [X]깨기 [N]다음날");
        GUILayout.EndArea();*/

        // 디버그 키
        if (Input.GetKeyDown(KeyCode.F))
            playerCondition?.AddFatigue(20f);

        if (Input.GetKeyDown(KeyCode.Z))
            OnPlayerStartSleep();

        if (Input.GetKeyDown(KeyCode.N) && _currentPhase == GamePhase.Action)
        {
            _isActionPhaseRunning = false;
            EnterPhase(GamePhase.Settlement);
        }
    }
#endif
}
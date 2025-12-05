# CanISleepTonight
CanISleepTonight?(오늘은 잠들 수 있을까?)

# 이웃 & 소음 시스템 (Neighbor / Distraction / Noise)

## 1. 개요
이 시스템은 아파트 형태의 맵에서 **이웃(Neighbor)** 과 그들이 만들어내는 **방해 요소(Distraction, 소음원)** 를 데이터 기반으로 관리하고, 게임 내 하루(Day) 단위로 **랜덤 소음 발생 / 플레이어의 제압(때리기) / 다음 날 리셋** 흐름을 구성하는 역할을 합니다.

**핵심 목표는 다음과 같습니다.**
* CSV + ScriptableObject 기반의 **테이블 드리븐 구조**
* 하루마다 랜덤으로 선정되는 **“오늘의 시끄러운 이웃들”**
* 거리에 따른 가중치를 반영한 **전체 소음값 계산**
* 타격 1회 → 그 날만 소음 OFF, 다음 날 자동 복구
* 각 Distraction마다 **3D 오디오**를 사용해 위치 기반 사운드 표현

---

## 2. 주요 데이터 구조

### 2-1. NeighborTable / DistractionTable / PlaceTable

**NeighborTableSO**
* **CSV:** `NeighborID`, `Name`, `LayoutID`, `Info` ...
* 각 이웃의 ID, 표시 이름, 사용할 인테리어 레이아웃 ID를 정의합니다.

**DistractionTableSO**
* **CSV:** `DistractionID`, `OwnerID`, `NoiseID`, `Tag`, `Level`, `SFXID`, `PlaceID`, `Info`
* 한 이웃이 가질 수 있는 개별 소음원을 정의합니다.
* `Level` 값은 소음 강도를 의미하며, 거리 가중치와 함께 최종 소음 계산에 사용됩니다.
* `SFXID` 를 통해 실제 오디오 클립과 연결됩니다.

**PlaceTableSO**
* **CSV:** `PlaceID`, `Floor`, `DistanceLevel`
* `PlaceID` 는 호실/위치 ID (예: `P_303`, `L_05` 등)
* `DistanceLevel` 은 플레이어 기준 거리 레벨 (0 ~ 5) 로, `NoiseManager` 에서 소음 가중치로 사용됩니다.

### 2-2. 런타임 모델

**NeighborRuntime**
원본 `NeighborDataRow` 를 래핑한 런타임 클래스
* **주요 필드:**
    * `Id` (NeighborID)
    * `data` (원본 데이터)
    * `houseSlot` / `houseInstance` (어느 HouseSlot에 어떤 프리팹이 올라가 있는지)
    * `placeId` (실제 배치된 위치)
    * `distractions` : `List<DistractionRuntime>`
    * `isAlive`, `isActiveToday`

**DistractionRuntime**
원본 `DistractionDataRow` 를 래핑한 런타임 클래스
* **주요 필드:**
    * `Id` / `OwnerId`
    * `data` (원본 데이터, 여기 포함된 `level`, `sfxId`, `placeId` 사용)
    * `owner` : `NeighborRuntime`
    * `anchor` : `DistractionAnchor` (씬 오브젝트와의 연결)
    * `placeId` (실제 최종 위치, 앵커/이웃 정보를 반영해 확정)
    * `isAlive` (설계상 영구 죽음은 사용 안 함 / true 고정)
    * `isActiveToday` (오늘 소음 후보인지)
    * `wasHitToday`, `isSilencedToday` (오늘 플레이어가 해결했는지 여부)

---

## 3. 하우스 & 이웃 배치 흐름

### 3-1. HouseSlot & PlayerLocationTracker

**HouseSlot**
* 각 호실의 인테리어가 붙는 지점(Transform)을 가진 슬롯 컴포넌트
* `houseSlotId`, `placeId`, `InteriorRoot` 등을 보유

**PlayerLocationTracker**
* `currentFloor`, `currentHouseSlotId` 를 들고 있는 플레이어 위치 트래커
* 플레이어의 방(예: `P_303`)을 따로 관리하도록 도와줌

### 3-2. NeighborManager 초기화 흐름

    GameManager → NeighborManager.InitializeWeek()
        ├─ ClearAllRuntime()
        ├─ BuildRuntimeFromData()            // 테이블 → NeighborRuntime / DistractionRuntime 생성
        ├─ AssignHouseSlotsAndInstantiateHouses()
        │     ├─ 플레이어 방 슬롯(P_303) 분리
        │     ├─ 나머지 HouseSlot 셔플
        │     ├─ 이웃들을 랜덤 슬롯에 배정 후, layoutPrefab 인스턴스 생성
        │     ├─ 남는 슬롯은 EMPTY 레이아웃으로 채움
        │     └─ 플레이어 방 슬롯에는 항상 PlayerRoom 프리팹 배치
        └─ LinkDistractionAnchors()          // 프리팹 내 DistractionAnchor ↔ DistractionRuntime 다시 연결

**AssignHouseSlotsAndInstantiateHouses()**
* `locationTracker.currentHouseSlotId` 와 `slot.placeId` 를 비교해 **플레이어 방 슬롯을 분리**합니다.
* 나머지 슬롯을 셔플하여 이웃들을 랜덤 배치합니다.
* 이웃마다 `layoutId` 에 해당하는 인테리어 프리팹을 `HouseSlot.InteriorRoot` 하위에 인스턴스화합니다.

**ResetHousesForNewDay()**
* 하루가 바뀔 때, 기존 인테리어 프리팹을 전부 `Destroy` 후 같은 규칙으로 인테리어를 다시 인스턴스화합니다.
* 마지막에 `LinkDistractionAnchors()` 를 다시 호출하여 **새로 생성된 프리팹 기준 DistractionAnchor 재연결**합니다.

---

## 4. DistractionAnchor & 타격(때리기) 처리

### 4-1. IHittable 인터페이스

    public interface IHittable
    {
        void OnHit();
    }

* 근접 무기에서 `Raycast` / `Collider` 를 통해 맞은 오브젝트가 `IHittable` 을 구현하고 있으면 `OnHit()` 호출
* 이웃 상호작용 중 “때리는” 케이스에서만 사용

### 4-2. DistractionAnchor

**역할**
* 프리팹 상의 콜라이더/모델과 `DistractionRuntime` 를 연결하는 앵커
* `IHittable` 구현 → 맞았을 때 오늘 소음 OFF + 콜라이더 비활성화
* AudioSource와 `NoiseSfxEntry` 를 잡아서 소리 재생/정지 관리

**주요 동작**
* **BindRuntime(DistractionRuntime runtime, NoiseSfxEntry sfxEntry)**
    * 런타임 모델과 앵커를 서로 참조로 연결 (`Runtime.anchor = this`)
    * `Runtime.worldTransform = transform;`
    * 앵커에 설정된 `placeId` 가 있으면 런타임 `placeId` 를 덮어씀
    * `audioSource.clip`, `volume`, `loop` 설정
    * `audioSource.outputAudioMixerGroup = AudioManager.Instance.SfxMixerGroup` 로 SFX 믹서에 연결

* **OnHit()**
    * 이미 오늘 한 번 맞았으면 무시
    * `NeighborManager.SetDistractionDead(distractionId)` 호출
        * → `wasHitToday = true`, `isSilencedToday = true`, `isActiveToday = false`
    * `audioSource.Stop()` 호출
    * 다음부터는 더 이상 때려도 반응하지 않도록 콜라이더 비활성화

* **EnsureAudioForToday(bool verbose = false)**
    * `Runtime.isAlive && Runtime.isActiveToday` 인 경우에만
    * `audioSource` 가 있고, `clip` 이 있으며, 재생 중이 아니면 `Play()`
    * 아니라면(재생 중이면) `Stop()`

---

## 5. 하루 단위 소음(Noise) 계산

### 5-1. NeighborManager의 “오늘 소음원 선정”

    GameManager → NeighborManager.SetupDay(dayIndex)
        ├─ 전체 이웃의 isActiveToday 초기화
        ├─ 집이 배정된 이웃들만 aliveNeighbors 후보로 수집
        ├─ minActiveNeighbors ~ maxActiveNeighbors 사이에서 오늘 시끄러운 이웃 수 결정
        ├─ aliveNeighbors 셔플 후 상위 N명을 오늘의 이웃으로 선정
        ├─ 각 이웃의 Distraction 중 isAlive == true 인 것들을 오늘 소음원으로 추가
        └─ _activeNeighborsToday / _activeDistractionsToday 에 캐시

* **SetDistractionDead() 이 호출되면:**
    * `runtime.wasHitToday = true`
    * `runtime.isSilencedToday = true`
    * `runtime.isActiveToday = false`
    * `_activeDistractionsToday` 리스트에서도 제거

### 5-2. NoiseManager
`NoiseManager` 는 매 프레임 일정 간격(tickInterval)마다 전체 소음을 재계산합니다.

* **Update()**
    * `_tickTimer` 누적 → tickInterval 초과 시 `CalculateNoise()`

* **CalculateNoise() 의 주요 로직**
    1. `neighborManager.ActiveDistractionsToday` 목록 가져오기
    2. 각 `DistractionRuntime d` 에 대해:
        * `!d.isAlive` 이거나 `!d.isActiveToday` 이면 스킵
        * 스킵 시에도 앵커가 있으면 `EnsureAudioForToday()` 호출해 정리
    3. `placeId` 로부터 거리 레벨 조회:
        * `neighborManager.GetDistanceLevel(d.placeId)`
        * → 내부에서 `PlaceTableSO` 의 `PlaceID → distanceLevel` 매핑 사용
    4. 거리 계수 `coef` 을 `distanceLevel` 로부터 계산
    5. `noise += d.data.level * coef`
    6. `anchor.EnsureAudioForToday()` 호출로 실제 재생 상태 보정
    7. `currentNoise = Mathf.Clamp(totalNoise, 0f, 100f);`
    8. `noiseDebugLog` 가 켜져 있으면 `DumpTodayNoiseState()` 와 함께 각 Distraction의 기여도 로그 출력

---

## 6. 하루 리셋 & 소음/프리팹 초기화
하루가 바뀔 때 흐름은 다음과 같이 설계되어 있습니다.

    GameManager
        ├─ NeighborManager.EndDay()
        │    └─ 오늘자 활성 플래그/리스트 정리
        ├─ NeighborManager.ResetHousesForNewDay()
        │    ├─ 모든 HouseSlot.InteriorRoot 자식 오브젝트 Destroy
        │    ├─ 이웃별 layout 프리팹 재생성
        │    ├─ 플레이어 방 프리팹 재생성
        │    └─ 나머지 슬롯은 EMPTY 프리팹으로 채움
        └─ NeighborManager.SetupDay(nextDayIndex)
             └─ 다음 날의 활성 이웃/소음원 재계산

* `ResetHousesForNewDay()` 이후 `LinkDistractionAnchors()` 가 다시 호출되므로, **레그돌, 트랜스폼 변경 등 모든 상태는 프리팹 재생성으로 리셋**됩니다.
* 각 DistractionAnchor 의 오디오/콜라이더 상태도 초기화됨
* 별도 “영구 비활성화” 개념은 사용하지 않고, **하루마다 완전 초기화 → 오늘만 해결하면 되는 구조**로 유지

---

## 7. 담당 파트 정리
아파트 층별 이웃 및 소음 시스템을 설계/구현했습니다.

* CSV + ScriptableObject 기반의 Neighbor/Distraction/Place 테이블을 바탕으로, 하루마다 랜덤으로 시끄러운 이웃을 선정하고, 플레이어의 상호작용(타격)에 따라 해당 날짜 동안만 소음이 비활성화되며, 다음 날에는 프리팹 재생성과 함께 전체 구조가 리셋되는 **데이터 드리븐 형태의 이웃/소음 관리 시스템을 구현했습니다.**

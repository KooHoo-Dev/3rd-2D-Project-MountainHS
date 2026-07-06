# 프로젝트 구조 분석

기준: 현재 `main` 브랜치의 `Assets/_Scripts`, `Assets/_Prefabs`, `Assets/_Data`, `Assets/_Scenes`를 읽어 정리했다.  
목적: UML 다이어그램 작성을 위한 클래스/관계/프리팹 기초 자료 정리.

## 1. 전체 구조 요약

- 핵심 흐름은 `GameManager`가 씬 전환과 전투 결과, `GameContext` 저장 데이터를 관리하고, 전투 씬 로드 시 `SceneBinder`가 플레이어/적 함선과 `CombatSystem`을 연결하는 구조다.
- `GameManager`는 `EnemyDatabase`, `WeaponDatabase`, `GameContext`, `PlayerController`를 중심으로 전투 준비 상태와 전투 결과를 이어준다.
- 전투 씬은 `PlayerController`와 `EnemyController`가 각각 `ShipController`를 상속하고, 같은 함선 시스템 묶음인 `WeaponSystem`, `ShieldSystem`, `EngineSystem`, `CrewSystem`, `ShipPowerController`, `ReactorSystem`을 사용한다.
- 적 함선은 씬에 고정된 오브젝트만 쓰는 구조가 아니라, `SceneBinder`가 `GameContext.EnemyType`을 보고 `EnemyDatabase`에서 적 프리팹을 가져와 생성할 수 있다.
- 무기는 `ShipController.InitWeapons()`에서 `WeaponTypes`와 `WeaponDatabase`를 이용해 동적으로 생성된다. 생성된 무기에는 `WeaponController`, `WeaponViewer`, 투사체 prefab 참조가 연결된다.
- 전투 판정은 `ProjectileController -> CombatSystem -> ShipController/ShieldSystem/Room` 흐름으로 이어진다. 피해 후에는 `FloatingTextController`, `ExplosionController`, `SpriteShakeController` 같은 표현 계열도 함께 호출된다.
- 환경 이벤트는 `SceneBinder`가 `EnvironmentController.InitStageEnvironment()`를 호출하면서 시작된다. `EnvironmentController`는 태양 플레어/소행성 공격을 통해 `ShipController.TakeDamageToRoom()` 또는 `ProjectileController` 기반 공격을 발생시킨다.
- 씬 간 유지 데이터는 `GameContext`가 담당한다. 저장되는 주요 값은 방 목록, 시설 상태, 승무원 상태, 스크랩, 함선 HP, 반응로 전력, 무기 타입 목록, 적 타입이다.

## 2. 클래스 구조 정리

### 2.1 핵심 매니저/시스템

| 클래스 | 역할 | 주요 참조 | 주요 책임 | UML 표시 포인트 |
|---|---|---|---|---|
| `GameManager` | 전역 게임/전투 흐름 관리자 | `GameContext`, `PlayerController`, `MainUIController`, `SceneBinder`, `ShopManager`, `EnemyDatabase`, `WeaponDatabase`, `BackgroundController`, `CursorController` | 씬 로드 처리, 전투 결과 처리, 전투 일시정지/정지, `GameContext` 저장/복원, DB 제공 | `SingletonBase<GameManager>` 상속, `GameContext` 소유 |
| `CombatSystem` | 전투 충돌/피해 판정 관리자 | `PlayerController`, `EnemyController`, `ProjectileController`, `ShieldSystem`, `ShipController`, `Room`, `FloatingTextController`, `ExplosionController`, `SpriteShakeController` | 함선 영역 바인딩, 투사체 영역 이동, 실드/방 충돌 처리, 피해 표시/폭발 풀 관리 | `SingletonBase<CombatSystem>` 상속, 양 함선 배열 보유 |
| `SceneBinder` | 전투 씬 런타임 바인더 | `GameContext`, `PlayerController`, `EnemyController`, `EnemyDatabase`, `CombatSystem`, `EnvironmentController` | 플레이어 상태 복원, 적 프리팹 생성/기존 적 탐색, 적 UI 그룹 연결, 전투/환경 초기화 | `GameManager.OnSceneLoaded`에서 호출 |
| `ShopManager` | 전투 준비/상점 관리자 | `GameManager`, `GameContext`, `CrewViewer`, UI `Button` | 시설 최대 전력 업그레이드, 반응로 업그레이드, 승무원 회복, 함선 수리 | `BattlePrepare` 중심 |
| `ShipController` | 함선 공통 기반 시스템 | `ShipData`, `Room`, `CrewController`, `CrewSystem`, `FacilitySystem`, `WeaponSystem`, `ShipPowerController`, `WeaponDatabase`, `WeaponTransformData` | 방/승무원/무기 생성, 피해 처리, 사망 판정, 전력 요청, 무기 타입 적용 | `PlayerController`, `EnemyController` 부모, `IDamageable`, `IDeadable` 구현 |
| `FacilitySystem` | 시설 공통 기반 | `ShipController`, `FacilityState`, `ERoomType`, `EFacilityStatus`, `GameManager` | 시설 전력/피해/수리/보조 상태 관리, 전투 실행 가능 여부 제공 | `WeaponSystem`, `ShieldSystem`, `EngineSystem` 부모 |
| `WeaponSystem` | 무기 시설 시스템 | `WeaponController`, `ITargeting`, `ProjectileTargetData`, `CursorController` | 무기 전원 상태 반영, 선택 무기/타겟 관리, 적 자동 타겟팅 전달, 타겟 표시 스프라이트 제어 | `FacilitySystem` 상속, `WeaponController` 목록 소유 |
| `ShieldSystem` | 실드 시설 시스템 | `CombatSystem`, `ShieldViewer` | 전력 기반 최대 실드 계산, 실드 충전, 실드 피해 처리 | `FacilitySystem` 상속 |
| `EngineSystem` | 엔진 시설 시스템 | `EngineViewer` | 전력 기반 도주 충전, 도주 가능 여부 계산 | `FacilitySystem` 상속 |
| `ShipPowerController` | 함선 전력 배분 시스템 | `ReactorSystem`, `FacilitySystem` | 시설 전력 할당/회수, 시설 피해로 인한 전력 오버플로 처리, 상태 복원 | `EnemyPowerController` 부모 |
| `EnemyPowerController` | 적 전력 초기 배분 | `PowerData`, `ShipPowerController` | 시작 시 실드/무기/엔진 전력 자동 할당 | `ShipPowerController` 상속 |
| `ReactorSystem` | 반응로 전력 저장 | 없음 | 최대 전력/현재 전력 증감 | `MonoBehaviour` |
| `CrewSystem` | 승무원 그룹/선택 관리자 | `ShipController`, `CrewController`, `Room`, `FacilitySystem`, `GameManager` | 선택 승무원 관리, 목표 방 이동 명령, 사망 승무원 제거, 승무원 로직 실행 가능 여부 제공 | `ShipController`와 상호 참조 |
| `EnvironmentController` | 전투 환경 이벤트 시스템 | `CombatSystem`, `ShipController`, `Room`, `ProjectileController`, `SolarFlareController` | 일정 주기 환경 피해, 소행성 투사체 생성, 태양 플레어 연출 후 방 피해 | `SceneBinder`가 초기화 |

### 2.2 Controller 계열

| 클래스 | 역할 | 주요 참조 | 주요 책임 | UML 표시 포인트 |
|---|---|---|---|---|
| `PlayerController` | 플레이어 함선 입력 컨트롤러 | `GameManager`, `CombatSystem`, `WeaponSystem`, `CrewSystem`, `SimpleInput` | 마우스 클릭 판독, 적 방 조준, 승무원 선택/이동, 시설 전력 키 입력 | `ShipController` 상속 |
| `EnemyController` | 적 함선 컨트롤러 | `RandomTargeter`, `CombatSystem`, `WeaponSystem`, `ShipOwnership` | 플레이어 방 목록으로 랜덤 타겟팅 구성, 적 진영 설정 | `ShipController` 상속 |
| `WeaponController` | 개별 무기 컨트롤러 | `WeaponSystem`, `ProjectileController`, `ProjectileTargetData`, `ITargeting` | 충전, 발사 가능 판정, 다중 발사 코루틴, 투사체 생성/타겟 전달 | `WeaponSystem`이 생성/소유 |
| `ProjectileController` | 투사체 컨트롤러 | `CombatSystem`, `GameManager`, `ProjectileTargetData`, `Collider2D` | 투사체 이동, 시작/종료 함선 영역 판정, 실드/방 충돌 요청, 폭발 생성 요청 | 무기/환경 공격 모두에서 사용 |
| `CrewController` | 개별 승무원 컨트롤러 | `CrewSystem`, `Room`, `FacilitySystem`, `CrewState` | 방 이동, 수리, 시설 보조, 피해/사망 처리 | `CrewSystem`이 관리 |
| `CursorController` | 커스텀 마우스/조준 커서 | `SimpleInput`, `WeaponSystem` | 마우스 위치 추적, 무기 조준 커서 변경/복구 | `SingletonBase<CursorController>` 상속 |
| `BackgroundController` | 전투 배경 컨트롤러 | `Image`, `Sprite` 목록 | 전투 씬 배경 랜덤 선택 | `GameManager.OnSceneLoaded`에서 호출 |
| `ExplosionController` | 폭발 효과 컨트롤러 | 없음 | 활성화 후 지정 시간 뒤 비활성화 | `CombatSystem` 풀에서 사용 |
| `FloatingTextController` | 피해 숫자 표시 컨트롤러 | `TMP_Text` | 텍스트/색상 설정, 이동, 일정 시간 후 비활성화 | `CombatSystem` 풀에서 사용 |
| `SolarFlareController` | 태양 플레어 연출 컨트롤러 | `Image` | 플레어 색상 보간 연출 | `EnvironmentController`가 호출 |
| `SpriteShakeController` | 스프라이트 흔들림 컨트롤러 | `SpriteRenderer`, `Transform` | 함선/방 피격 흔들림 연출 | `CombatSystem`, `RoomViewer`에서 사용 |
| `CameraShakeController` | 카메라 흔들림 컨트롤러 | `Camera.main` | 카메라 위치 흔들림 연출 | 현재 `CombatSystem`에 참조 있으나 일부 호출은 주석 처리 |
| `AspectRatioController` | 카메라 비율 컨트롤러 | `Camera.main` | 16:9 레터박스 적용 | `Main Camera.prefab` |

### 2.3 Viewer/UI 계열

| 클래스 | 역할 | 주요 참조 | 주요 책임 | UML 표시 포인트 |
|---|---|---|---|---|
| `MainUIController` | 공통 메뉴/씬 이동 UI | `GameManager`, `BattleResultViewer`, `SceneManager` | 씬 이동, 메인 메뉴 복귀, 게임 종료, 도주 요청, 전투 결과 UI 표시 | `Main UI.prefab` |
| `BattleResultViewer` | 전투 결과 UI | `GameManager`, `EBattleResultExtensions` | 승패/보상 텍스트 표시, 결과에 따른 씬 이동 트리거 | `Battle Result UI.prefab` |
| `CrewViewer` | 승무원 상태 UI | `CrewController`, `GameManager`, `CrewState` 간접 | 이름/HP 표시, 선택 글로우, 목표 방 표시, 상점 회복 UI 표시 | 전투/상점 겸용 |
| `EngineViewer` | 엔진 도주 UI | `EngineSystem` | 도주 충전바, 점프 버튼 표시 | `EngineSystem` 참조 |
| `FacilityPowerViewer` | 시설 전력/상태 UI | `FacilitySystem`, `GameManager`, `ERoomTypeExtensions` | 전력바, 피해바, 보조 아이콘, 시설명 텍스트, 적 시설 자동 탐색 | `PowerViewer` 상속 |
| `HpViewer` | 함선 HP UI | `ShipController`, `GameManager` | HP 바 생성/색상 갱신, 적 함선 자동 탐색 옵션 | 전투/상점 겸용 |
| `PowerViewer` | 기본 전력바 UI | `FacilitySystem` | 시설 전력바 생성/색상 갱신 | `FacilityPowerViewer` 부모 |
| `ReactorViewer` | 반응로 전력 UI | `ReactorSystem`, `GameManager` | 반응로 전력칸 표시, `GameContext` 기반 표시 가능 | `Reactor Canvas.prefab` |
| `RoomViewer` | 방 표시 UI/연출 | `Room`, `SpriteShakeController` | 방 번호/타입 색상 표시, 피격 시 흔들림 요청 | `Room.prefab` |
| `ScrapViewer` | 스크랩 표시 UI | `GameManager` | 현재 스크랩 텍스트 표시 | `Scrap UI.prefab` |
| `ShieldViewer` | 실드 UI | `ShieldSystem` | 실드 칸/충전바/메인 실드 표시, 실드 재생성 연출 | `Shield UI.prefab` |
| `ShipViewer` | 함선 방 연결 시각화 | `ShipController`, `RoomViewer`, `Room` | 방 Viewer 동기화, 방 연결선/Gizmos 생성 | `Room.neighborRoomIds`를 reflection으로 읽음 |
| `WeaponViewer` | 무기 충전 UI | `WeaponController`, `Image` | 무기 충전 스프라이트/바 갱신, 런타임 충전바 주입 | 무기 prefab에 붙음 |

### 2.4 데이터/상태 클래스

| 클래스 | 역할 | 주요 필드 | 사용 위치 | UML 표시 포인트 |
|---|---|---|---|---|
| `GameContext` | 씬 간 저장 데이터 | `Rooms`, `FacilityStates`, `CrewStates`, `WeaponTypes`, `EnemyType`, `ScrapAmount`, `ShipHp`, `ShipPower` | `GameManager`, `SceneBinder`, `ShopManager`, Viewer | 순수 `[Serializable]` 데이터 클래스 |
| `ShipData` | 함선 구성 ScriptableObject | `MaxHp`, `List<Room>` | `ShipController` | `PlayerShipData`, `EnemyShip1/2/3Data` |
| `EnemyDatabase` | 적 프리팹 DB | `EnemyPrefabEntry[]` | `GameManager`, `SceneBinder` | `ScriptableObject`, `EEnemyType -> Enemy prefab` |
| `EnemyPrefabEntry` | 적 타입/프리팹 매핑 | `enemyType`, `prefab` | `EnemyDatabase` | 직렬화 데이터 |
| `WeaponDatabase` | 무기 프리팹 DB | `WeaponPrefabEntry[]` | `GameManager`, `ShipController.InitWeapons` | `ScriptableObject`, `EWeaponType -> Weapon prefab` |
| `WeaponPrefabEntry` | 무기 타입/프리팹 매핑 | `weaponType`, `prefab` | `WeaponDatabase` | 직렬화 데이터 |
| `WeaponTransformData` | 무기 배치 데이터 | `WeaponOffset`, `WeaponRotation`, `isFlipX`, `isFlipY` | `ShipController.InitWeapons` | 함선별 무기 배치 값 |
| `Room` | 방 데이터/런타임 상태 | `Id`, `Type`, `LocalPosition`, `GlobalPosition`, `Crews`, `FacilitySystem`, `neighborRoomIds`, `IsHit` | `ShipData`, `ShipController`, `CrewController`, `CombatSystem`, `RoomViewer` | 데이터지만 런타임 참조 포함 |
| `FacilityState` | 시설 저장 상태 | `Type`, `TotalDamage`, `MaxPowerLimit`, `Power` | `GameContext`, `FacilitySystem.ApplyState` | 순수 상태 스냅샷 |
| `CrewState` | 승무원 저장 상태 | `Hp`, `MaxHp`, `Name`, `LastRoomId` | `GameContext`, `PlayerController.SetCrewsState` | 순수 상태 스냅샷 |
| `ProjectileTargetData` | 투사체 타겟 값 | `TargetPosition`, `RoomId` | `WeaponController`, `WeaponSystem`, `ProjectileController`, `EnvironmentController` | `struct` |
| `PowerData` | 적 초기 전력 값 | `ShieldPower`, `WeaponPower`, `EnginePower` | `EnemyPowerController` | 직렬화 데이터 |
| Enums | 상태/타입 구분 | `EEnemyType`, `EWeaponType`, `ERoomType`, `EShipSide`, `EBattleResult`, `EExplosionType`, `EFacilityStatus`, `EPowerBarStatus`, `ECrewStatus` | 전투/시설/UI 전반 | UML에서는 enum으로 표시 |

### 2.5 기타 클래스

| 클래스 | 역할 | 사용 위치 | 비고 |
|---|---|---|---|
| `RandomTargeter` | 랜덤 방 타겟 제공 | `EnemyController`, `WeaponController` | `ITargeting` 구현 |
| `RoomGraph` | 방 그래프 예정 구조 | 직접 사용 확인 안 됨 | 확인 필요 |
| `SingletonBase<T>` | 싱글톤 기반 | `GameManager`, `CombatSystem`, `CursorController` | `DontDestroyOnLoad` 처리 |
| `SimpleInput` | Input System 래퍼 | `GameManager`, `PlayerController`, `CursorController` | static utility |
| `Tags` | 태그/레이어 문자열 상수 | 클릭/충돌/함선 판별 | static utility |
| `LayerMaskExtensions` | LayerMask 포함 여부 등 | `PlayerController`, `ProjectileController` | static utility |
| `TransformExtensions`, `CharacterController2D`, `ComponentPool<T>`, `SpriteFlipper` | 범용 유틸/샘플성 | 핵심 FTL 흐름 직접 연결은 제한적 | 확인 필요 |
| `EBattleResultExtensions`, `EDamageTypeExtensions`, `ERoomTypeExtensions` | enum 표시/계산 확장 | UI, 피해 표시, 시설명 표시 | static utility |
| `zTest/Test`, `TestA`, `TestB` | 테스트/학습용 클래스 | 실제 씬/프리팹 연결 확인 안 됨 | 확인 필요 |

## 3. UML 다이어그램 작성용 관계 요약

### 상속 관계

- `GameManager -> SingletonBase<GameManager>`
- `CombatSystem -> SingletonBase<CombatSystem>`
- `CursorController -> SingletonBase<CursorController>`
- `PlayerController -> ShipController`
- `EnemyController -> ShipController`
- `WeaponSystem -> FacilitySystem`
- `ShieldSystem -> FacilitySystem`
- `EngineSystem -> FacilitySystem`
- `EnemyPowerController -> ShipPowerController`
- `FacilityPowerViewer -> PowerViewer`

### 인터페이스 구현 관계

- `ShipController : IDamageable, IDeadable`
- `RandomTargeter : ITargeting`

### 포함/소유 관계

- `GameManager` owns `GameContext`, `EnemyDatabase`, `WeaponDatabase`
- `GameContext` owns `List<Room>`, `List<FacilityState>`, `List<CrewState>`, `List<EWeaponType>`, `EEnemyType`
- `EnemyDatabase` owns `EnemyPrefabEntry[]`
- `WeaponDatabase` owns `WeaponPrefabEntry[]`
- `ShipController` owns runtime `Rooms`, `Crews`, `FacilitySystems`, `WeaponTypes`, `WeaponTransformData`
- `ShipController` instantiates `Room`, `CrewController`, `WeaponController`, `CrewViewer`, `WeaponViewer` related prefab instances
- `WeaponSystem` owns `List<WeaponController>` and target sprite objects
- `CrewSystem` owns `List<CrewController>` and selected crew list
- `CombatSystem` owns/caches player/enemy `ShipController`, ship colliders, `SpriteShakeController`, floating text pool, explosion pool

### 주요 참조 관계

- `GameManager -> SceneBinder`, `ShopManager`, `MainUIController`, `BackgroundController`, `CursorController`
- `SceneBinder -> EnemyDatabase -> Enemy prefab -> EnemyController`
- `SceneBinder -> EnvironmentController`
- `PlayerController -> CombatSystem`, `WeaponSystem`, `CrewSystem`
- `EnemyController -> RandomTargeter -> CombatSystem.GetPlayerRooms()`
- `ShipController -> WeaponDatabase -> Weapon prefab -> WeaponController`
- `WeaponController -> ProjectileController`
- `ProjectileController -> CombatSystem`
- `CombatSystem -> ShieldSystem`, `ShipController`, `FloatingTextController`, `ExplosionController`, `SpriteShakeController`
- `EnvironmentController -> CombatSystem`, `ShipController`, `ProjectileController`, `SolarFlareController`
- `Room -> FacilitySystem`, `CrewController`
- Viewers mostly reference matching System/Controller or fallback to `GameManager/GameContext`

### 호출 흐름상 중요한 관계

- 씬 로드:
  - `SceneManager.sceneLoaded -> GameManager.OnSceneLoaded -> SceneBinder.BindSceneObjects`
  - `SceneBinder.SpawnEnemyOrFindSceneEnemy -> EnemyDatabase.GetPrefab -> Instantiate enemy -> EnemyController`
  - `SceneBinder.BindCombatSystem -> CombatSystem.InitCombatSystem`
  - `SceneBinder.BindStageEnvironment -> EnvironmentController.InitStageEnvironment`
- 플레이어 전투 입력:
  - `PlayerController.HandleMouse -> WeaponSystem.SetSelectedRoomTarget`
  - `PlayerController.HandleMouse -> CrewSystem.AddSelectedCrew/SetTargetRoom`
  - `PlayerController.TestHandleFacilityPower -> ShipController.HandlePowerInput -> ShipPowerController.TryAllocatePower/TryDeallocatePower`
- 무기/투사체:
  - `ShipController.InitWeapons -> WeaponDatabase.GetPrefab -> Instantiate WeaponController`
  - `WeaponSystem.SetSelectedWeapon -> CursorController.SetCursorAim`
  - `WeaponController.Update -> FireCoroutine -> Instantiate ProjectileController -> ProjectileController.SetTarget`
  - `ProjectileController.OnTriggerExit2D -> CombatSystem.MoveToEnemyArea`
  - `ProjectileController.OnTriggerEnter2D -> CombatSystem.HandleShieldCollision`
  - `ProjectileController.Update -> CombatSystem.HandleRoomCollision`
- 피해/결과:
  - `CombatSystem.HandleRoomCollision -> ShipController.TakeDamageToRoom`
  - `ShipController.TakeDamageToRoom -> Room.TakeDamageToCrews/TakeDamageToFacility -> CheckGameOver`
  - `ShipController.CheckGameOver -> GameManager.StopBattle -> DeadCoroutine -> GameManager.HandleBattleResult`
  - `GameManager.HandleBattleResult -> MainUIController.SetBattleResult -> BattleResultViewer`
  - `BattleResultViewer.DisableBattleResultViewer -> GameManager.MoveSceneByBattleResult`
- 환경:
  - `EnvironmentController.EnvironmentCoroutine -> ApplyEnvironmentEffect`
  - 소행성: `EnvironmentController.SpawnAsteroidAttack -> ProjectileController -> CombatSystem.MoveToEnemyArea`
  - 태양: `SolarFlareController.ApplyFlareEffect -> ShipController.TakeDamageToRoom -> CombatSystem.SetFloatingText`

## 4. 프리팹 정리

| 프리팹 | 역할 | 연결 스크립트 | 사용 씬/시스템 | 재사용성 | UML 표시 포인트/비고 |
|---|---|---|---|---|---|
| `GameManager.prefab` | 전역 게임 매니저 | `GameManager` | `_MainMenu`, `BattlePrepare`, `BattleScene*`, `GameOver` | 전역 재사용 | `EnemyDatabase`, `WeaponDatabase`, `GameContext`의 루트 |
| `CustomMouse.prefab` | 커스텀 커서 | `CursorController` | 주요 씬 | 전역 재사용 | `SingletonBase<CursorController>` |
| `Main Camera.prefab` | 카메라 | `AspectRatioController` | 주요 씬 | 재사용 | 카메라 비율 제어 |
| `Crew.prefab` | 승무원 런타임 오브젝트 | `CrewController` | `ShipController.InitCrews`, 전투 씬 | 재사용 | `CrewSystem` 소유 대상 |
| `Room.prefab` | 방 표시/피격 연출 | `RoomViewer`, `SpriteShakeController` | `ShipController.InitRooms`, 전투 씬 | 재사용 | `Room` 데이터 시각화 |
| `Shield.prefab` | 실드 시각 오브젝트 | 직접 프로젝트 스크립트 없음 | 적/전투 씬 | 확인 필요 | `ShieldViewer`와 연결 여부는 Inspector 확인 필요 |
| `EnemySpawnPoint.prefab` | 적 생성 위치 | 직접 프로젝트 스크립트 없음 | `SceneBinder` enemy spawn | 씬 배치용 | 위치 기준 오브젝트 |
| `Enemy/Enemy1.prefab` | 적 함선 1 | `EnemyController`, `CrewSystem`, `EnemyPowerController`, `ReactorSystem`, `WeaponSystem`, `EngineSystem`, `ShieldSystem`, `ShipOwnership`, `ShipViewer`, `SpriteShakeController` | `EnemyDatabase`, `BattleScene*` | 재사용 | `EnemyShip1Data`, 무기/방/승무원 prefab 의존 |
| `Enemy/Enemy2.prefab` | 적 함선 2 | Enemy1과 동일 계열 | `EnemyDatabase`, `BattleScene*` | 재사용 | `EnemyShip2Data` 기반 |
| `Enemy/Enemy3.prefab` | 적 함선 3 | Enemy1과 동일 계열 | `EnemyDatabase`, `BattleScene*` | 재사용 | `EnemyShip3Data` 기반 |
| `Weapon/Basic Laser 1.prefab` | 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController.InitWeapons` | 재사용 | `Red Laser Projectile` 참조 |
| `Weapon/Burst Laser 1.prefab` | 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController.InitWeapons` | 재사용 | `Red Laser Projectile` 참조 |
| `Weapon/Burst Laser 2.prefab` | 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController.InitWeapons`, 적 prefab | 재사용 | `Purple Laser Projectile` 참조 |
| `Weapon/Burst Laser 3.prefab` | 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController.InitWeapons` | 재사용 | `Purple Laser Projectile` 참조 |
| `Weapon/Missile 1.prefab` | 미사일 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController.InitWeapons` | 재사용 | `Basic Missile Projectile` 참조 |
| `Projectile/Red Laser Projectile.prefab` | 레이저 투사체 | `ProjectileController` | Basic/Burst Laser 계열 | 재사용 | `CombatSystem` 충돌 처리 대상 |
| `Projectile/Purple Laser Projectile.prefab` | 레이저 투사체 | `ProjectileController` | Burst Laser 2/3 | 재사용 | `CombatSystem` 충돌 처리 대상 |
| `Projectile/Green Laser Projectile.prefab` | 레이저 투사체 | `ProjectileController` | 사용 위치 확인 필요 | 재사용 가능 | DB 직접 매핑은 확인 안 됨 |
| `Projectile/Basic Missile Projectile.prefab` | 미사일 투사체 | `ProjectileController` | `Missile 1` | 재사용 | `CombatSystem` 충돌 처리 대상 |
| `Projectile/Asteroid Projectile.prefab` | 환경 소행성 투사체 | `ProjectileController` | `EnvironmentController`, `BattleScene*` | 환경 전용 가능성 | 환경 이벤트가 직접 생성 |
| `Explosion/Red Explosion.prefab` | 폭발 효과 | `ExplosionController` | `CombatSystem` explosion pool, 전투 씬 | 재사용 | 현재 전투 씬에서 주로 참조 |
| `Explosion/Blue/Dark Red/Purple Explosion.prefab` | 폭발 효과 변형 | `ExplosionController` | 테스트 씬 | 재사용 가능 | 실제 전투 사용 여부 확인 필요 |
| `Floating Text.prefab` | 피해 숫자 표시 | `FloatingTextController` | `CombatSystem` floating text pool | 재사용 | 피해 타입별 색상/숫자 표시 |
| `Solar Flare.prefab` | 태양 플레어 연출 | `SolarFlareController` | `EnvironmentController`, 테스트 씬 | 환경 재사용 | 태양 스테이지용 |
| `_UI/Main UI.prefab` | 공통 메뉴 UI | `MainUIController` | 주요 씬 | 재사용 | 씬 이동/일시정지/결과 UI 연결 |
| `_UI/Battle Result UI.prefab` | 전투 결과 UI | `BattleResultViewer` | `BattleScene*`, 테스트 씬 | 재사용 | `GameManager.MoveSceneByBattleResult`로 이어짐 |
| `_UI/Crew/Crew UI.prefab` | 승무원 UI 그룹 | 내부 `Crew Summary` | `BattleScene*` | 재사용 | 전투 승무원 요약 묶음 |
| `_UI/Crew/Crew Summary.prefab` | 승무원 요약 UI | `CrewViewer` | `ShipController.InitCrews`, 상점/전투 UI | 재사용 | 전투/회복 UI 겸용 |
| `_UI/Crew/Crew Heal Group.prefab` | 승무원 회복 UI 묶음 | 직접 프로젝트 스크립트 없음, 내부 `Crew Summary` | 상점 UI 계열 | 특정 UI 가능성 | 확인 필요 |
| `_UI/Facility/Facilities Canvas.prefab` | 시설 UI 묶음 | 내부 `Facility UI` | `BattleScene*` | 재사용 | 시설 UI 그룹 |
| `_UI/Facility/Facility UI.prefab` | 시설 전력/상태 UI | `FacilityPowerViewer` | 전투 시설 UI | 재사용 | `FacilitySystem` 참조 |
| `_UI/Facility/Facility Upgrade UI.prefab` | 시설 업그레이드 UI | `FacilityPowerViewer` | `BattlePrepare` | 상점 전용 가능성 | `GameContext` 기반 표시 가능 |
| `_UI/Reactor Canvas.prefab` | 반응로 UI | `ReactorViewer` | `BattleScene*`, `BattlePrepare` | 재사용 | `ReactorSystem` 또는 `GameContext` 참조 |
| `_UI/Scrap UI.prefab` | 스크랩 UI | `ScrapViewer` | `BattleScene*`, `BattlePrepare` | 재사용 | `GameManager.GetScrap` |
| `_UI/Shield UI.prefab` | 실드 UI | `ShieldViewer` | `BattleScene*` | 재사용 | `ShieldSystem` 참조 |
| `_UI/Ship Hp/Hp Canvas.prefab` | 함선 HP UI | `HpViewer` | `BattleScene*`, `BattlePrepare` | 재사용 | `ShipController` 또는 `GameContext` 참조 |
| `_UI/Ship Hp/Hp Bar.prefab` | HP 바 조각 | 직접 프로젝트 스크립트 없음 | `HpViewer`가 생성/사용 | UI 부품 | 반복 UI 단위 |
| `_UI/Power Bar/Power Bar.prefab` | 전력바 조각 | 직접 프로젝트 스크립트 없음 | `PowerViewer`, `ReactorViewer`, `FacilityPowerViewer` | UI 부품 | 반복 UI 단위 |
| `_UI/Power Bar/Power Bar Group.prefab` | 전력바 그룹 | 직접 프로젝트 스크립트 없음 | 여러 UI prefab 내부 | UI 부품 | 그룹 컨테이너 |
| `_UI/Weapon Charge UI.prefab` | 무기 충전 UI 그룹 | 내부 `Weapon Charge Status` | `BattleScene*` | 재사용 | 무기 충전 UI 묶음 |
| `_UI/Weapon Charge Status.prefab` | 개별 무기 충전 상태 UI | 직접 프로젝트 스크립트 없음 | `ShipController.InitWeapons` | UI 부품 | `WeaponViewer.SetChargeBar`로 연결됨 |
| `_UI/Ship Debug Buttons.prefab` | 디버그 버튼 UI | 직접 프로젝트 스크립트 확인 안 됨 | `BattleScene*` | 디버그/씬 전용 | 버튼 이벤트 Inspector 확인 필요 |

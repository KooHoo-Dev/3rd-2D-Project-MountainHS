# 프로젝트 구조 분석

기준 브랜치: `main`

작성 범위: UML 다이어그램 기초 자료 생성을 위한 1~4번 항목 중심 정리.

주의: 이 문서는 현재 레포지토리에 존재하는 스크립트, 프리팹, ScriptableObject 파일을 기준으로 정리했다. Unity Inspector 연결 상태나 런타임 씬 오브젝트 배치는 에디터 실행 없이는 일부 확정할 수 없으므로, 불명확한 항목은 "확인 필요"로 표시했다.

## 1. 전체 구조 요약

- 핵심 흐름
  - `GameManager`는 전역 싱글톤으로 씬 로드 이벤트를 받아 전투 씬, 전투 준비 씬, 결과 처리 흐름을 제어한다.
  - 전투 씬 진입 시 `GameManager`가 `SceneBinder`를 찾아 `GameContext`를 전달하고, `SceneBinder`가 플레이어 함선과 적 함선을 찾아/생성한 뒤 상태를 복원한다.
  - `SceneBinder`는 복원된 `PlayerController`, 생성된 `EnemyController`를 `CombatSystem.InitCombatSystem`에 전달하여 전투 시스템의 양측 함선 참조를 구성한다.
  - `ShipController`는 함선 공통 기반 클래스이며 방(`Room`), 승무원(`CrewController`), 시설(`FacilitySystem`), 무기(`WeaponController`)를 초기화하고 소유한다.
  - `PlayerController`는 입력을 받아 무기 조준, 승무원 선택/이동, 시설 전력 할당을 수행한다.
  - `EnemyController`는 `RandomTargeter`를 통해 플레이어 방 목록을 대상으로 삼고, 적 무기 시스템에 타겟팅 시스템을 주입한다.
  - `CombatSystem`은 투사체의 함선 영역 이동, 보호막/방 충돌 처리, 피격 연출, 폭발/플로팅 텍스트 풀링을 담당한다.
  - 전투 결과는 `ShipController.CheckGameOver` 또는 UI 액션을 통해 `GameManager.HandleBattleResult`로 전달되고, `GameManager`가 결과 UI 표시 및 다음 씬 이동을 담당한다.

- 주요 매니저/시스템 관계
  - `GameManager`
    - 소유/참조: `GameContext`, `EnemyDatabase`, `WeaponDatabase`, `PlayerController`, `MainUIController`, `ShopManager`, `SceneBinder`
    - 호출: `SceneBinder.BindSceneObjectsWithEnvironment`, `CombatSystem.StopCombat`, `MainUIController.SetBattleResult`, `SceneManager.LoadScene`
  - `SceneBinder`
    - 참조: `GameContext`, `PlayerController`, `EnemyController`, `EnemyDatabase`, `StageEnvironmentController`
    - 역할: 씬 오브젝트 바인딩, 적 프리팹 생성, 플레이어 상태 복원, 전투 시스템 연결
  - `CombatSystem`
    - 참조: `PlayerController`, `EnemyController`, 각 함선의 `Collider2D`, `SpriteShakeController`, `ShieldSystem`, `Room`
    - 역할: 전투 중 충돌/피해/연출 중재
  - `ShipController`
    - 소유: `Room`, `CrewController`, `FacilitySystem`, `WeaponController`, `CrewSystem`, `WeaponSystem`, `ShipPowerController`, `ShipOwnership`
    - 역할: 함선 데이터 기반 런타임 구성 및 피해/사망 처리
  - `ShopManager`
    - 참조: `GameManager`, `GameContext`
    - 역할: 전투 준비 씬에서 강화/회복 결과를 `GameContext`에 반영

- 씬 전환 시 유지되는 데이터와 씬 오브젝트 의존성
  - 유지 데이터
    - `GameContext`가 플레이어 함선 HP, 스크랩, 반응로 전력, 시설 상태, 승무원 상태, 무기 타입 목록, 적 타입을 보관한다.
    - `ShopManager.VisitCount`는 선형 전투 씬 진행 단계와 적/환경 선택에 사용된다.
  - 씬 오브젝트 의존성
    - `GameManager`는 `SceneManager.sceneLoaded`에서 씬별 오브젝트를 런타임 검색한다.
    - 전투 씬에는 `SceneBinder`, `PlayerController`, 적 생성 위치/부모, 적 UI 그룹, 환경 컨트롤러가 필요하다.
    - 전투 준비 씬에는 `ShopManager`가 필요하며, `GameManager`가 `ShopManager.ApplyGameContext`를 호출한다.
    - `SceneBinder`는 `GameManager.Instance.EnemyDatabase`에서 적 프리팹을 받아 생성한다.

## 2. 클래스 구조 정리

### 2.1 핵심 매니저/시스템

| 클래스 | 역할 | 주요 참조 | 주요 책임 | UML 표시 포인트 |
|---|---|---|---|---|
| `GameManager` | 전체 게임 흐름과 전역 상태 관리 | `GameContext`, `EnemyDatabase`, `WeaponDatabase`, `PlayerController`, `MainUIController`, `ShopManager`, `SceneBinder` | 씬 로드 처리, 전투 결과 처리, `GameContext` 저장/복원, 전투 정지/재개, 상점/전투 씬 연결 | `SingletonBase<GameManager>` 상속, `SceneManager.sceneLoaded` 이벤트 사용, 여러 시스템으로 향하는 중심 참조 |
| `SceneBinder` | 전투 씬 런타임 의존성 연결 | `GameContext`, `PlayerController`, `EnemyController`, `StageEnvironmentController`, `EnemyDatabase` | 적 생성, 플레이어 상태 복원, 적 UI 그룹 연결, `CombatSystem` 초기화, 환경 활성화 | `MonoBehaviour`, `GameContext -> SceneBinder -> Player/Enemy/CombatSystem` 호출 흐름의 핵심 |
| `CombatSystem` | 전투 충돌/피해/연출 중재 | `PlayerController`, `EnemyController`, `ShipController`, `Room`, `ShieldSystem`, `ProjectileController`, `FloatingTextController`, `ExplosionController` | 투사체 이동 보정, 방/보호막 충돌 처리, 함선별 콜라이더/흔들림 참조 관리, 풀링 | `SingletonBase<CombatSystem>` 상속, `ShipSide` 기반 딕셔너리 참조, 전투 런타임 허브 |
| `ShipController` | 플레이어/적 함선 공통 기반 | `ShipData`, `Room`, `CrewController`, `CrewSystem`, `WeaponSystem`, `ShipPowerController`, `ShipOwnership`, `WeaponDatabase` | 방/승무원/무기 초기화, 시설 등록, 피해/수리/사망 처리, 전투 기능 정지 | `MonoBehaviour`, `IDamageable`, `IDeadable` 구현, `PlayerController`/`EnemyController`의 부모 |
| `FacilitySystem` | 시설 공통 전력/피해 상태 관리 | `ShipController`, `FacilityState`, `RoomType`, `FacilityStatus` | 시설 전력 증가/감소, 피해/수리, 상태 갱신, 승무원 보조 효과 | `MonoBehaviour`, `WeaponSystem`/`EngineSystem`/`ShieldSystem`의 부모 |
| `WeaponSystem` | 무기 시설 및 무기 타겟 관리 | `WeaponController`, `ProjectileTargetData`, `ITargeting`, `CursorController` | 무기 전력 활성화, 충전 속도 반영, 플레이어 조준 대상 적용, 적 타겟팅 시스템 주입 | `FacilitySystem` 상속, `ITargeting` 참조, `WeaponController` 목록 소유 |
| `ShieldSystem` | 보호막 시설 | `FacilitySystem`, `ProjectileController` | 보호막 전력/피해 처리 | `FacilitySystem` 상속, 세부 호출 관계는 확인 필요 |
| `EngineSystem` | 엔진 시설 | `FacilitySystem` | 엔진 전력/상태 관리 | `FacilitySystem` 상속 |
| `ReactorSystem` | 반응로 전력 저장소 | `ShipPowerController` | 최대 전력/현재 전력 증가, 감소, 설정 | `MonoBehaviour`, `ShipPowerController`가 합성/참조 |
| `ShipPowerController` | 함선 시설 전력 분배 | `ReactorSystem`, `FacilitySystem`, `RoomType`, `FacilityState` | 반응로 전력 할당/회수, 시설 전력 오버플로 처리, 저장 상태 적용 | `MonoBehaviour`, `EnemyPowerController`의 부모 |
| `EnemyPowerController` | 적 함선 전력 제어 | `ShipPowerController` | 적 전력 분배 특화 처리 | `ShipPowerController` 상속 |
| `CrewSystem` | 승무원 선택/이동/사망 관리 | `ShipController`, `CrewController`, `Room`, `FacilitySystem` | 선택 승무원 관리, 이동 목표 지정, 승무원 사망 반영 | `MonoBehaviour`, `ShipController`와 같은 오브젝트 구성 요소 |
| `ShopManager` | 전투 준비 씬 강화/회복 관리 | `GameManager`, `GameContext`, `CrewViewer`, Unity UI `Button` | 시설/반응로 강화, 승무원 회복, 함선 수리, 방문 횟수 관리 | `MonoBehaviour`, static `VisitCount`, `GameContext` 변경 호출 |
| `CursorController` | 마우스 커서 상태 관리 | `WeaponSystem`, `PlayerController` | 기본/조준 커서 전환 | `SingletonBase<CursorController>` 상속 |
| `MainUIController` | 메인 UI/결과 UI/씬 버튼 제어 | `GameManager`, `BattleResultViewer`, `SceneManager` | 씬 이동, 게임 종료, 도주 처리, 전투 결과 UI 표시 | `MonoBehaviour`, UI 버튼 이벤트의 진입점 |
| `StageEnvironmentController` | 전투 환경 활성화 관리 | `SceneBinder`, `AsteroidSpawner`, `SolarFlareController` | 소행성/태양 플레어 스테이지 환경 구성 | `MonoBehaviour`, 실제 연결 스크립트/필드 확인 필요 |
| `BackgroundController` | 배경 랜덤 선택 | `GameManager` | 전투 씬 배경 선택/변경 | `MonoBehaviour`, 상세 연결 확인 필요 |

### 2.2 Controller 계열

| 클래스 | 역할 | 주요 참조 | 주요 책임 | UML 표시 포인트 |
|---|---|---|---|---|
| `PlayerController` | 플레이어 함선 제어 | `GameManager`, `CombatSystem`, `CrewSystem`, `WeaponSystem`, `ShipPowerController`, `Room`, `CrewController` | 입력 처리, 무기 선택/조준, 승무원 선택/이동, 시설 전력 할당, 저장 상태 복원 | `ShipController` 상속, 플레이어 입력에서 시스템 호출로 이어지는 핵심 클래스 |
| `EnemyController` | 적 함선 제어 | `CombatSystem`, `RandomTargeter`, `WeaponSystem` | 적 타겟팅 시스템 생성, 적 무기에 타겟팅 주입, 적 소유권 설정 | `ShipController` 상속, `ITargeting` 구현체를 무기 시스템에 연결 |
| `WeaponController` | 단일 무기 동작 | `WeaponSystem`, `ITargeting`, `ProjectileController`, `WeaponViewer`, `ProjectileTargetData` | 충전, 준비 상태 관리, 플레이어/적 발사 처리, 투사체 생성 | `MonoBehaviour`, `WeaponSystem`이 여러 개 소유 |
| `ProjectileController` | 투사체 이동/충돌 | `CombatSystem`, `Room`, `ShieldSystem` | 목표 지점 이동, 시작/도착 콜라이더 판정, 보호막/방 충돌 전달, 폭발 요청 | `MonoBehaviour`, 충돌 이벤트에서 `CombatSystem` 호출 |
| `CrewController` | 단일 승무원 동작 | `CrewSystem`, `Room`, `CrewViewer` | 방 배치/이동, 선택 상태, HP/사망 처리, 시설 수리/보조와 연결 | `MonoBehaviour`, 세부 필드/메서드 관계 확인 필요 |
| `AsteroidSpawner` | 소행성 생성 | `ProjectileController`, `CombatSystem` | 환경 투사체 생성 | `MonoBehaviour`, `StageEnvironmentController`에서 사용 추정, 확인 필요 |
| `SolarFlareController` | 태양 플레어 이벤트 | `ShipController`, `FacilitySystem` | 태양 플레어 환경 효과 처리 | `MonoBehaviour`, 상세 호출 관계 확인 필요 |
| `ExplosionController` | 폭발 연출 | `CombatSystem` | 풀에서 꺼낸 폭발 오브젝트 재생/비활성화 | `MonoBehaviour`, `CombatSystem.GetExplosionObject`와 연결 |
| `FloatingTextController` | 피해량 텍스트 연출 | `CombatSystem` | 피해 텍스트 표시 후 비활성화 | `MonoBehaviour`, `CombatSystem.SetFloatingText`와 연결 |
| `AspectRatioController` | 카메라 화면 비율 처리 | `Camera` | 화면 비율 보정 | `MonoBehaviour`, `Main Camera.prefab`에 연결 |

### 2.3 Viewer/UI 계열

| 클래스 | 역할 | 주요 참조 | 주요 책임 | UML 표시 포인트 |
|---|---|---|---|---|
| `PowerViewer` | 전력 UI 공통 표시 | `PowerBarStatus` | 전력 바 상태 표시 | `MonoBehaviour`, `FacilityPowerViewer`의 부모 |
| `FacilityPowerViewer` | 시설 전력 UI 입력/표시 | `PowerViewer`, `ShipPowerController`, `FacilitySystem`, `RoomType`, `IPointerClickHandler` | 시설 전력 클릭 처리, 전력 상태 표시 | `PowerViewer` 상속, `IPointerClickHandler` 구현 |
| `ReactorViewer` | 반응로 전력 UI | `ReactorSystem` 또는 `ShipPowerController` | 반응로 전력 표시 | `MonoBehaviour`, 상세 참조 확인 필요 |
| `ShieldViewer` | 보호막 UI | `ShieldSystem` | 보호막 상태 표시 | `MonoBehaviour`, 상세 참조 확인 필요 |
| `HpViewer` | 함선 HP UI | `ShipController` | 함선 체력 표시 | `MonoBehaviour`, 플레이어/적 함선 UI 연결 |
| `ScrapViewer` | 스크랩 UI | `PlayerController` 또는 `GameManager` | 보유 스크랩 표시 | `MonoBehaviour`, 상세 참조 확인 필요 |
| `RoomViewer` | 방 스프라이트/상태 표시 | `Room`, `FacilitySystem` | 방 위치/상태/피격 표시 | `MonoBehaviour`, `Room.prefab`에 연결 |
| `WeaponViewer` | 무기 충전 UI | `WeaponController`, UI Slider/Image | 무기 충전 바 및 상태 표시 | `MonoBehaviour`, 무기 프리팹에 연결 |
| `CrewViewer` | 승무원 UI/요약 표시 | `CrewController`, Unity UI `Button` | 승무원 HP/선택/회복 UI 표시 | `MonoBehaviour`, `Crew Summary.prefab`과 승무원 UI에서 사용 |
| `BattleResultViewer` | 전투 결과 UI | `BattleResult`, `GameManager` | 승패/도주 결과 및 보상 표시 | `MonoBehaviour`, `MainUIController`가 활성화 |
| `ShipViewer` | 함선 시각 표시 | `ShipController` | 함선 표시 갱신 | `MonoBehaviour`, 적 프리팹에 연결 확인 |
| `SpriteShakeController` | 피격 흔들림 연출 | `CombatSystem`, `ShipController` | 피격 시 스프라이트 흔들림 | `MonoBehaviour`, 함선/방 프리팹에 연결 |

### 2.4 데이터/상태 클래스

| 클래스 | 역할 | 주요 필드 | 사용 위치 | UML 표시 포인트 |
|---|---|---|---|---|
| `GameContext` | 씬 전환 간 플레이어 상태 저장 | `Rooms`, `FacilityStates`, `CrewStates`, `FacilityUpgradeCount`, `ScrapAmount`, `ShipHp`, `ShipMaxHp`, `ShipPower`, `ShipReactorCapacity`, `ReactorUpgradeCount`, `WeaponTypes`, `EnemyType` | `GameManager`, `SceneBinder`, `ShopManager` | 순수 직렬화 데이터 클래스, 전역 런타임 상태 저장소 |
| `Room` | 함선 방 데이터/런타임 상태 | `Id`, `Type`, `LocalPosition`, `GlobalPosition`, `MaxCrewCapacity`, `neighborRoomIds`, `FacilitySystem`, `Crews`, `IsHit` | `ShipController`, `CombatSystem`, `CrewSystem`, `ProjectileController` | `[System.Serializable]` 데이터 클래스, `MonoBehaviour` 아님 |
| `ShipData` | 함선 기본 데이터 ScriptableObject | `MaxHp`, `Rooms` | `ShipController`, `PlayerShipData.asset`, `EnemyShip*Data.asset` | `ScriptableObject`, 방 배치 데이터 소스 |
| `EnemyDatabase` | 적 타입별 프리팹 데이터베이스 | `EnemyPrefabEntry(enemyType, prefab)` | `GameManager`, `SceneBinder` | `ScriptableObject`, `EnemyType -> GameObject` 매핑 |
| `WeaponDatabase` | 무기 타입별 프리팹 데이터베이스 | `WeaponPrefabEntry(weaponType, prefab)` | `ShipController`, `GameManager` | `ScriptableObject`, `WeaponType -> GameObject` 매핑 |
| `FacilityState` | 시설 저장 상태 | `Type`, `Power`, `MaxPowerLimit`, `TotalDamage` | `GameContext`, `FacilitySystem`, `ShipPowerController`, `PlayerController` | 순수 데이터 클래스, 시설 복원에 사용 |
| `CrewState` | 승무원 저장 상태 | 승무원 HP/방 위치 관련 필드 | `GameContext`, `PlayerController`, `ShipController` | 순수 데이터 클래스, 상세 필드는 확인 필요 |
| `ProjectileTargetData` | 투사체 목표 데이터 | `TargetPosition`, `RoomId` | `WeaponSystem`, `WeaponController`, `ProjectileController` | 순수 데이터 클래스/struct 성격, 조준 정보 전달 |
| `WeaponTransformData` | 무기 배치 데이터 | `WeaponOffset`, `WeaponRotation`, `isFlipX`, `isFlipY` | `ShipController` | `[System.Serializable]`, 무기 프리팹 위치/회전 적용 |
| `ShipOwnership` | 함선 소유 진영 정보 | `ShipSide` | `ShipController`, `CombatSystem`, `PlayerController`, `EnemyController` | `MonoBehaviour`, `ShipSide` 기반 전투 매핑에 사용 |
| `ShipSide` | 함선 진영 enum | `Player`, `Enemy0` 등 | `ShipOwnership`, `CombatSystem` | UML enum |
| `BattleResult` | 전투 결과 enum | 플레이어/적 파괴, 도주 등 | `GameManager`, `MainUIController`, `BattleResultViewer`, `ShipController` | UML enum |
| `EnemyType` | 적 종류 enum | `Enemy1`, `Enemy2`, `Enemy3` | `GameManager`, `EnemyDatabase`, `SceneBinder`, `GameContext` | UML enum |
| `WeaponType` | 무기 종류 enum | `BasicLaser1`, `BurstLaser1`, `BurstLaser2`, `BurstLaser3`, `Missile1` | `GameContext`, `WeaponDatabase`, `ShipController` | UML enum |
| `RoomType` | 방/시설 종류 enum | `Weapon`, `Engine`, `Shield` 등 | `Room`, `FacilitySystem`, `ShipPowerController`, `ShopManager` | UML enum |
| `FacilityStatus` | 시설 상태 enum | `Normal`, `Damaged`, `Broked`, `Disabled` | `FacilitySystem`, `Room` | UML enum |
| `PowerBarStatus` | 전력 UI 상태 enum | 확인 필요 | `PowerViewer`, `FacilityPowerViewer` | UML enum |

### 2.5 기타 클래스

| 클래스 | 역할 | 사용 위치 | 비고 |
|---|---|---|---|
| `SingletonBase<T>` | 싱글톤 기반 클래스 | `GameManager`, `CombatSystem`, `CursorController` | 전역 접근 패턴의 기반 |
| `IDamageable` | 피해 처리 인터페이스 | `ShipController` | 구현 클래스: `ShipController` |
| `IDeadable` | 사망 처리 인터페이스 | `ShipController` | 구현 클래스: `ShipController` |
| `ITargeting` | 타겟 선택 인터페이스 | `WeaponSystem`, `WeaponController`, `RandomTargeter` | 적 무기 타겟팅 추상화 |
| `RandomTargeter` | 랜덤 방 타겟팅 구현 | `EnemyController`, `WeaponController` | `ITargeting` 구현, 플레이어 방 목록 참조 |
| `ShipDebugController` | 함선 디버그 UI/버튼 | 디버그 프리팹 또는 씬 | 확인 필요 |
| `TutorialController` | 튜토리얼 UI 제어 | `Tutorial Canvas.prefab` | 확인 필요 |
| `BackgroundController` | 배경 관리 | 전투 씬 | `GameManager`가 랜덤 배경 호출 |

## 3. UML 다이어그램 작성용 관계 요약

- 상속 관계
  - `GameManager -> SingletonBase<GameManager>`
  - `CombatSystem -> SingletonBase<CombatSystem>`
  - `CursorController -> SingletonBase<CursorController>`
  - `PlayerController -> ShipController -> MonoBehaviour`
  - `EnemyController -> ShipController -> MonoBehaviour`
  - `WeaponSystem -> FacilitySystem -> MonoBehaviour`
  - `ShieldSystem -> FacilitySystem -> MonoBehaviour`
  - `EngineSystem -> FacilitySystem -> MonoBehaviour`
  - `EnemyPowerController -> ShipPowerController -> MonoBehaviour`
  - `FacilityPowerViewer -> PowerViewer -> MonoBehaviour`

- 인터페이스 구현 관계
  - `ShipController` implements `IDamageable`
  - `ShipController` implements `IDeadable`
  - `FacilityPowerViewer` implements `IPointerClickHandler`
  - `RandomTargeter` implements `ITargeting`

- 포함/소유 관계
  - `GameManager` owns `GameContext`
  - `GameContext` contains `List<Room>`, `List<FacilityState>`, `List<CrewState>`, `List<WeaponType>`
  - `ShipController` owns `Rooms`, `Crews`, `FacilitySystems`
  - `ShipController` composes/uses `CrewSystem`, `WeaponSystem`, `ShipPowerController`, `ShipOwnership`
  - `ShipController` instantiates room, crew, weapon, weapon-charge UI prefabs during initialization
  - `WeaponSystem` owns `List<WeaponController>`
  - `CrewSystem` owns selected crew list and references all crew on the ship
  - `ShipPowerController` owns/reference-list `FacilitySystem` objects and one `ReactorSystem`
  - `CombatSystem` keeps runtime maps for `ShipSide -> Collider2D`, `ShipSide -> SpriteShakeController`, `Collider2D -> ShipSide`
  - `EnemyDatabase` contains `EnemyPrefabEntry`
  - `WeaponDatabase` contains `WeaponPrefabEntry`
  - `ShipData` contains room layout data

- 참조 관계
  - `GameManager -> SceneBinder`: 씬 로드 후 바인딩 호출
  - `GameManager -> ShopManager`: 전투 준비 씬에서 상태 적용
  - `GameManager -> MainUIController`: 전투 결과 UI 표시
  - `SceneBinder -> GameManager.EnemyDatabase`: 적 프리팹 조회
  - `SceneBinder -> PlayerController/EnemyController`: 상태 복원 및 전투 시스템 연결
  - `SceneBinder -> StageEnvironmentController`: 소행성/태양 플레어 환경 설정
  - `CombatSystem -> ShipController`: 피해 대상 함선 조회
  - `CombatSystem -> ProjectileController`: 투사체 충돌 처리
  - `PlayerController -> CombatSystem`: 클릭 위치에서 방/승무원 조회
  - `PlayerController -> CrewSystem`: 승무원 선택/이동 요청
  - `PlayerController -> WeaponSystem`: 무기 선택/방 타겟 지정
  - `EnemyController -> RandomTargeter -> Room`: 적 무기의 목표 방 선택
  - `WeaponController -> ProjectileController`: 발사 시 투사체 생성 및 목표 전달
  - `WeaponController -> ITargeting`: 적 무기 자동 목표 선택
  - `ProjectileController -> CombatSystem`: 보호막/방 충돌 처리 요청
  - `ShopManager -> GameManager -> GameContext`: 강화/회복 데이터 변경
  - `MainUIController -> GameManager`: 씬 이동, 도주, 초기화 요청

- 호출 흐름상 중요한 관계
  - 전투 씬 진입
    - `SceneManager.sceneLoaded`
    - `GameManager.OnSceneLoaded`
    - `SceneBinder.BindSceneObjectsWithEnvironment`
    - `SceneBinder.SpawnEnemyOrFindSceneEnemy`
    - `PlayerController.SetFacilitySystems / SetCrewsState / SetWeaponTypes`
    - `CombatSystem.InitCombatSystem`
  - 플레이어 공격
    - `PlayerController.Update`
    - `WeaponSystem.SetSelectedWeapon`
    - `CombatSystem.GetRoomOrNull`
    - `WeaponSystem.SetSelectedRoomTarget`
    - `WeaponController.FireCoroutine`
    - `ProjectileController`
    - `CombatSystem.HandleShieldCollision` 또는 `CombatSystem.HandleRoomCollision`
  - 적 공격
    - `EnemyController.Start`
    - `RandomTargeter`
    - `WeaponSystem.SetWeaponsTargetingSystem`
    - `WeaponController.UpdateEnemyFire`
    - `ProjectileController`
    - `CombatSystem`
  - 피해/전투 종료
    - `CombatSystem.HandleRoomCollision`
    - `ShipController.TakeDamageToRoom`
    - `ShipController.CheckGameOver`
    - `GameManager.HandleBattleResult`
    - `MainUIController.SetBattleResult`
  - 전투 준비/상점
    - `ShopManager.Awake`
    - `GameManager.EnsureBattlePrepareTestGameContext`
    - `ShopManager.ApplyGameContext`
    - `ShopManager.IncreaseMaxPowerLimit / IncreaseReactorMaxPower / HealCrew / Repair`
    - `GameManager`의 `GameContext` 변경 메서드

## 4. 프리팹 정리

| 프리팹 | 역할 | 연결 스크립트 | 사용 씬/시스템 | 재사용성 | 비고 |
|---|---|---|---|---|---|
| `Assets/_Prefabs/GameManager.prefab` | 전역 게임 매니저 | `GameManager` | 전체 게임 흐름 | 재사용 가능/전역 | `EnemyDatabase`, `WeaponDatabase`, `GameContext` 참조 필요 |
| `Assets/_Prefabs/Main Camera.prefab` | 메인 카메라 | `AspectRatioController` | 여러 씬 | 재사용 가능 | 카메라 비율 보정 |
| `Assets/_Prefabs/CustomMouse.prefab` | 커스텀 커서 | `CursorController` | 전투/입력 시스템 | 재사용 가능 | 싱글톤 |
| `Assets/_Prefabs/Room.prefab` | 함선 방 런타임 오브젝트 | `RoomViewer`, `SpriteShakeController` | `ShipController` 초기화 | 재사용 가능 | `ShipData.Rooms` 기반으로 생성 |
| `Assets/_Prefabs/Crew.prefab` | 승무원 오브젝트 | `CrewController` | `ShipController`, `CrewSystem` | 재사용 가능 | 플레이어/적 승무원 생성에 사용 |
| `Assets/_Prefabs/Shield.prefab` | 보호막 오브젝트 | 확인 필요 | `ShieldSystem`/함선 프리팹 | 재사용 가능 | 연결 스크립트 확인 필요 |
| `Assets/_Prefabs/Enemy/Enemy1.prefab` | 적 함선 1 | `EnemyController`, `EnemyPowerController`, `CrewSystem`, `WeaponSystem`, `EngineSystem`, `ShieldSystem`, `ReactorSystem`, `ShipOwnership`, `ShipViewer`, `SpriteShakeController` | `EnemyDatabase`, `SceneBinder` | 재사용 가능 | `EnemyType.Enemy1` |
| `Assets/_Prefabs/Enemy/Enemy2.prefab` | 적 함선 2 | `EnemyController`, `EnemyPowerController`, `CrewSystem`, `WeaponSystem`, `EngineSystem`, `ShieldSystem`, `ReactorSystem`, `ShipOwnership`, `ShipViewer`, `SpriteShakeController` | `EnemyDatabase`, `SceneBinder` | 재사용 가능 | `EnemyType.Enemy2` |
| `Assets/_Prefabs/Enemy/Enemy3.prefab` | 적 함선 3 | `EnemyController`, `EnemyPowerController`, `CrewSystem`, `WeaponSystem`, `EngineSystem`, `ShieldSystem`, `ReactorSystem`, `ShipOwnership`, `ShipViewer`, `SpriteShakeController` | `EnemyDatabase`, `SceneBinder` | 재사용 가능 | `EnemyType.Enemy3` |
| `Assets/_Prefabs/Weapon/Basic Laser 1.prefab` | 기본 레이저 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController` | 재사용 가능 | `WeaponType.BasicLaser1` |
| `Assets/_Prefabs/Weapon/Burst Laser 1.prefab` | 버스트 레이저 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController` | 재사용 가능 | `WeaponType.BurstLaser1` |
| `Assets/_Prefabs/Weapon/Burst Laser 2.prefab` | 버스트 레이저 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController` | 재사용 가능 | `WeaponType.BurstLaser2` |
| `Assets/_Prefabs/Weapon/Burst Laser 3.prefab` | 버스트 레이저 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController` | 재사용 가능 | `WeaponType.BurstLaser3` |
| `Assets/_Prefabs/Weapon/Missile 1.prefab` | 미사일 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController` | 재사용 가능 | `WeaponType.Missile1` |
| `Assets/_Prefabs/Projectile/Red Laser Projectile.prefab` | 레이저 투사체 | `ProjectileController` | `WeaponController` | 재사용 가능 | 무기 프리팹에서 참조 |
| `Assets/_Prefabs/Projectile/Purple Laser Projectile.prefab` | 레이저 투사체 | `ProjectileController` | `WeaponController` | 재사용 가능 | 무기 프리팹에서 참조 |
| `Assets/_Prefabs/Projectile/Green Laser Projectile.prefab` | 레이저 투사체 | `ProjectileController` | `WeaponController` | 재사용 가능 | 무기 프리팹에서 참조 |
| `Assets/_Prefabs/Projectile/Basic Missile Projectile.prefab` | 미사일 투사체 | `ProjectileController` | `WeaponController` | 재사용 가능 | 무기 프리팹에서 참조 |
| `Assets/_Prefabs/Projectile/Asteroid Projectile.prefab` | 소행성 투사체 | `ProjectileController` | `AsteroidSpawner` | 재사용 가능 | 환경 이벤트용 |
| `Assets/_Prefabs/Explosion/Blue Explosion.prefab` | 폭발 연출 | `ExplosionController` | `CombatSystem` | 재사용 가능 | 풀링 대상 |
| `Assets/_Prefabs/Explosion/Dark Red Explosion.prefab` | 폭발 연출 | `ExplosionController` | `CombatSystem` | 재사용 가능 | 풀링 대상 |
| `Assets/_Prefabs/Explosion/Purple Explosion.prefab` | 폭발 연출 | `ExplosionController` | `CombatSystem` | 재사용 가능 | 풀링 대상 |
| `Assets/_Prefabs/Explosion/Red Explosion.prefab` | 폭발 연출 | `ExplosionController` | `CombatSystem` | 재사용 가능 | 풀링 대상 |
| `Assets/_Prefabs/Floating Text.prefab` | 피해량 텍스트 | `FloatingTextController` | `CombatSystem` | 재사용 가능 | 풀링 대상 |
| `Assets/_Prefabs/Solar Flare.prefab` | 태양 플레어 환경 오브젝트 | `SolarFlareController` | `StageEnvironmentController` | 재사용 가능 | 태양 스테이지용 |
| `Assets/_Prefabs/Background Canvas.prefab` | 배경 캔버스 | 확인 필요 | 배경 시스템 | 씬/시스템 전용 가능성 | 확인 필요 |
| `Assets/_Prefabs/EnemySpawnPoint.prefab` | 적 생성 위치 | 확인 필요 | `SceneBinder` | 씬 전용 가능성 | 전투 씬 배치용 |
| `Assets/_Prefabs/_UI/Main UI.prefab` | 메인 전투 UI | `MainUIController` | 전투/메뉴 UI | 재사용 가능 | `BattleResultViewer` 참조 가능 |
| `Assets/_Prefabs/_UI/Battle Result UI.prefab` | 전투 결과 UI | `BattleResultViewer` | `MainUIController` | 재사용 가능 | 전투 결과 표시 |
| `Assets/_Prefabs/_UI/Scrap UI.prefab` | 스크랩 표시 UI | `ScrapViewer` | 전투/상점 UI | 재사용 가능 | 사용 위치 확인 필요 |
| `Assets/_Prefabs/_UI/Reactor Canvas.prefab` | 반응로 UI | `ReactorViewer` | 플레이어 함선 UI | 재사용 가능 | 사용 위치 확인 필요 |
| `Assets/_Prefabs/_UI/Shield UI.prefab` | 보호막 UI | `ShieldViewer` | 함선 UI | 재사용 가능 | 사용 위치 확인 필요 |
| `Assets/_Prefabs/_UI/Ship Hp/Hp Canvas.prefab` | 함선 HP UI | `HpViewer` | 함선 UI | 재사용 가능 | 플레이어/적 사용 가능 |
| `Assets/_Prefabs/_UI/Ship Hp/Hp Bar.prefab` | HP 바 단위 UI | 확인 필요 | `HpViewer` | 재사용 가능 | 확인 필요 |
| `Assets/_Prefabs/_UI/Power Bar/Power Bar.prefab` | 전력 칸 UI | 확인 필요 | `PowerViewer` | 재사용 가능 | 확인 필요 |
| `Assets/_Prefabs/_UI/Power Bar/Power Bar Group.prefab` | 전력 바 그룹 UI | 확인 필요 | `PowerViewer` | 재사용 가능 | 확인 필요 |
| `Assets/_Prefabs/_UI/Facility/Facilities Canvas.prefab` | 시설 UI 캔버스 | 확인 필요 | 플레이어 함선 UI | 재사용 가능 | 확인 필요 |
| `Assets/_Prefabs/_UI/Facility/Facility UI.prefab` | 시설 전력 UI | `FacilityPowerViewer` | `ShipPowerController`/시설 UI | 재사용 가능 | 시설 전력 클릭 UI |
| `Assets/_Prefabs/_UI/Facility/Facility Upgrade UI.prefab` | 시설 업그레이드 UI | `FacilityPowerViewer` | `ShopManager` | 전투 준비 씬 전용 가능성 | 상점 UI 연결 확인 필요 |
| `Assets/_Prefabs/_UI/Crew/Crew Summary.prefab` | 승무원 요약 UI | `CrewViewer` | `ShipController`, `ShopManager` | 재사용 가능 | 전투/상점 모두 사용 |
| `Assets/_Prefabs/_UI/Crew/Crew UI.prefab` | 승무원 UI | 확인 필요 | 승무원 표시 | 재사용 가능 | 확인 필요 |
| `Assets/_Prefabs/_UI/Crew/Crew Heal Group.prefab` | 승무원 회복 UI 그룹 | 확인 필요 | `ShopManager` | 전투 준비 씬 전용 가능성 | 확인 필요 |
| `Assets/_Prefabs/_UI/Weapon Charge UI.prefab` | 무기 충전 UI | 확인 필요 | 무기 UI | 재사용 가능 | 확인 필요 |
| `Assets/_Prefabs/_UI/Weapon Charge Status.prefab` | 무기 충전 상태 UI | `WeaponViewer` 또는 관련 UI | `ShipController.InitWeapons` | 재사용 가능 | 무기 생성 시 함께 생성 |
| `Assets/_Prefabs/_UI/Ship Debug Buttons.prefab` | 디버그 버튼 UI | `ShipDebugController` 추정 | 디버그/테스트 | 확인 필요 | 확인 필요 |
| `Assets/_Prefabs/_UI/Tutorial Canvas.prefab` | 튜토리얼 UI | `TutorialController` 추정 | 튜토리얼 씬/전투 씬 | 확인 필요 | 확인 필요 |


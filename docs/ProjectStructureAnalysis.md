# 프로젝트 구조 분석

기준 브랜치: `main`

작성 범위: UML 다이어그램 작성 기초 자료로 사용할 수 있도록 1~4번 항목, 즉 전체 구조 요약, 클래스 구조, UML 관계 요약, 프리팹 구조만 중점 정리했다.

주의:
- 이 문서는 현재 레포지토리에 실제로 존재하는 `Assets/_Scripts`, `Assets/_Prefabs`, `Assets/_Data` 파일을 기준으로 작성했다.
- Unity Inspector 연결, 씬에 배치된 오브젝트, 외부 패키지 컴포넌트는 에디터 실행 없이 완전히 확정하기 어렵기 때문에 불명확한 항목은 `확인 필요`로 표시했다.

## 1. 전체 구조 요약

- 핵심 흐름
  - `GameManager`는 `SingletonBase<GameManager>` 기반 전역 매니저이며 씬 로드, 전투 결과, 일시정지, `GameContext` 저장/복원, 상점 진입 흐름을 담당한다.
  - 전투 씬 로드 후 `GameManager.OnSceneLoaded`가 `SceneBinder`를 찾고, `GameContext`에 저장된 적 타입과 플레이어 상태를 전달한다.
  - `SceneBinder`는 `GameManager.Instance.EnemyDatabase`에서 적 프리팹을 조회하여 `EnemyController`를 생성하거나 씬 내 적을 찾고, `PlayerController`에 저장 상태를 복원한 뒤 `CombatSystem.InitCombatSystem(player, enemy)`를 호출한다.
  - `ShipController`는 플레이어/적 함선 공통 기반 클래스이며 `ShipData`를 기반으로 `Room`, `CrewController`, `WeaponController`를 생성하고 `FacilityController` 계열 시설을 함선에 연결한다.
  - `PlayerController`는 키보드/마우스 입력을 받아 시설 전력 할당, 무기 선택/조준, 승무원 선택/이동을 수행한다.
  - `EnemyController`는 시작 시 `RandomTargeter`를 생성하고 `WeaponSystem`에 `ITargeting` 구현체를 주입하여 적 무기 자동 타겟팅을 구성한다.
  - `CombatSystem`은 플레이어/적 함선의 콜라이더와 진영 정보를 저장하고, 투사체 이동 보정, 보호막 충돌, 방 충돌, 폭발/피해 텍스트 연출을 중재한다.
  - 전투 결과는 `ShipController.CheckGameOver` 또는 UI의 도주 버튼을 통해 `GameManager.HandleBattleResult`로 모이고, `MainUIController`가 결과 UI 및 씬 이동을 담당한다.

- 주요 매니저/시스템 관계
  - `GameManager`
    - 참조: `GameContext`, `EnemyDatabase`, `WeaponDatabase`, `PlayerController`, `MainUIController`, `ShopManager`, `SceneBinder`, `BackgroundController`
    - 주요 호출: `SceneBinder.BindSceneObjectsWithEnvironment`, `ShopManager.ApplyGameContext`, `MainUIController.SetBattleResult`, `SceneManager.LoadScene`
  - `SceneBinder`
    - 참조: `GameContext`, `PlayerController`, `EnemyController`, `EnemyDatabase`, `EnvironmentController`
    - 주요 호출: `PlayerController.SetReactorMaxPower`, `SetReactorPower`, `SetFacilitySystems`, `SetCrewsState`, `SetWeaponTypes`, `CombatSystem.InitCombatSystem`, `EnvironmentController.InitStageEnvironment`
  - `ShipController`
    - 참조/소유: `ShipData`, `Room`, `CrewController`, `CrewSystem`, `WeaponSystem`, `ShipPowerController`, `ShipOwnership`, `FacilityController`, `WeaponDatabase`
    - 주요 책임: 함선 런타임 초기화, 방/승무원/무기 생성, 시설 연결, 피해/사망 처리
  - `CombatSystem`
    - 참조: `PlayerController`, `EnemyController`, `ShipController`, `ShieldController`, `ProjectileController`, `Room`, `CameraShakeController`, `SpriteShakeController`, `FloatingTextController`, `ExplosionController`
    - 주요 책임: 양측 함선 매핑, 투사체의 상대 함선 영역 이동, 보호막/방 피해 처리, 풀링 연출
  - `FacilityController` 계열
    - 공통 부모: `FacilityController`
    - 하위 클래스: `WeaponSystem`, `EngineController`, `ShieldController`
    - 주요 책임: 시설 전력, 피해, 수리, 승무원 보조 효과 처리
  - `ShipPowerController`
    - 참조: `ReactorController`, `FacilityController` 목록
    - 주요 책임: 반응로 전력 할당/회수, 시설별 할당 전력 저장
  - `EnvironmentController`
    - 참조: `CombatSystem`, `ShipController`, `ProjectileController`, `SolarFlareController`
    - 주요 책임: 소행성/태양 플레어 스테이지 효과 실행
  - `ShopManager`
    - 참조: `GameManager`, `GameContext`, `CrewViewer`
    - 주요 책임: 전투 준비 씬에서 시설/반응로 강화, 승무원 회복, 함선 수리, 방문 횟수 관리

- 씬 전환 시 유지되는 데이터와 씬 오브젝트 의존성
  - 유지 데이터
    - `GameContext`가 플레이어 함선의 방 목록 복사본, 시설 상태, 승무원 상태, 스크랩, 함선 HP, 반응로 전력, 무기 타입 목록, 적 타입을 보관한다.
    - `ShopManager.VisitCount`는 `BattleScene`의 선형 진행에서 적 타입과 환경 조합을 선택하는 데 사용된다.
  - 씬 오브젝트 의존성
    - 전투 씬에는 `SceneBinder`, `PlayerController`, `CombatSystem`, `GameManager`, 적 스폰 위치/부모, 적 승무원/무기 UI 그룹이 필요하다.
    - 환경 효과를 사용하려면 `SceneBinder`의 `EnvironmentController` 참조와 `EnvironmentController` 내부의 `SolarFlareController`, 소행성 투사체 프리팹 참조가 필요하다.
    - 전투 준비 씬에는 `ShopManager`가 필요하며, `GameManager.OnSceneLoaded`가 `ShopManager.ApplyGameContext(gameContext)`를 호출한다.
    - `MainUIController`는 결과 UI 표시와 버튼 기반 씬 이동을 담당하므로 전투 결과 UI가 필요한 씬에서 필요하다.

## 2. 클래스 구조 정리

### 2.1 핵심 매니저/시스템

| 클래스 | 역할 | 주요 참조 | 주요 책임 | UML 표시 포인트 |
|---|---|---|---|---|
| `GameManager` | 전체 게임 흐름과 전역 상태 관리 | `GameContext`, `EnemyDatabase`, `WeaponDatabase`, `PlayerController`, `MainUIController`, `ShopManager`, `SceneBinder`, `BackgroundController` | 씬 로드 처리, 전투 결과 처리, 일시정지/정지, `GameContext` 저장/복원, 상점/전투 씬 연결 | `SingletonBase<GameManager>` 상속, `SceneManager.sceneLoaded` 이벤트 구독 |
| `SceneBinder` | 전투 씬 런타임 바인딩 | `GameContext`, `PlayerController`, `EnemyController`, `EnvironmentController`, `EnemyDatabase` | 적 생성, 적 UI 그룹 연결, 플레이어 상태 복원, `CombatSystem` 초기화, 환경 초기화 | `MonoBehaviour`, `GameContext -> SceneBinder -> Player/Enemy/CombatSystem` 흐름 |
| `CombatSystem` | 전투 충돌/피해/연출 중재 | `PlayerController`, `EnemyController`, `ShipController`, `ShieldController`, `ProjectileController`, `Room`, `SpriteShakeController`, `CameraShakeController` | 투사체 영역 이동, 보호막/방 충돌 처리, 함선 진영별 콜라이더 매핑, 폭발/피해 텍스트 풀링 | `SingletonBase<CombatSystem>` 상속, `ShipSide` 기반 딕셔너리 보유 |
| `ShipController` | 플레이어/적 함선 공통 기반 | `ShipData`, `Room`, `CrewController`, `CrewSystem`, `WeaponSystem`, `ShipPowerController`, `ShipOwnership`, `FacilityController`, `WeaponDatabase` | 방/승무원/무기 생성, 시설 등록, 피해/수리/사망 처리, 전투 기능 정지 | `MonoBehaviour`, `IDamageable`, `IDeadable` 구현, `PlayerController`/`EnemyController` 부모 |
| `FacilityController` | 시설 공통 상태/전력/피해 제어 | `ShipController`, `ShipPowerController`, `FacilityState`, `RoomType`, `FacilityStatus` | 전력 증가/감소, 피해/수리, 시설 상태 갱신, 승무원 보조 효과 | `MonoBehaviour`, `WeaponSystem`/`EngineController`/`ShieldController` 부모 |
| `WeaponSystem` | 무기 시설 및 타겟 관리 | `WeaponController`, `ProjectileTargetData`, `ITargeting`, `CursorController` | 무기 전력 활성화, 충전 속도 반영, 선택 무기/목표 방 관리, 적 타겟팅 시스템 주입 | `FacilityController` 상속, `List<WeaponController>` 소유 |
| `EngineController` | 엔진 시설 제어 | `FacilityController`, `GameManager` | 엔진 전력 기반 탈출 충전, `CanEscape` 상태 갱신 | `FacilityController` 상속 |
| `ShieldController` | 보호막 시설 제어 | `FacilityController`, `ProjectileController` | 전력 기반 최대 보호막 계산, 보호막 충전, 보호막 피해 처리 | `FacilityController` 상속 |
| `ReactorController` | 반응로 전력 저장소 | `ShipPowerController` | 최대 전력/현재 전력 증가, 감소, 직접 설정 | `MonoBehaviour`, `ShipPowerController`가 참조 |
| `ShipPowerController` | 함선 전력 분배 | `ReactorController`, `FacilityController`, `RoomType`, `FacilityState` | 반응로 전력 할당/회수, 시설별 할당 전력 관리, 저장 상태 적용 | `MonoBehaviour`, `EnemyPowerController` 부모 |
| `EnemyPowerController` | 적 전력 초기 할당 | `PowerData`, `ShipPowerController` | 적 함선 시작 시 실드/무기/엔진 전력 자동 할당 | `ShipPowerController` 상속, `PowerData` 포함 |
| `CrewSystem` | 승무원 선택/이동/사망 관리 | `ShipController`, `CrewController`, `Room`, `FacilityController` | 선택 승무원 목록 관리, 이동 목표 지정, 승무원 사망 반영 | `MonoBehaviour`, `ShipController`와 같은 함선 오브젝트 구성 |
| `EnvironmentController` | 전투 환경 효과 관리 | `CombatSystem`, `ShipController`, `ProjectileController`, `SolarFlareController` | 소행성 투사체 생성, 태양 플레어 피해 적용, 환경 코루틴 실행 | `MonoBehaviour`, `SceneBinder`에서 초기화 |
| `ShopManager` | 전투 준비 씬 강화/회복 관리 | `GameManager`, `GameContext`, `CrewViewer`, Unity UI `Button` | 시설/반응로 강화, 승무원 회복, 함선 수리, 방문 횟수 증가 | `MonoBehaviour`, static `VisitCount` 사용 |
| `MainUIController` | UI 버튼/결과 UI 제어 | `GameManager`, `BattleResultViewer`, `SceneManager` | 씬 이동, 메인 메뉴 복귀, 게임 종료, 도주 처리, 결과 UI 표시 | `MonoBehaviour`, UI 이벤트 진입점 |
| `CursorController` | 커서 이미지 제어 | `WeaponSystem` | 조준 커서/기본 커서 전환 | `SingletonBase<CursorController>` 상속 |
| `BackgroundController` | 배경 랜덤 선택 | 전투 씬 배경 오브젝트 | 전투 씬 배경 변경 | `MonoBehaviour`, `GameManager.ChangeBackground` 및 씬 로드 흐름에서 호출 |

### 2.2 Controller 계열

| 클래스 | 역할 | 주요 참조 | 주요 책임 | UML 표시 포인트 |
|---|---|---|---|---|
| `PlayerController` | 플레이어 함선 입력 제어 | `GameManager`, `CombatSystem`, `CrewSystem`, `WeaponSystem`, `ShipPowerController`, `Room`, `CrewController` | 키 입력 전력 제어, 마우스 클릭 조준/승무원 선택, 저장 상태 복원 | `ShipController` 상속, 사용자 입력에서 시스템 호출로 이어지는 핵심 클래스 |
| `EnemyController` | 적 함선 제어 | `CombatSystem`, `RandomTargeter`, `WeaponSystem`, `ShipOwnership` | 플레이어 방 목록 기반 랜덤 타겟팅 구성, 적 진영 설정 | `ShipController` 상속, `ITargeting` 구현체를 `WeaponSystem`에 주입 |
| `WeaponController` | 단일 무기 동작 | `WeaponSystem`, `ITargeting`, `ProjectileController`, `ProjectileTargetData` | 충전/방전, 준비 상태 갱신, 플레이어/적 발사, 투사체 생성 | `MonoBehaviour`, `WeaponSystem`이 여러 개 보유 |
| `ProjectileController` | 투사체 이동/충돌 제어 | `CombatSystem`, `ProjectileTargetData`, `Collider2D` | 목표 지점 이동, 시작/종료 함선 콜라이더 처리, 보호막/방 충돌 전달 | `MonoBehaviour`, 충돌 이벤트에서 `CombatSystem` 호출 |
| `CrewController` | 단일 승무원 동작 | `CrewSystem`, `Room`, `CrewState`, `FacilityController` | 현재/목표 방 관리, 이동, 수리, 시설 보조, 피해/사망 처리 | `MonoBehaviour`, `CrewSystem`과 양방향 참조 |
| `CameraShakeController` | 카메라 흔들림 제어 | `CombatSystem` | 피격 시 카메라 흔들림 처리 | `MonoBehaviour`, 현재 `CombatSystem`에서 참조하나 호출 일부는 주석 처리 |
| `SolarFlareController` | 태양 플레어 화면 효과 | Unity UI `Image`, `EnvironmentController` | 플레어 색상 전환 코루틴 실행 | `MonoBehaviour`, `EnvironmentController`가 호출 |
| `ExplosionController` | 폭발 연출 제어 | `CombatSystem` | 폭발 오브젝트 재생/비활성화 | `MonoBehaviour`, `CombatSystem` 풀링 대상 |
| `FloatingTextController` | 피해량 텍스트 제어 | `CombatSystem` | 텍스트/색상 설정, 표시 연출 | `MonoBehaviour`, `CombatSystem` 풀링 대상 |
| `AspectRatioController` | 카메라 화면 비율 보정 | `Camera` | 화면 비율 조정 | `MonoBehaviour`, `Main Camera.prefab` 연결 |

### 2.3 Viewer/UI 계열

| 클래스 | 역할 | 주요 참조 | 주요 책임 | UML 표시 포인트 |
|---|---|---|---|---|
| `PowerViewer` | 전력 UI 공통 표시 | `PowerBarStatus` | 전력 바 상태 표시 | `MonoBehaviour`, `FacilityPowerViewer` 부모 |
| `FacilityPowerViewer` | 시설 전력 UI 입력/표시 | `PowerViewer`, `ShipPowerController`, `RoomType`, `IPointerClickHandler` | 클릭으로 시설 전력 할당/회수, 시설 전력 상태 표시 | `PowerViewer` 상속, `IPointerClickHandler` 구현 |
| `ReactorViewer` | 반응로 전력 UI | `ReactorController` 또는 `ShipPowerController` | 반응로 전력 표시 | `MonoBehaviour`, 상세 Inspector 연결 확인 필요 |
| `ShieldViewer` | 보호막 UI | `ShieldController` | 보호막 개수/충전 표시 | `MonoBehaviour`, 상세 Inspector 연결 확인 필요 |
| `EngineViewer` | 엔진/탈출 UI | `EngineController` | 탈출 충전 상태 표시 | `MonoBehaviour` |
| `HpViewer` | 함선 HP UI | `ShipController`, `EHpBarStatus` | 함선 체력 바 표시 | `MonoBehaviour` |
| `ScrapViewer` | 스크랩 UI | `GameManager` 또는 `PlayerController` | 스크랩 수량 표시 | `MonoBehaviour`, 상세 Inspector 연결 확인 필요 |
| `RoomViewer` | 방 표시 UI/스프라이트 | `Room` | 방 위치/피격/시설 상태 표시 | `MonoBehaviour`, `Room.prefab` 연결 |
| `WeaponViewer` | 무기 충전 UI 연결 | `WeaponController`, Unity UI `Image` | 무기 충전 바 참조 설정/표시 | `MonoBehaviour`, 무기 프리팹 연결 |
| `CrewViewer` | 승무원 요약/회복 UI | `CrewController`, Unity UI `Button`, SpriteRenderer | 승무원 스프라이트/목표 스프라이트/회복 버튼 연결 | `MonoBehaviour`, `Crew Summary.prefab` 연결 |
| `BattleResultViewer` | 전투 결과 UI | `BattleResult`, `GameManager` | 결과 텍스트/보상 표시 및 이후 씬 이동 연결 | `MonoBehaviour`, `MainUIController`가 활성화 |
| `ShipViewer` | 함선 표시 | `ShipController` | 함선 시각 상태 표시 | `MonoBehaviour`, 적 프리팹 연결 |
| `SpriteShakeController` | 스프라이트 흔들림 연출 | `CombatSystem`, `ShipController` | 피격 시 함선/방 흔들림 | `MonoBehaviour`, 함선/방 프리팹 연결 |

### 2.4 데이터/상태 클래스

| 클래스 | 역할 | 주요 필드 | 사용 위치 | UML 표시 포인트 |
|---|---|---|---|---|
| `GameContext` | 씬 전환 간 플레이어 상태 저장 | `Rooms`, `FacilityStates`, `CrewStates`, `FacilityUpgradeCount`, `ScrapAmount`, `ShipHp`, `ShipMaxHp`, `ShipReactorCapacity`, `ShipPower`, `ReactorUpgradeCount`, `WeaponTypes`, `EnemyType` | `GameManager`, `SceneBinder`, `ShopManager` | `[Serializable]` 순수 데이터 클래스, `MonoBehaviour` 아님 |
| `Room` | 함선 방 데이터/런타임 상태 | `neighborRoomIds`, `FacilitySystem`, `Crews`, `Type`, `LocalPosition`, `GlobalPosition`, `MaxCrewCapacity`, `Id`, `IsHit` | `ShipData`, `ShipController`, `CombatSystem`, `CrewController`, `ProjectileController` | `[Serializable]` 순수 데이터 클래스, `FacilityController`/`CrewController` 참조 |
| `ShipData` | 함선 기본 데이터 ScriptableObject | `MaxHp`, `Rooms` | `ShipController`, `PlayerShipData.asset`, `EnemyShip*Data.asset` | `ScriptableObject`, 함선 방 배치 데이터 소스 |
| `FacilityState` | 시설 저장 상태 | `TotalDamage`, `MaxPowerLimit`, `Power`, `Type` | `GameContext`, `FacilityController`, `PlayerController`, `ShipPowerController` | 순수 데이터 클래스, `FacilityController`에서 생성 가능 |
| `CrewState` | 승무원 저장 상태 | `MaxHp`, `Hp`, `Name`, `LastRoomId` | `GameContext`, `CrewController`, `PlayerController` | 순수 데이터 클래스, `CrewController`에서 생성 가능 |
| `ProjectileTargetData` | 투사체 목표 데이터 | `TargetPosition`, `RoomId` | `WeaponSystem`, `WeaponController`, `ProjectileController`, `EnvironmentController` | `struct`, 타겟 방/좌표 전달 |
| `WeaponTransformData` | 무기 배치 데이터 | `WeaponOffset`, `WeaponRotation`, `isFlipX`, `isFlipY` | `ShipController.InitWeapons` | `[Serializable]`, 함선별 무기 위치 보정 |
| `EnemyDatabase` | 적 타입별 프리팹 DB | `EnemyPrefabEntry[] entries` | `GameManager`, `SceneBinder`, `EnemyDatabase.asset` | `ScriptableObject`, `EnemyType -> GameObject` 매핑 |
| `EnemyPrefabEntry` | 적 프리팹 매핑 단위 | `enemyType`, `prefab` | `EnemyDatabase` | `[Serializable]` 데이터 클래스 |
| `WeaponDatabase` | 무기 타입별 프리팹 DB | `WeaponPrefabEntry[] entries` | `GameManager`, `ShipController`, `WeaponDatabase.asset` | `ScriptableObject`, `WeaponType -> GameObject` 매핑 |
| `WeaponPrefabEntry` | 무기 프리팹 매핑 단위 | `weaponType`, `prefab` | `WeaponDatabase` | `[Serializable]` 데이터 클래스 |
| `PowerData` | 적 시작 전력 데이터 | `ShieldPower`, `WeaponPower`, `EnginePower` | `EnemyPowerController` | `[Serializable]` 순수 데이터 클래스 |
| `RoomGraph` | 방 그래프 관련 데이터/로직 | 확인 필요 | 확인 필요 | 파일 존재, 구체 사용 관계 확인 필요 |
| `ShipOwnership` | 함선 진영 정보 | `ShipSide` | `ShipController`, `CombatSystem`, `PlayerController`, `EnemyController` | `MonoBehaviour`, `ShipSide` 기반 매핑 |
| `BattleResult` | 전투 결과 enum | `None`, 플레이어/적 파괴/전멸/도주 등 | `GameManager`, `MainUIController`, `BattleResultViewer`, `ShipController` | UML enum |
| `EnemyType` | 적 종류 enum | `None`, `Random`, `Enemy1`, `Enemy2`, `Enemy3` | `GameManager`, `EnemyDatabase`, `SceneBinder`, `GameContext` | UML enum |
| `WeaponType` | 무기 종류 enum | `BasicLaser1`, `BurstLaser1`, `BurstLaser2`, `BurstLaser3`, `Missile1` 등 | `GameContext`, `WeaponDatabase`, `ShipController` | UML enum |
| `RoomType` | 방/시설 종류 enum | `Empty = -1`, `Weapon = 0`, `Engine`, `Shield` | `Room`, `FacilityController`, `ShipPowerController`, `ShopManager` | UML enum |
| `ShipSide` | 함선 진영 enum | `None`, `Player`, `Enemy0` | `ShipOwnership`, `CombatSystem`, `PlayerController` | UML enum |
| `FacilityStatus` | 시설 상태 enum | `Normal`, `Damaged`, `Broked`, `Disabled` | `FacilityController`, `CrewController`, `Room` | UML enum |
| `CrewStatus` | 승무원 상태 enum | `Moving`, `Idle`, `Repair`, `Assisting` | `CrewController` | UML enum |
| `EDamageType` | 피해 표시 타입 enum | `Shield`, `Room`, `Crew` | `CombatSystem`, 피해 텍스트 확장 메서드 | UML enum, `CombatSystem.cs` 내부 선언 |
| `PowerBarStatus` | 전력 UI 상태 enum | `Available`, `Broken`, `UnAvailable` | `PowerViewer`, `FacilityPowerViewer` | UML enum |
| `ReactorBarStatus` | 반응로 UI 상태 enum | 확인 필요 | `ReactorViewer` | UML enum |
| `EHpBarStatus` | HP 바 UI 상태 enum | `Available`, `UnAvailable` | `HpViewer` | UML enum |
| `ExplosionType` | 폭발 타입 enum | 확인 필요 | 폭발 관련 리소스 | UML enum |
| `SceneIndex` | 씬 인덱스 enum | `MainMenu`, `Battle`, `GameOver`, `BattlePrepare` | `MainUIController`, `GameManager` | UML enum |

### 2.5 기타 클래스

| 클래스 | 역할 | 사용 위치 | 비고 |
|---|---|---|---|
| `SingletonBase<T>` | 싱글톤 기반 클래스 | `GameManager`, `CombatSystem`, `CursorController` | `Utilities` 네임스페이스 |
| `IDamageable` | 피해 처리 인터페이스 | `ShipController` | 구현 클래스: `ShipController` |
| `IDeadable` | 사망 처리 인터페이스 | `ShipController` | 구현 클래스: `ShipController` |
| `ITargeting` | 목표 방 선택 인터페이스 | `WeaponController`, `WeaponSystem`, `RandomTargeter` | 적 무기 타겟팅 추상화 |
| `RandomTargeter` | 랜덤 방 선택 타겟터 | `EnemyController`, `WeaponController` | `ITargeting` 구현, `List<Room>` 복사 보관 |
| `ComponentPool<T>` | 컴포넌트 풀 유틸 | 확인 필요 | 제네릭 유틸 클래스 |
| `SimpleInput` | 입력 래퍼 유틸 | `GameManager`, `PlayerController`, `ProjectileController` 등 | `UnityEngine.InputSystem` 기반 유틸 |
| `CharacterController2D` | 2D 캐릭터 컨트롤 유틸 | 확인 필요 | 현재 핵심 전투 구조와 직접 관계 확인 필요 |
| `SpriteFlipper` | 애니메이션 상태 기반 스프라이트 플립 | Animator StateMachineBehaviour | 사용 위치 확인 필요 |
| `Test`, `TestA`, `TestB` | 테스트/실험용 클래스 | `Assets/_Scripts/zTest` | UML 핵심 구조에서는 낮은 우선순위 |

## 3. UML 다이어그램 작성용 관계 요약

- 상속 관계
  - `GameManager -> SingletonBase<GameManager>`
  - `CombatSystem -> SingletonBase<CombatSystem>`
  - `CursorController -> SingletonBase<CursorController>`
  - `PlayerController -> ShipController -> MonoBehaviour`
  - `EnemyController -> ShipController -> MonoBehaviour`
  - `WeaponSystem -> FacilityController -> MonoBehaviour`
  - `EngineController -> FacilityController -> MonoBehaviour`
  - `ShieldController -> FacilityController -> MonoBehaviour`
  - `EnemyPowerController -> ShipPowerController -> MonoBehaviour`
  - `FacilityPowerViewer -> PowerViewer -> MonoBehaviour`
  - `ShipData -> ScriptableObject`
  - `EnemyDatabase -> ScriptableObject`
  - `WeaponDatabase -> ScriptableObject`

- 인터페이스 구현 관계
  - `ShipController` implements `IDamageable`
  - `ShipController` implements `IDeadable`
  - `RandomTargeter` implements `ITargeting`
  - `FacilityPowerViewer` implements `IPointerClickHandler`

- 포함/소유 관계
  - `GameManager` owns/reference `GameContext`
  - `GameContext` contains `List<Room>`, `List<FacilityState>`, `List<CrewState>`, `List<int> FacilityUpgradeCount`, `List<WeaponType>`
  - `ShipController` contains `List<Room> Rooms`, `List<CrewController> Crews`, `List<FacilityController> FacilitySystems`, `List<WeaponType> WeaponTypes`
  - `ShipController` owns/uses component references: `CrewSystem`, `WeaponSystem`, `ShipPowerController`, `ShipOwnership`
  - `ShipController` instantiates `Room.prefab`, `Crew.prefab`, `Crew Summary.prefab`, weapon prefabs, `Weapon Charge Status.prefab`
  - `Room` references one `FacilityController` and a list of `CrewController`
  - `WeaponSystem` owns `List<WeaponController>` and target marker objects
  - `WeaponController` owns a projectile prefab reference and uses `ProjectileTargetData`
  - `ShipPowerController` owns/reference-list `FacilityController` and one `ReactorController`
  - `EnemyPowerController` contains `PowerData`
  - `CombatSystem` stores arrays/dictionaries for `ShipController`, `Collider2D`, `ShipSide`, `SpriteShakeController`
  - `EnvironmentController` owns coroutine state and references `SolarFlareController`, asteroid projectile prefab, asteroid group transform
  - `EnemyDatabase` contains `EnemyPrefabEntry[]`
  - `WeaponDatabase` contains `WeaponPrefabEntry[]`
  - `ShipData` contains `List<Room>`

- 참조 관계
  - `GameManager -> SceneBinder`: 씬 로드 후 전투 오브젝트 바인딩 요청
  - `GameManager -> GameContext`: 전투 결과/상점/씬 전환 상태 저장
  - `GameManager -> MainUIController`: 결과 UI 표시 및 씬 이동 위임
  - `GameManager -> ShopManager`: 전투 준비 씬에서 `GameContext` 적용
  - `GameManager -> BackgroundController`: 전투 씬 배경 랜덤 변경
  - `SceneBinder -> EnemyDatabase`: 적 타입으로 적 프리팹 조회
  - `SceneBinder -> PlayerController`: HP, 스크랩, 반응로, 시설, 승무원, 무기 상태 복원
  - `SceneBinder -> EnemyController`: 적 프리팹 생성 및 외부 UI 그룹 연결
  - `SceneBinder -> CombatSystem`: 플레이어/적 함선 참조 초기화
  - `SceneBinder -> EnvironmentController`: 환경 플래그 설정 및 환경 코루틴 시작
  - `PlayerController -> CombatSystem`: 클릭 위치에 해당하는 적 방 조회
  - `PlayerController -> WeaponSystem`: 선택 무기와 목표 방 설정
  - `PlayerController -> CrewSystem`: 승무원 선택 및 이동 목표 설정
  - `EnemyController -> RandomTargeter -> Room`: 플레이어 방 목록 기반 무작위 목표 선택
  - `WeaponSystem -> WeaponController`: 전력 상태/충전 속도/타겟팅 시스템 전달
  - `WeaponController -> ProjectileController`: 발사 시 투사체 생성과 타겟 데이터 주입
  - `ProjectileController -> CombatSystem`: 보호막/방 충돌 처리 요청
  - `CombatSystem -> ShieldController`: 보호막 피해 처리
  - `CombatSystem -> ShipController`: 방 피해 처리
  - `CrewController -> Room -> FacilityController`: 수리/보조/피해 처리
  - `ShopManager -> GameManager -> GameContext`: 강화/회복 결과 저장
  - `EnvironmentController -> CombatSystem`: 환경 피해 대상 함선/콜라이더 조회
  - `EnvironmentController -> ProjectileController`: 소행성 투사체 타겟/콜라이더 설정
  - `EnvironmentController -> ShipController`: 태양 플레어 방 피해 직접 적용

- 호출 흐름상 중요한 관계
  - 전투 씬 진입
    - `SceneManager.sceneLoaded`
    - `GameManager.OnSceneLoaded`
    - `GameManager.BindLinearBattleScene` 또는 `SceneBinder.BindSceneObjects`
    - `SceneBinder.SpawnEnemyOrFindSceneEnemy`
    - `PlayerController.SetReactorMaxPower / SetReactorPower / SetFacilitySystems / SetCrewsState / SetWeaponTypes`
    - `CombatSystem.InitCombatSystem`
    - `EnvironmentController.SetStageEnvironment / InitStageEnvironment`
  - 함선 초기화
    - `ShipController.Awake`
    - `ShipController.InitShipReferences`
    - `ShipController.TryInitializeShip`
    - `InitRooms`
    - `InitCrews`
    - `InitWeapons`
    - `WeaponSystem.SetWeaponSystemOfWeapons`
  - 플레이어 공격
    - `PlayerController.Update`
    - `PlayerController.HandleMouse`
    - `CombatSystem.GetRoomOrNull`
    - `WeaponSystem.SetSelectedWeapon`
    - `WeaponSystem.SetSelectedRoomTarget`
    - `WeaponController.FireCoroutine`
    - `ProjectileController.OnTriggerExit2D / OnTriggerEnter2D / Update`
    - `CombatSystem.HandleShieldCollision` 또는 `CombatSystem.HandleRoomCollision`
  - 적 공격
    - `EnemyController.Start`
    - `RandomTargeter.GetTarget`
    - `WeaponSystem.SetWeaponsTargetingSystem`
    - `WeaponController.FireCoroutine(float seed)`
    - `ProjectileController`
    - `CombatSystem`
  - 승무원 이동/수리/보조
    - `PlayerController.HandleMouse`
    - `CrewSystem.AddSelectedCrew`
    - `CrewSystem.SetTargetRoom`
    - `CrewController.SetTargetRoom`
    - `CrewController.Update`
    - `Room.RepairFacility` 또는 `Room.ApplyAssistEffectOnFacility`
  - 전투 종료
    - `CombatSystem.HandleRoomCollision`
    - `ShipController.TakeDamageToRoom`
    - `ShipController.CheckGameOver`
    - `GameManager.StopBattle`
    - `ShipController.DeadCoroutine`
    - `GameManager.HandleBattleResult`
    - `MainUIController.SetBattleResult`
  - 상점/전투 준비
    - `ShopManager.Awake`
    - `GameManager.EnsureBattlePrepareTestGameContext`
    - `ShopManager.ApplyGameContext`
    - `ShopManager.IncreaseMaxPowerLimit / IncreaseReactorMaxPower / HealCrew / Repair`
    - `GameManager`의 `GameContext` 변경 메서드

## 4. 프리팹 정리

| 프리팹 | 역할 | 연결 스크립트 | 사용 씬/시스템 | 재사용성 | 비고 |
|---|---|---|---|---|---|
| `Assets/_Prefabs/GameManager.prefab` | 전역 게임 매니저 | `GameManager` | 전체 게임 흐름 | 전역 재사용 | `EnemyDatabase`, `WeaponDatabase`, UI 참조 필요 |
| `Assets/_Prefabs/Main Camera.prefab` | 메인 카메라 | `AspectRatioController` | 여러 씬 | 재사용 가능 | 외부/Unity 컴포넌트 포함 |
| `Assets/_Prefabs/CustomMouse.prefab` | 커스텀 커서 | `CursorController` | 입력/무기 조준 | 재사용 가능 | 싱글톤 |
| `Assets/_Prefabs/Room.prefab` | 함선 방 오브젝트 | `RoomViewer`, `SpriteShakeController` | `ShipController.InitRooms` | 재사용 가능 | 외부/Unity 컴포넌트 포함 |
| `Assets/_Prefabs/Crew.prefab` | 승무원 오브젝트 | `CrewController` | `ShipController.InitCrews`, `CrewSystem` | 재사용 가능 | 플레이어/적 공통 |
| `Assets/_Prefabs/Shield.prefab` | 보호막 오브젝트 | 현재 프로젝트 스크립트 없음 | 함선/보호막 표시 추정 | 확인 필요 | 연결 관계 확인 필요 |
| `Assets/_Prefabs/EnemySpawnPoint.prefab` | 적 생성 위치 | 없음 | `SceneBinder`의 spawn point로 사용 가능 | 씬 전용 가능성 | Transform 목적 프리팹 |
| `Assets/_Prefabs/Background Canvas.prefab` | 배경 캔버스 | 현재 프로젝트 스크립트 매핑 없음 | 배경 시스템/씬 UI | 확인 필요 | Unity UI 또는 외부 스크립트 포함 |
| `Assets/_Prefabs/Solar Flare.prefab` | 태양 플레어 효과 | `SolarFlareController` | `EnvironmentController` | 재사용 가능 | Unity UI 컴포넌트 포함 |
| `Assets/_Prefabs/Floating Text.prefab` | 피해량 텍스트 | `FloatingTextController` | `CombatSystem` | 재사용 가능 | 풀링 대상, 외부/Unity UI 컴포넌트 포함 |
| `Assets/_Prefabs/Enemy/Enemy1.prefab` | 적 함선 1 | `EnemyController`, `EnemyPowerController`, `CrewSystem`, `WeaponSystem`, `EngineController`, `ShieldController`, `ReactorController`, `ShipOwnership`, `ShipViewer`, `SpriteShakeController` | `EnemyDatabase`, `SceneBinder` | 재사용 가능 | `EnemyType.Enemy1` |
| `Assets/_Prefabs/Enemy/Enemy2.prefab` | 적 함선 2 | `EnemyController`, `EnemyPowerController`, `CrewSystem`, `WeaponSystem`, `EngineController`, `ShieldController`, `ReactorController`, `ShipOwnership`, `ShipViewer`, `SpriteShakeController` | `EnemyDatabase`, `SceneBinder` | 재사용 가능 | `EnemyType.Enemy2` |
| `Assets/_Prefabs/Enemy/Enemy3.prefab` | 적 함선 3 | `EnemyController`, `EnemyPowerController`, `CrewSystem`, `WeaponSystem`, `EngineController`, `ShieldController`, `ReactorController`, `ShipOwnership`, `ShipViewer`, `SpriteShakeController` | `EnemyDatabase`, `SceneBinder` | 재사용 가능 | `EnemyType.Enemy3` |
| `Assets/_Prefabs/Weapon/Basic Laser 1.prefab` | 기본 레이저 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController.InitWeapons` | 재사용 가능 | 투사체 프리팹 참조 |
| `Assets/_Prefabs/Weapon/Burst Laser 1.prefab` | 버스트 레이저 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController.InitWeapons` | 재사용 가능 | 투사체 프리팹 참조 |
| `Assets/_Prefabs/Weapon/Burst Laser 2.prefab` | 버스트 레이저 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController.InitWeapons` | 재사용 가능 | 투사체 프리팹 참조 |
| `Assets/_Prefabs/Weapon/Burst Laser 3.prefab` | 버스트 레이저 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController.InitWeapons` | 재사용 가능 | 투사체 프리팹 참조 |
| `Assets/_Prefabs/Weapon/Missile 1.prefab` | 미사일 무기 | `WeaponController`, `WeaponViewer` | `WeaponDatabase`, `ShipController.InitWeapons` | 재사용 가능 | 투사체 프리팹 참조 |
| `Assets/_Prefabs/Projectile/Red Laser Projectile.prefab` | 레이저 투사체 | `ProjectileController` | `WeaponController` | 재사용 가능 | 무기 프리팹에서 참조 |
| `Assets/_Prefabs/Projectile/Purple Laser Projectile.prefab` | 레이저 투사체 | `ProjectileController` | `WeaponController` | 재사용 가능 | 무기 프리팹에서 참조 |
| `Assets/_Prefabs/Projectile/Green Laser Projectile.prefab` | 레이저 투사체 | `ProjectileController` | `WeaponController` | 재사용 가능 | 무기 프리팹에서 참조 |
| `Assets/_Prefabs/Projectile/Basic Missile Projectile.prefab` | 미사일 투사체 | `ProjectileController` | `WeaponController` | 재사용 가능 | 무기 프리팹에서 참조 |
| `Assets/_Prefabs/Projectile/Asteroid Projectile.prefab` | 소행성 투사체 | `ProjectileController` | `EnvironmentController` | 재사용 가능 | 환경 효과용 |
| `Assets/_Prefabs/Explosion/Blue Explosion.prefab` | 폭발 연출 | `ExplosionController` | `CombatSystem` | 재사용 가능 | 풀링 대상 |
| `Assets/_Prefabs/Explosion/Dark Red Explosion.prefab` | 폭발 연출 | `ExplosionController` | `CombatSystem` | 재사용 가능 | 풀링 대상 |
| `Assets/_Prefabs/Explosion/Purple Explosion.prefab` | 폭발 연출 | `ExplosionController` | `CombatSystem` | 재사용 가능 | 풀링 대상 |
| `Assets/_Prefabs/Explosion/Red Explosion.prefab` | 폭발 연출 | `ExplosionController` | `CombatSystem` | 재사용 가능 | 풀링 대상 |
| `Assets/_Prefabs/_UI/Main UI.prefab` | 메인/전투 UI | `MainUIController` | 전투/메뉴 UI | 재사용 가능 | Unity UI/외부 컴포넌트 포함 |
| `Assets/_Prefabs/_UI/Battle Result UI.prefab` | 전투 결과 UI | `BattleResultViewer` | `MainUIController` | 재사용 가능 | 결과 표시 |
| `Assets/_Prefabs/_UI/Scrap UI.prefab` | 스크랩 UI | `ScrapViewer` | 전투/상점 UI | 재사용 가능 | Unity UI/외부 컴포넌트 포함 |
| `Assets/_Prefabs/_UI/Reactor Canvas.prefab` | 반응로 UI | `ReactorViewer` | 플레이어 함선 UI | 재사용 가능 | Unity UI 컴포넌트 포함 |
| `Assets/_Prefabs/_UI/Shield UI.prefab` | 보호막 UI | `ShieldViewer` | 함선 UI | 재사용 가능 | Unity UI 컴포넌트 포함 |
| `Assets/_Prefabs/_UI/Ship Hp/Hp Canvas.prefab` | 함선 HP UI | `HpViewer` | 함선 UI | 재사용 가능 | Unity UI 컴포넌트 포함 |
| `Assets/_Prefabs/_UI/Ship Hp/Hp Bar.prefab` | HP 바 단위 UI | 현재 프로젝트 스크립트 매핑 없음 | `HpViewer` 하위 UI 가능성 | 재사용 가능 | 확인 필요 |
| `Assets/_Prefabs/_UI/Power Bar/Power Bar.prefab` | 전력 칸 UI | 현재 프로젝트 스크립트 매핑 없음 | `PowerViewer` 하위 UI 가능성 | 재사용 가능 | 확인 필요 |
| `Assets/_Prefabs/_UI/Power Bar/Power Bar Group.prefab` | 전력 바 그룹 UI | 현재 프로젝트 스크립트 매핑 없음 | `PowerViewer` 하위 UI 가능성 | 재사용 가능 | 확인 필요 |
| `Assets/_Prefabs/_UI/Facility/Facilities Canvas.prefab` | 시설 UI 캔버스 | 현재 프로젝트 스크립트 매핑 없음 | 플레이어 함선 UI | 재사용 가능 | 확인 필요 |
| `Assets/_Prefabs/_UI/Facility/Facility UI.prefab` | 시설 전력 UI | `FacilityPowerViewer` | 시설 전력 조작 UI | 재사용 가능 | `IPointerClickHandler` 기반 |
| `Assets/_Prefabs/_UI/Facility/Facility Upgrade UI.prefab` | 시설 업그레이드 UI | `FacilityPowerViewer` | `ShopManager` 또는 상점 UI | 씬 전용 가능성 | 사용 위치 확인 필요 |
| `Assets/_Prefabs/_UI/Crew/Crew Summary.prefab` | 승무원 요약 UI | `CrewViewer` | `ShipController.InitCrews`, `ShopManager.InitCrewSummaries` | 재사용 가능 | 승무원 회복 버튼 포함 |
| `Assets/_Prefabs/_UI/Crew/Crew UI.prefab` | 승무원 UI | 현재 프로젝트 스크립트 매핑 없음 | 승무원 표시 UI | 확인 필요 | Unity UI 컴포넌트 중심 |
| `Assets/_Prefabs/_UI/Crew/Crew Heal Group.prefab` | 승무원 회복 UI 그룹 | 현재 프로젝트 스크립트 매핑 없음 | `ShopManager` 상점 UI 가능성 | 전투 준비 씬 전용 가능성 | 확인 필요 |
| `Assets/_Prefabs/_UI/Weapon Charge Status.prefab` | 무기 충전 상태 UI | 현재 프로젝트 스크립트 매핑 없음 | `ShipController.InitWeapons` | 재사용 가능 | 무기 생성 시 함께 생성 |
| `Assets/_Prefabs/_UI/Weapon Charge UI.prefab` | 무기 충전 UI 그룹 | 현재 프로젝트 스크립트 매핑 없음 | 무기 UI | 재사용 가능 | 확인 필요 |
| `Assets/_Prefabs/_UI/Ship Debug Buttons.prefab` | 디버그 버튼 UI | 현재 프로젝트 스크립트 매핑 없음 | 디버그/테스트 UI | 확인 필요 | 확인 필요 |
| `Assets/_Prefabs/_UI/Tutorial Canvas.prefab` | 튜토리얼 UI | 현재 프로젝트 스크립트 매핑 없음 | 튜토리얼 UI | 확인 필요 | 확인 필요 |

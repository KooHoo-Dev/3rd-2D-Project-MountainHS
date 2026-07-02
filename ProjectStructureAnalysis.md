# 프로젝트 구조 분석

## 1. 전체 구조 요약

- 핵심 흐름: `_MainMenu` 또는 `BattlePrepare`에서 `GameManager`가 유지되고, `BattleScene` 로드시 `SceneBinder`가 `GameContext`를 `PlayerController`에 다시 주입한 뒤 `CombatSystem`을 초기화한다.
- 전투 흐름: `PlayerController`/`EnemyController`가 `ShipController`를 상속하고, 함선 내부의 `WeaponSystem`, `EngineSystem`, `ShieldSystem`, `CrewSystem`, `ShipPowerController`가 전투 상태를 갱신한다.
- 씬 간 유지 데이터: `GameManager : SingletonBase<GameManager>`가 `DontDestroyOnLoad`로 유지되며 `GameContext`에 방, 시설 상태, 승무원 상태, 함선 HP, 스크랩, 반응로 전력을 저장한다.
- 씬 오브젝트 의존성: `BattleScene`은 `PlayerController`, `EnemyController`, `CombatSystem`, `SceneBinder`가 필요하다. `BattlePrepare`는 `ShopManager`가 `GameContext`를 읽고 업그레이드/회복 UI를 만든다.

## 2. 클래스 구조 정리

### 2.1 핵심 매니저/시스템

| 클래스 | 역할 | 주요 참조 | 주요 책임 | UML 표시 포인트 |
|---|---|---|---|---|
| `GameManager` | 씬 간 전투 결과/상태 관리 | `GameContext`, `PlayerController`, `MainUIController`, `SceneBinder`, `ShopManager` | 씬 로드 이벤트 처리, 전투 결과 처리, `GameContext` 저장/복원 | `SingletonBase<GameManager>` 상속, `GameContext` 소유 |
| `CombatSystem` | 전투 충돌/타겟 판정 중심 | `PlayerController`, `EnemyController`, `ProjectileController`, `ShieldSystem`, `Room` | 투사체 이동 영역 전환, 실드/방 충돌 처리, 함선 배열 관리 | `SingletonBase<CombatSystem>` 상속, 양 함선 참조 |
| `SceneBinder` | 전투 씬 오브젝트 바인딩 | `GameContext`, `PlayerController`, `EnemyController`, `CombatSystem` | 저장된 상태를 플레이어 함선에 적용하고 전투 시스템 초기화 | `GameManager.OnSceneLoaded`에서 호출 |
| `ShopManager` | 전투 준비/상점 UI 로직 | `GameManager`, `CrewViewer`, `Button`, `GameContext` | 시설/반응로 업그레이드, 승무원 회복, 함선 수리 | `BattlePrepare` 전용 가능성이 큼 |
| `ShipController` | 함선 공통 기반 | `ShipData`, `Room`, `CrewController`, `FacilitySystem`, `CrewSystem`, `ShipPowerController` | 방/승무원 생성, 데미지 처리, 사망 판정, 전력 요청 | `MonoBehaviour`, `IDamageable`, `IDeadable` 구현 |
| `FacilitySystem` | 시설 공통 기반 | `FacilityState`, `ERoomType`, `EFacilityStatus` | 전력, 피해, 수리, 보조 효과 상태 관리 | `WeaponSystem`, `EngineSystem`, `ShieldSystem` 부모 |
| `WeaponSystem` | 무기 시설 | `WeaponController`, `ITargeting`, `ProjectileTargetData` | 무기 활성화, 플레이어/적 타겟 전달 | `FacilitySystem` 상속 |
| `ShieldSystem` | 실드 시설 | `ShieldViewer`, `CombatSystem` | 전력 기반 실드 수, 충전, 실드 피해 처리 | `FacilitySystem` 상속 |
| `EngineSystem` | 엔진 시설 | `EngineViewer` | 도주 충전, 도주 가능 상태 계산 | `FacilitySystem` 상속 |
| `ShipPowerController` | 함선 전력 배분 | `ReactorSystem`, `FacilitySystem` | 시설 전력 할당/회수, 오버플로 처리 | `EnemyPowerController` 부모 |
| `EnemyPowerController` | 적 전력 기본 배분 | `ShipPowerController` | 시작 시 실드/무기/엔진 전력 자동 할당 | `ShipPowerController` 상속 |
| `ReactorSystem` | 반응로 전력 저장 | 없음 | 최대 전력/현재 전력 증감 | `MonoBehaviour` |
| `CrewSystem` | 승무원 선택/이동 집합 관리 | `CrewController`, `Room`, `ShipController` | 선택 승무원 관리, 목표 방 이동, 사망 승무원 제거 | `ShipController`와 상호 참조 |

### 2.2 Controller 계열

| 클래스 | 역할 | 주요 참조 | 주요 책임 | UML 표시 포인트 |
|---|---|---|---|---|
| `PlayerController` | 플레이어 함선 입력 처리 | `GameManager`, `CombatSystem`, `WeaponSystem`, `CrewSystem` | 마우스 클릭, 무기 조준, 승무원 선택/이동, 전력 입력 | `ShipController` 상속 |
| `EnemyController` | 적 함선 초기화 | `RandomTargeter`, `WeaponSystem`, `CombatSystem` | 적 타겟팅 시스템 연결, 적 소유권 설정 | `ShipController` 상속 |
| `WeaponController` | 개별 무기 | `ProjectileController`, `ProjectileTargetData` | 충전, 준비 상태, 투사체 생성 | `WeaponSystem`이 다수 소유 |
| `ProjectileController` | 투사체 | `CombatSystem`, `ProjectileTargetData`, `Collider2D` | 이동, 함선/실드/방 충돌 이벤트 처리 | `WeaponController`가 생성 |
| `CrewController` | 개별 승무원 | `Room`, `CrewSystem`, `CrewState` | 이동, 수리, 시설 보조, 피해/사망 처리 | `CrewSystem`이 관리 |
| `ShipOwnership` | 함선 소유 진영 표시 | `EShipSide` | Player/Enemy 판별용 값 보관 | `CombatSystem`, `PlayerController`, `EnemyController`에서 사용 |

### 2.3 Viewer/UI 계열

| 클래스 | 역할 | 주요 참조 | 주요 책임 | UML 표시 포인트 |
|---|---|---|---|---|
| `MainUIController` | 공통 씬 이동 UI | `GameManager`, `SceneManager` | 씬 이동, 메인 메뉴 복귀, 게임 종료, 도주 요청 | `Main UI.prefab` 및 씬에서 사용 |
| `CrewViewer` | 승무원 UI/선택 표시 | `CrewController`, `GameManager`, `CrewState` 간접 | 이름/HP 표시, 선택 글로우, 회복 버튼 | 전투/상점 양쪽에서 사용 |
| `EngineViewer` | 엔진 도주 UI | `EngineSystem` | 점프 충전 바, 점프 버튼 표시 | `EngineSystem` 참조 |
| `FacilityPowerViewer` | 시설 전력 UI | `FacilitySystem`, `GameManager` | 전력바, 피해바, 보조 아이콘, 상점용 상태 표시 | `PowerViewer` 상속 |
| `HpViewer` | 함선 HP UI | `ShipController`, `GameManager` | HP 칸 생성/색상 표시 | 전투/상점 양쪽 가능 |
| `PowerViewer` | 일반 전력바 UI | `FacilitySystem` | 시설 전력 표시 | `FacilityPowerViewer` 부모 |
| `ReactorViewer` | 반응로 전력 UI | `ReactorSystem`, `GameManager` | 반응로 전력칸 표시 | 상점에서는 `GameContext` 사용 가능 |
| `RoomViewer` | 방 시각화 | `Room` | 방 번호/타입 색상 표시 | `Room.prefab`에 연결 |
| `ScrapViewer` | 스크랩 UI | `GameManager` | 현재 스크랩 텍스트 표시 | `Scrap UI.prefab` |
| `ShieldViewer` | 실드 UI/스프라이트 표시 | `ShieldSystem` | 실드 칸, 충전바, 메인 실드 투명도 표시 | `Shield UI.prefab` |
| `ShipViewer` | 함선 방/연결 시각화 | `ShipController`, `RoomViewer`, `Room` | 방 Viewer 갱신, 방 연결선/Gizmos 표시 | `Room.neighborRoomIds`를 reflection으로 읽음 |
| `WeaponViewer` | 무기 충전 UI | `WeaponController` | 충전 스프라이트/바 갱신 | 개별 무기 오브젝트에 사용 |

### 2.4 데이터/상태 클래스

| 클래스 | 역할 | 주요 필드 | 사용 위치 | UML 표시 포인트 |
|---|---|---|---|---|
| `GameContext` | 씬 간 저장 데이터 | `Rooms`, `FacilityStates`, `CrewStates`, `ScrapAmount`, `ShipHp`, `ShipPower` | `GameManager`, `SceneBinder`, `ShopManager`, Viewer | 순수 `[Serializable]` 데이터 클래스 |
| `ShipData` | 함선 ScriptableObject | `MaxHp`, `List<Room>` | `ShipController` | `ScriptableObject`, `_Data`에 2개 존재 |
| `Room` | 방 데이터/런타임 상태 | `Id`, `Type`, `LocalPosition`, `GlobalPosition`, `Crews`, `FacilitySystem`, `neighborRoomIds` | `ShipData`, `ShipController`, `CrewController`, `CombatSystem` | 순수 데이터지만 런타임 참조 포함 |
| `FacilityState` | 시설 저장 상태 | `Type`, `TotalDamage`, `MaxPowerLimit`, `Power` | `GameContext`, `FacilitySystem.ApplyState` | 순수 상태 스냅샷 |
| `CrewState` | 승무원 저장 상태 | `Hp`, `MaxHp`, `Name`, `LastRoomId` | `GameContext`, `PlayerController.SetCrewsState` | 순수 상태 스냅샷 |
| `ProjectileTargetData` | 투사체 타겟 값 | `TargetPosition`, `RoomId` | `WeaponController`, `WeaponSystem`, `ProjectileController` | `struct` |
| Enums | 상태/타입 구분 | `ERoomType`, `EShipSide`, `EBattleResult`, etc. | 전투/시설/승무원 전반 | UML에서는 enum으로 표시 |

### 2.5 기타 클래스

| 클래스 | 역할 | 사용 위치 | 비고 |
|---|---|---|---|
| `RandomTargeter` | 랜덤 방 타겟 제공 | `EnemyController` | `ITargeting` 구현 |
| `RoomGraph` | 방 그래프 예정 구조 | 현재 직접 사용 확인 안 됨 | 확인 필요 |
| `SingletonBase<T>` | 싱글톤 기반 | `GameManager`, `CombatSystem` | `DontDestroyOnLoad` 처리 |
| `SimpleInput` | Input System 래퍼 | `GameManager`, `PlayerController` 등 | static utility |
| `Tags` | 태그/레이어 문자열 상수 | 전투/클릭/충돌 로직 | static utility |
| `LayerMaskExtensions` | LayerMask 편의 메서드 | `PlayerController`, `ProjectileController` | static utility |
| `TransformExtensions`, `CharacterController2D`, `ComponentPool<T>`, `SpriteFlipper` | 범용 유틸/샘플성 | 현재 핵심 FTL 흐름 직접 사용 확인 필요 | 중요도 낮음 |
| `zTest/Test`, `TestA`, `TestB` | 테스트/학습용 클래스 | 실제 씬/프리팹 연결 확인 안 됨 | 확인 필요 |

## 3. UML 다이어그램 작성용 관계 요약

- 상속 관계:
  - `GameManager -> SingletonBase<GameManager>`
  - `CombatSystem -> SingletonBase<CombatSystem>`
  - `PlayerController -> ShipController`
  - `EnemyController -> ShipController`
  - `WeaponSystem`, `ShieldSystem`, `EngineSystem -> FacilitySystem`
  - `EnemyPowerController -> ShipPowerController`
  - `FacilityPowerViewer -> PowerViewer`
- 인터페이스 구현 관계:
  - `ShipController : IDamageable, IDeadable`
  - `RandomTargeter : ITargeting`
- 포함/소유 관계:
  - `GameManager` owns `GameContext`
  - `GameContext` owns `Room`, `FacilityState`, `CrewState` lists
  - `ShipController` owns runtime `Rooms`, `Crews`, `FacilitySystems`
  - `WeaponSystem` owns `WeaponController` list
  - `CrewSystem` owns `CrewController` list and selected crew list
- 참조 관계:
  - `SceneBinder -> PlayerController/EnemyController/CombatSystem`
  - `PlayerController -> GameManager/CombatSystem/WeaponSystem/CrewSystem`
  - `EnemyController -> RandomTargeter -> Room`
  - `ProjectileController -> CombatSystem`
  - `Room -> FacilitySystem`, `Room -> CrewController`
  - Viewers mostly reference their matching System/Controller or fallback to `GameManager`
- 호출 흐름상 중요한 관계:
  - `SceneManager.sceneLoaded -> GameManager.OnSceneLoaded -> SceneBinder.BindSceneObjects -> CombatSystem.InitCombatSystem`
  - `WeaponController.Update -> Instantiate projectile -> ProjectileController.SetTarget`
  - `ProjectileController.OnTriggerExit2D -> CombatSystem.MoveToEnemyArea`
  - `ProjectileController.Update/OnTriggerEnter2D -> CombatSystem.HandleRoomCollision/HandleShieldCollision`
  - `ShipController.TakeDamageToRoom -> Room.TakeDamageToCrews/TakeDamageToFacility -> CheckGameOver -> GameManager.HandleBattleResult`
  - `ShopManager` upgrade/heal/repair methods -> `GameManager` -> `GameContext`

## 4. 프리팹 정리

| 프리팹 | 역할 | 연결 스크립트 | 사용 씬/시스템 | 재사용성 | 비고 |
|---|---|---|---|---|---|
| `GameManager.prefab` | 전역 매니저 | `GameManager` | `_MainMenu`, `BattleScene`, `BattlePrepare`, `GameOver` | 재사용/전역 | 싱글톤 |
| `Crew.prefab` | 승무원 런타임 오브젝트 | `CrewController` | `BattleScene`, `ShipController.InitCrews` | 재사용 | `Crew Sprite`, `Target Room` 포함 |
| `Room.prefab` | 방 표시/콜라이더 | `RoomViewer` | `BattleScene`, `ShipController.InitRooms` | 재사용 | 방 번호/색상 표시 |
| `Red Laser.prefab` | 투사체 | `ProjectileController` | `WeaponController` projectile prefab | 재사용 | 무기 발사체 |
| `Shield.prefab` | 실드 시각 오브젝트 | 직접 연결 스크립트 없음 | `BattleScene` | 확인 필요 | 시각용 가능성, 정확한 연결은 Inspector 확인 필요 |
| `Main UI.prefab` | 공통 메뉴/씬 이동 UI | `MainUIController` | 여러 씬 | 재사용 | Toggle Menu 포함 |
| `Scrap UI.prefab` | 스크랩 표시 | `ScrapViewer` | `BattleScene`, `BattlePrepare` | 재사용 | `GameManager.GetScrap` 사용 |
| `Reactor Canvas.prefab` | 반응로 전력 UI | `ReactorViewer` | `BattleScene`, `BattlePrepare` | 재사용 | `Power Bar` 중첩 |
| `Shield UI.prefab` | 실드 UI | `ShieldViewer` | `BattleScene` | 재사용 가능 | `ShieldSystem` 필요 |
| `Ship Debug Buttons.prefab` | 디버그 버튼 묶음 | 직접 프로젝트 스크립트 확인 안 됨 | `BattleScene` | 특정 씬/디버그용 | 확인 필요 |
| `Hp Canvas.prefab` | 함선 HP UI | `HpViewer` | `BattleScene`, `BattlePrepare` | 재사용 | `Hp Bar` 중첩 |
| `Hp Bar.prefab` | HP 칸 단위 | 없음 | `HpViewer`가 생성 | 재사용 | UI 부품 |
| `Facilities Canvas.prefab` | 시설 UI 묶음 | 없음 직접, 내부 `Facility UI` | `BattleScene` | 재사용 | `Facility UI`, `Power Bar` 중첩 |
| `Facility UI.prefab` | 시설 전력/상태 UI | `FacilityPowerViewer` | `Facilities Canvas`, `BattleScene` | 재사용 | 시설 시스템 참조 필요 |
| `Facility Upgrade UI.prefab` | 상점 시설 업그레이드 UI | `FacilityPowerViewer` | `BattlePrepare` | 특정 씬 가능성 | `GameContext` 기반 표시 가능 |
| `Power Bar Group.prefab` | 전력바 그룹 | 없음 | 여러 UI prefab 내부 | 재사용 | UI 부품 |
| `Power Bar.prefab` | 전력바 단위 | 없음 | Viewer가 생성/표시 | 재사용 | UI 부품 |
| `Crew UI.prefab` | 승무원 전투 UI 묶음 | 없음 직접, 내부 `Crew Summary` | `BattleScene` | 재사용 가능 | `Crew Summary Group` 포함 |
| `Crew Summary.prefab` | 승무원 요약 UI | `CrewViewer` | `BattleScene`, `BattlePrepare`, `Crew UI`, `Crew Heal Group` | 재사용 | 전투/회복 UI 겸용 |
| `Crew Heal Group.prefab` | 회복 UI 묶음 | 없음 직접, 내부 `Crew Summary` | 확인 필요 | 특정 UI 가능성 | 파일 참조만 확인 |

## 5. 씬 정리

| 씬 | 목적 | 핵심 오브젝트 | 주요 스크립트 | 씬 전환/데이터 의존성 |
|---|---|---|---|---|
| `_MainMenu.unity` | 메인 메뉴 | Main Camera, Global Light 2D, EventSystem, `GameManager`, `Main UI` | `MainUIController` | `MainUIController.MoveScene`로 전환 |
| `BattleScene.unity` | 실제 전투 | Player Ship, Enemy Ship, Combat System, SceneBinder, UI, Debug UI | `PlayerController`, `EnemyController`, `CombatSystem`, `SceneBinder`, `WeaponSystem`, `ShieldSystem`, `EngineSystem`, `CrewSystem`, Viewers | `GameManager.OnSceneLoaded`가 `SceneBinder.BindSceneObjects` 호출 |
| `BattlePrepare.unity` | 전투 준비/상점 | ShopManager, Manager Group, UI Group, Upgrade UI Root, Restore UI Root | `ShopManager`, `ReactorViewer` | `GameContext`를 읽어 업그레이드/회복 처리 |
| `GameOver.unity` | 게임오버 화면 | Main Camera, EventSystem, Text | 프로젝트 스크립트 직접 참조 확인 안 됨 | `GameManager.HandleBattleResult`에서 이동 |
| `Test Ground/Shader Test.unity` | 셰이더/비주얼 테스트 | Main Camera, Directional Light | 프로젝트 핵심 스크립트 직접 참조 확인 안 됨 | 테스트 씬으로 보임, 확인 필요 |
| `Settings/Scenes/URP2DSceneTemplate.unity` | URP 2D 템플릿 | 확인 필요 | 확인 필요 | 템플릿/설정용 |
| `zz_External Assets/AllIn1SpriteShader/Demo*.unity` | 외부 셰이더 데모 | 외부 패키지 데모 오브젝트 | 외부 데모 스크립트 | 프로젝트 핵심 흐름과 분리된 외부 에셋 |

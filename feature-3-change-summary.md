# feature-3 작업 정리

## 작업 브랜치

- `feature-3`

## 구현 목표

이번 작업은 BattleScene에 고정 배치된 enemy에 의존하지 않고, Unity Editor에서 등록한 enemy prefab 목록 중 하나를 랜덤으로 스폰하도록 만드는 것이 목표였다.

추가로 전투 중 일정 주기마다 player와 enemy의 랜덤 방에 스테이지 환경 데미지를 적용하는 기능도 추가했다.

## 핵심 구현 내용

- 적 종류 enum `EEnemyType` 추가
- `EEnemyType`과 enemy prefab을 연결하는 `EnemyDatabase` ScriptableObject 추가
- `SceneBinder`에서 enemy prefab 랜덤 선택 및 스폰
- enemy prefab 스폰 후 외부 UI 그룹 주입
  - `Enemy Crew Summary Group`
  - `Enemy Weapon Charge Group`
- `StageEnvironmentController` 추가
- 일정 주기마다 랜덤 방 개수와 랜덤 방을 선택해 환경 데미지 적용
- 환경 데미지는 기존 방 데미지 흐름을 최대한 활용하도록 `ShipController.TakeDamageToRoom(int damage, int crewDamage, int roomId)` 추가

## 변경 파일 목록

### 새로 추가한 파일

- `Assets/_Scripts/_Enum/EEnemyType.cs`
  - enemy prefab을 구분하는 enum

- `Assets/_Scripts/ShipController/EnemyDatabase.cs`
  - Unity Editor에서 enemy prefab 목록을 등록하는 ScriptableObject

- `Assets/_Scripts/StageEnvironmentController.cs`
  - 주기적인 스테이지 환경 효과 컨트롤러

- `feature-3-change-summary.md`
  - 작업 요약 문서

- `feature-3-unity-setup-test-checklist.md`
  - Unity Editor 설정 및 테스트 체크리스트

### 수정한 파일

- `Assets/_Scripts/SceneBinder.cs`
  - enemy 랜덤 스폰
  - enemy UI 그룹 주입
  - 환경 컨트롤러 초기화

- `Assets/_Scripts/ShipController/ShipController.cs`
  - 스폰 후 UI 그룹 주입을 위해 초기화 흐름 일부 분리
  - 환경 데미지용 `TakeDamageToRoom(int damage, int crewDamage, int roomId)` 추가

- `Assets/_Scripts/CombatSystem.cs`
  - 환경 컨트롤러가 현재 player/enemy를 가져올 수 있도록 `GetShip(EShipSide shipSide)` 추가

## Unity Editor에서 직접 주입해야 하는 정적인 요소

이 작업에는 코드만으로 완성되지 않는 요소가 있다.  
Unity Editor에서 직접 asset을 만들고 Inspector에 연결해야 한다.

### 1. EnemyDatabase

가장 중요한 정적 설정 asset이다.

역할:

- 어떤 enemy prefab들이 랜덤 후보인지 관리한다.
- 각 prefab이 어떤 `EEnemyType`인지 Inspector에서 지정한다.
- `SceneBinder`는 이 asset에서 prefab 하나를 랜덤으로 가져온다.

Unity Editor 작업:

1. Project 창에서 우클릭
2. `Create > SO > EnemyDatabase`
3. asset 이름을 `EnemyDatabase`로 변경
4. `Entries` 배열 크기를 `3`으로 설정
5. 아래 prefab들을 연결

| Enemy Type | Prefab |
| --- | --- |
| `Enemy1` | `Assets/_Prefabs/Enemy/Enemy1.prefab` |
| `Enemy1_1` | `Assets/_Prefabs/Enemy/Enemy1 1.prefab` |
| `Enemy1_2` | `Assets/_Prefabs/Enemy/Enemy1 2.prefab` |

주의:

- `EnemyDatabase`를 만들고 끝내면 안 된다.
- 반드시 BattleScene의 `SceneBinder > Enemy Database` 필드에 연결해야 한다.
- 연결하지 않으면 랜덤 스폰 대신 기존 씬 enemy fallback이 동작한다.

### 2. Enemy Spawn Point

역할:

- enemy prefab이 생성될 위치를 정한다.

Unity Editor 작업:

1. BattleScene에서 빈 GameObject 생성
2. 이름을 `Enemy Spawn Point`로 지정
3. 기존 enemy가 있던 위치 근처로 이동
4. `SceneBinder > Enemy Spawn Point`에 연결

### 3. Enemy Spawn Parent

역할:

- 스폰된 enemy가 Hierarchy 어디 아래에 들어갈지 정한다.

추천:

- `Ships` 오브젝트가 있다면 여기에 연결
- 없으면 `Enemy Spawn Group` 같은 빈 GameObject를 만들어 연결

### 4. Enemy UI 그룹

enemy prefab은 아래 UI 그룹을 자체적으로 갖고 있지 않다.  
BattleScene의 UI 그룹을 스폰 이후 주입해야 한다.

`SceneBinder`에 연결할 값:

- `Enemy Crew Summary Group Transform`
- `Enemy Weapon Charge Group Transform`

연결 대상:

- BattleScene의 `Enemy Crew Summary Group`
- BattleScene의 `Enemy Weapon Charge Group`

### 5. StageEnvironmentController

역할:

- 전투 중 일정 주기마다 환경 효과를 발생시킨다.
- player/enemy의 랜덤 방에 소행성 오브젝트를 생성하고 데미지를 준다.

Unity Editor 작업:

1. BattleScene의 `Manager Group` 아래 빈 GameObject 생성
2. 이름을 `Stage Environment Controller`로 지정
3. `StageEnvironmentController` 컴포넌트 추가
4. `SceneBinder > Stage Environment Controller`에 연결

Inspector에서 설정할 값:

- `Effect Interval`
- `Min Room Hit Count`
- `Max Room Hit Count`
- `Room Damage`
- `Crew Damage`
- `Apply To Player`
- `Apply To Enemy`
- `Asteroid Attack Prefab`
- `Asteroid Group Transform`
- `Asteroid Lifetime`

## 적 랜덤 스폰 흐름

1. `GameManager`가 BattleScene 로드 시 `SceneBinder.BindSceneObjects(gameContext)` 호출
2. `SceneBinder`가 `EnemyDatabase.GetRandomPrefab()` 호출
3. 등록된 enemy prefab 중 하나를 랜덤 선택
4. 기존 BattleScene에 배치된 enemy 제거
5. enemy prefab 생성
6. 스폰된 enemy에 외부 UI 그룹 주입
7. `enemy.InitializeShipIfReady()` 호출
8. `CombatSystem.InitCombatSystem(player, enemy)` 호출

## 환경 효과 흐름

1. `SceneBinder`가 `CombatSystem` 바인딩 후 `StageEnvironmentController` 초기화
2. `StageEnvironmentController`가 `Effect Interval`마다 실행
3. player와 enemy 중 설정된 대상에게 적용
4. 각 함선의 방 목록에서 랜덤 방 개수 선택
5. 중복되지 않게 랜덤 방 선택
6. 선택된 방 위치에 `Asteroid Attack Prefab` 생성
7. `ship.TakeDamageToRoom(roomDamage, crewDamage, room.Id)` 호출
8. 기존 방 데미지 흐름으로 시설/승무원/함선 HP 데미지 및 승패 체크 처리

## 확인한 내용

- Unity batchmode import 실행 성공
- `dotnet restore` 성공
- `dotnet build 3rd-2D-Project-MountainHS.sln --no-restore` 성공
- 빌드 오류 0개
- 기존 코드의 미사용 필드 경고 2개만 남음
  - `CrewSystem.isPlayer`
  - `WeaponController.prevIsPowered`

## 꼭 기억할 점

- 이번 작업은 Unity Editor 설정이 들어가야 실제 랜덤 스폰이 보인다.
- 특히 `EnemyDatabase`는 반드시 직접 생성하고 `SceneBinder`에 연결해야 한다.
- 코드상 fallback이 있어서 `EnemyDatabase`가 비어 있어도 전투가 바로 깨지지는 않지만, 이 경우 랜덤 스폰 테스트는 되지 않는다.
- BattlePrepare와 BattleScene을 여러 번 왕복하면서 enemy, UI, 환경 효과가 중복되지 않는지 확인해야 한다.


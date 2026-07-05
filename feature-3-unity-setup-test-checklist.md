# feature-3 Unity 설정 / 테스트 체크리스트

이 문서는 코드 설명보다 Unity Editor에서 직접 무엇을 만들어서 어디에 연결해야 하는지에 초점을 맞춘다.

## 1. Unity에서 직접 설정해야 할 부분

### A. EnemyDatabase 만들기

`EnemyDatabase`는 코드가 자동으로 만들어주는 오브젝트가 아니다.  
Unity Editor에서 직접 만들어서 `SceneBinder`에 넣어줘야 하는 정적인 설정 asset이다.

1. Project 창에서 적당한 폴더 선택
   - 추천 위치: `Assets/_ScriptableObjects`
   - 폴더가 없다면 프로젝트 규칙에 맞춰 새 폴더를 만들어도 된다.
2. 우클릭
3. `Create > SO > EnemyDatabase` 선택
4. asset 이름을 `EnemyDatabase`로 변경
5. Inspector의 `Entries` 배열 크기를 `3`으로 설정
6. 아래처럼 enum과 prefab을 연결

| Enemy Type | Prefab |
| --- | --- |
| `Enemy1` | `Assets/_Prefabs/Enemy/Enemy1.prefab` |
| `Enemy1_1` | `Assets/_Prefabs/Enemy/Enemy1 1.prefab` |
| `Enemy1_2` | `Assets/_Prefabs/Enemy/Enemy1 2.prefab` |

중요:

- `EnemyDatabase`를 만들기만 하면 끝이 아니다.
- 반드시 BattleScene의 `SceneBinder`에 이 asset을 연결해야 랜덤 스폰이 켜진다.
- 연결하지 않으면 기존 BattleScene에 배치된 enemy를 사용하는 fallback이 동작한다.

### B. Enemy Spawn Point 만들기

랜덤 enemy가 생성될 위치를 Unity Editor에서 Transform으로 지정해야 한다.

추천 방식:

1. BattleScene 열기
2. Hierarchy에서 `Ships` 또는 전투 함선 그룹 선택
3. 빈 GameObject 생성
4. 이름을 `Enemy Spawn Point`로 변경
5. 기존 고정 enemy가 있던 위치로 이동
6. 이 Transform을 `SceneBinder > Enemy Spawn Point`에 연결

이 값은 prefab이 어디에 생성될지를 정하는 기준이다.

### C. Enemy Spawn Parent 정하기

스폰된 enemy가 Hierarchy 어디 아래에 들어갈지 정해야 한다.

추천:

- `Ships` 오브젝트가 있다면 `Ships`를 연결
- 별도 그룹을 쓰고 싶다면 `Enemy Spawn Group` 같은 빈 GameObject를 만들고 연결

이 값은 기능 동작보다 Hierarchy 정리에 가깝지만, 연결해두는 편이 확인하기 쉽다.

### D. Enemy UI 그룹 연결

enemy prefab 안에는 아래 두 UI 그룹이 직접 들어있지 않다.  
그래서 BattleScene에 이미 있는 UI 그룹을 `SceneBinder`가 스폰 후 주입한다.

`SceneBinder`에 연결해야 하는 값:

| SceneBinder Field | 연결할 오브젝트 |
| --- | --- |
| `Enemy Crew Summary Group Transform` | BattleScene의 `Enemy Crew Summary Group` |
| `Enemy Weapon Charge Group Transform` | BattleScene의 `Enemy Weapon Charge Group` |

이 연결이 빠지면 enemy 승무원 요약 UI 또는 무기 차지 UI 생성이 실패할 수 있다.

### E. SceneBinder 필드 최종 연결 목록

BattleScene의 `Scene Binder` 오브젝트를 선택하고 아래를 모두 확인한다.

| Field | 반드시 연결? | 설명 |
| --- | --- | --- |
| `Enemy Database` | 예 | 랜덤 enemy prefab 목록 |
| `Enemy Spawn Point` | 예 | enemy 생성 위치 |
| `Enemy Spawn Parent` | 권장 | enemy가 들어갈 Hierarchy 부모 |
| `Enemy Crew Summary Group Transform` | 예 | enemy crew UI가 생성될 부모 |
| `Enemy Weapon Charge Group Transform` | 예 | enemy weapon charge UI가 생성될 부모 |
| `Stage Environment Controller` | 예 | 환경 효과 컨트롤러 |

### F. StageEnvironmentController 추가

`StageEnvironmentController`도 씬에 직접 붙여야 한다.

추천 방식:

1. BattleScene의 `Manager Group` 아래 빈 GameObject 생성
2. 이름을 `Stage Environment Controller`로 변경
3. `StageEnvironmentController` 컴포넌트 추가
4. 이 오브젝트를 `SceneBinder > Stage Environment Controller`에 연결

Inspector에서 설정할 값:

| Field | 추천 테스트 값 | 설명 |
| --- | --- | --- |
| `Effect Interval` | `3` | 테스트 중에는 짧게 설정 |
| `Min Room Hit Count` | `1` | 한 번에 맞을 최소 방 개수 |
| `Max Room Hit Count` | `2` | 한 번에 맞을 최대 방 개수 |
| `Room Damage` | `1` | 방/시설/함선 HP 데미지 |
| `Crew Damage` | `10` | 방 안 승무원 데미지 |
| `Apply To Player` | 체크 | 플레이어에게 적용 |
| `Apply To Enemy` | 체크 | 적에게 적용 |
| `Asteroid Attack Prefab` | 소행성 연출 prefab | 방 위치에 생성될 오브젝트 |
| `Asteroid Group Transform` | 선택 | 생성 오브젝트 정리용 부모 |
| `Asteroid Lifetime` | `1.5` | 소행성 오브젝트 제거 시간 |

### G. 소행성 prefab 연결

`Asteroid Attack Prefab`은 환경 효과가 발생할 때 방 위치에 생성되는 연출 오브젝트다.

확인할 것:

- prefab이 너무 크거나 작지 않은지
- 생성 위치가 방 중심 기준으로 괜찮은지
- `Asteroid Lifetime` 이후 사라지는지
- 별도 collider나 damage script가 붙어 있더라도 현재 환경 데미지는 코드에서 직접 처리한다는 점

## 2. 꼭 테스트해야 할 지점

### A. BattleScene 첫 진입

- Console 에러 없이 시작되는지
- enemy가 정확히 1개만 있는지
- enemy가 `Enemy Spawn Point` 위치에 생성되는지
- enemy prefab이 `EnemyDatabase`에 등록한 3개 중 하나인지
- `Ship initialization failed` 로그가 없는지
- `Enemy Crew Summary Group`, `Enemy Weapon Charge Group` 아래 UI가 생성되는지

### B. BattleScene을 여러 번 로딩할 때 랜덤 적이 나오는지

여러 번 BattleScene에 들어가서 확인한다.

- enemy prefab이 랜덤으로 바뀌는지
- 같은 enemy가 연속으로 나올 수는 있지만, 충분히 반복하면 3종류가 모두 나오는지
- 이전 전투의 enemy가 Hierarchy에 남지 않는지
- 이전 전투의 enemy UI가 남아서 누적되지 않는지

### C. BattlePrepare <-> BattleScene 왕복

반드시 여러 번 왕복 테스트한다.

테스트 흐름:

1. BattlePrepare에서 BattleScene으로 이동
2. 전투 진행
3. 승리 또는 도망 등으로 BattlePrepare 복귀
4. 다시 BattleScene 진입
5. 위 과정을 3회 이상 반복

확인할 것:

- Scene 전환 중 Console 에러가 없는지
- 매번 enemy가 새로 정상 스폰되는지
- `CombatSystem`이 이전 enemy를 계속 참조하지 않는지
- enemy UI가 중복으로 쌓이지 않는지
- player 상태가 BattlePrepare와 BattleScene 사이에서 정상 유지되는지

### D. Enemy UI 테스트

- enemy weapon charge UI가 생성되는지
- enemy crew summary UI가 생성되는지
- enemy weapon charge bar가 실제로 차오르는지
- BattleScene 재진입 시 UI가 중복 생성되지 않는지

### E. Player 공격 테스트

- player 무기를 선택할 수 있는지
- enemy 방 클릭 시 타겟팅이 되는지
- projectile이 enemy 방향으로 날아가는지
- enemy 방/시설/함선 HP에 데미지가 들어가는지
- enemy HP 0 또는 crew 전멸 시 전투 결과가 정상 처리되는지

### F. Enemy 자동 공격 테스트

- enemy가 player 방을 자동으로 선택하는지
- enemy projectile이 player 방향으로 날아오는지
- shield/room damage 처리가 기존처럼 동작하는지
- enemy가 스폰된 뒤에도 `RandomTargeter`가 player rooms를 정상 참조하는지

### G. 환경 효과 테스트

테스트 중에는 `Effect Interval`을 `3` 정도로 짧게 설정하는 것을 추천한다.

확인할 것:

- 일정 주기마다 소행성 오브젝트가 생성되는지
- 소행성 오브젝트가 랜덤 방 위치에 생성되는지
- 한 번에 맞는 방 개수가 `Min/Max Room Hit Count` 범위 안인지
- 같은 주기 안에서 같은 함선의 같은 방이 중복 선택되지 않는지
- `Apply To Player`, `Apply To Enemy` 체크 상태에 따라 적용 대상이 바뀌는지
- `Asteroid Lifetime` 이후 소행성 오브젝트가 제거되는지

### H. 환경 데미지 결과 테스트

- 랜덤 방의 시설 데미지가 증가하는지
- 해당 방에 승무원이 있으면 crew damage가 적용되는지
- 함선 HP가 감소하는지
- 방 흔들림 연출이 나오는지
- 환경 데미지로 함선 HP가 0이 되면 전투 결과가 정상 처리되는지
- 환경 데미지로 모든 승무원이 죽으면 전투 결과가 정상 처리되는지

### I. Scene 전환 타이밍과 환경 효과

환경 효과 코루틴과 Scene 이동 타이밍이 겹칠 수 있으므로 확인한다.

- BattleScene에서 BattlePrepare로 넘어갈 때 환경 효과 관련 에러가 없는지
- 다시 BattleScene에 들어왔을 때 환경 효과가 한 번만 실행되는지
- 이전 BattleScene의 환경 효과가 남아서 중복 실행되지 않는지

### J. 일부 연결 누락 테스트

의도적으로 일부 연결을 빼고 Console 로그를 확인하면 문제 원인을 빨리 파악할 수 있다.

| 빼볼 항목 | 기대 결과 |
| --- | --- |
| `Enemy Database` | 기존 씬 enemy fallback 사용 |
| `Asteroid Attack Prefab` | 환경 데미지는 적용, prefab 누락 에러 표시 |
| `Stage Environment Controller` | 환경 효과만 비활성화, 전투 자체는 진행 |

## 빠른 최종 확인 순서

1. `EnemyDatabase` asset 생성 및 prefab 3개 연결
2. `SceneBinder` 필드 전체 연결
3. `StageEnvironmentController` 추가 및 연결
4. BattleScene 시작 시 enemy 1개만 랜덤 스폰
5. enemy weapon/crew UI 정상 생성
6. player 공격 정상
7. enemy 자동 공격 정상
8. 환경 효과 주기적 발생
9. 환경 데미지로 방/승무원/함선 HP 변화
10. BattlePrepare와 BattleScene 왕복 3회 이상
11. UI, enemy, 환경 효과 중복 없음
12. 승리/패배 결과 처리 정상


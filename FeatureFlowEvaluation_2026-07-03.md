# 내일 구현 전 기능 흐름 평가

작성일: 2026-07-03  
범위: 코드 수정 없이 현재 프로젝트 구조를 읽고 평가만 수행

## 1. 전체 평가

- 전체 흐름은 현재 프로젝트 구조에 대체로 맞는다.
- 특히 무기별 필요 전력은 `WeaponSystem`이 이미 `weapons` 리스트를 들고 있고 매 프레임 `WeaponController.IsPowered`를 갱신하므로, 가장 작은 변경으로 구현할 수 있다.
- 투사체 회피와 피격 이펙트도 `ProjectileController`와 `CombatSystem` 사이의 기존 흐름 안에서 처리 가능하다.
- 단, 회피 기능은 현재 생각한 흐름 중 한 부분을 반드시 바꾸는 것이 안전하다. "빗나간 투사체를 파괴하지 않는다"는 방식은 현재 이동/도착 구조에서는 중복 판정 또는 계속 도착 상태가 되는 문제를 만들 가능성이 높다.
- 현재 `CombatSystem.HandleRoomCollision(ProjectileController projectile)`는 이미 `bool`을 반환한다. 따라서 시그니처를 새로 bool로 바꾸는 문제라기보다, `ProjectileController`가 반환값을 사용하도록 바꾸는 문제가 핵심이다.
- 대규모 리팩토링이나 새 클래스 생성은 필요 없어 보인다.

구조적으로 가장 어색한 부분:

- 회피율을 `ProjectileController`에 두는 설계는 "투사체가 회피한다"처럼 읽힐 수 있다.
- 게임 의미상 회피는 목표 함선의 엔진/상태에서 계산되는 것이 더 자연스럽다.
- 하지만 현재 코드에는 엔진 회피율 개념이 없고, 내일 최소 구현을 목표로 한다면 `ProjectileController`에 임시 명중/회피 관련 SerializeField를 두는 방식도 가능하다.
- 장기적으로는 `EngineSystem` 또는 목표 `ShipController` 쪽에서 회피율을 계산하고, `CombatSystem`이 그 값을 사용해 판정하는 흐름이 더 맞다.

## 2. 기능별 평가

### 2-1. 무기별 필요 전력

현재 설계의 장점:

- `WeaponSystem`이 이미 `weapons` 리스트를 관리한다.
- `WeaponSystem.UpdateWeaponActivation()`이 모든 무기의 `IsPowered`를 매 프레임 갱신하므로, 이 메소드 내부만 바꿔도 기능 구현이 가능하다.
- `weapons` 리스트 index 순서가 이미 UI 버튼 index와 연결된다.
- `WeaponController`는 무기 프리팹에 붙는 컴포넌트이므로, 무기별 `requiredPower`를 SerializeField로 두면 프리팹 단위 설정이 쉽다.

위험한 지점:

- 현재 `WeaponController`는 `IsPowered == false`가 되면 `isReady = false`, `chargeProgress = 0f`로 초기화한다.
- 따라서 전력이 부족해져 비활성화된 무기는 충전도가 즉시 사라진다. 이것은 기존 동작과 일관되지만, "전력 부족이면 충전 일시정지"를 원한다면 별도 설계가 필요하다.
- `WeaponSystem.UpdateWeaponTargeting()`은 적 무기에서 `weapon.IsPowered`인 무기만 타겟을 잡는다. 무기별 전력 적용 후에도 이 흐름은 자연스럽다.
- 플레이어는 선택된 무기가 전력 부족 상태여도 타겟을 지정할 수 있다. 현재 구조에서도 전력 여부를 선택/타겟 지정에서 막고 있지는 않다.
- `SetSelectedWeapon(int index)`의 범위 체크가 `index > weapons.Count`라서 `index == weapons.Count`일 때 예외가 날 수 있다. 이번 기능의 직접 범위는 아니지만, index 기반 구현을 할 때 같이 눈에 띄는 위험이다.
- `requiredPower` 값이 0 이하이면 무기가 공짜로 켜지는 문제가 생길 수 있다. 기본값은 1로 두고, 잘못된 값은 Unity에서 바로 보이게 로그 또는 Assert가 필요하다.

더 나은 대안:

- 현재 코드에는 `WeaponData` 또는 `WeaponState`가 없다. `WeaponDatabase`는 `EWeaponType`과 프리팹 매핑만 한다.
- 지금 시점에서 `WeaponData`를 새로 만드는 것은 과하다.
- `WeaponController`에 `[SerializeField] private int requiredPower = 1;`와 읽기 전용 프로퍼티를 추가하는 것이 가장 작고 안전하다.

최소 수정 추천 방향:

- `WeaponController`에 `RequiredPower` 읽기 프로퍼티와 `requiredPower` SerializeField 추가.
- `WeaponSystem.UpdateWeaponActivation()`에서 `Power`를 남은 전력으로 사용한다.
- `weapons`를 0번부터 순회하며 `remainingPower >= weapon.RequiredPower`이면 해당 무기만 활성화하고 남은 전력을 차감한다.
- 부족하면 해당 무기는 비활성화한다.
- 함수 시그니처 변경은 필요 없다.

### 2-2. 회피 판정

현재 설계의 장점:

- `CombatSystem.HandleRoomCollision()`은 이미 투사체가 방에 도착했을 때 호출되는 중앙 판정 지점이다.
- 현재도 `bool`을 반환하고 있으므로, 명중/빗나감 결과를 반환하는 구조 자체는 이미 맞춰져 있다.
- `ProjectileController`가 충돌 후 결과에 따라 자기 파괴 여부를 결정하는 것도 책임 분리가 크게 어색하지 않다.

위험한 지점:

- 현재 `ProjectileController.Update()`는 `IsArrived`이면 `HandleRoomCollision(this)`를 호출한 뒤 무조건 `Destroy(gameObject)` 한다.
- 회피 후 파괴하지 않도록 바꾸면, `transform.position == Target` 상태가 유지되는 동안 다음 프레임에도 계속 `IsArrived == true`가 되어 중복 판정될 가능성이 높다.
- 현재 `IsArrived`는 `transform.position == Target`의 정확 비교다. `MoveTowards`가 목표점에 정확히 도달시키므로 작동은 하지만, 도착 후 유지시키면 반복 호출에 취약하다.
- `StartCol`이 null인 상태로 `HandleRoomCollision()` 또는 `HandleShieldCollision()`이 호출되면 `projectile.StartCol.CompareTag(...)`에서 null 문제가 날 수 있다. 정상 경로에서는 StartCol을 지나간 뒤 이동하지만, 프리팹/레이어 세팅이 틀리면 바로 드러난다.
- 목표 방이 null인 경우 `ShipController.TakeDamageToRoom()`에서 `room.TakeDamageToCrews(crewDamage)`를 먼저 호출하므로 null 예외가 난다. 현재는 방 ID가 정상이라는 전제에 기대고 있다.

더 나은 대안:

- "빗나가면 투사체를 유지"보다는 "빗나가도 투사체는 소모되고, 데미지만 적용하지 않는다"가 현재 구조에서는 훨씬 안전하다.
- 빗나간 투사체를 계속 날아가게 하고 싶다면, 단순 bool 반환보다 `ProjectileController`에 miss 상태, 새 목표점, 충돌 무시 시간, 중복 판정 방지 플래그 등이 필요해져 범위가 커진다.
- 회피율의 출처는 장기적으로 목표 함선의 `EngineSystem.Power` 또는 `EngineSystem`의 별도 회피율 프로퍼티가 더 자연스럽다.
- 단기 최소 구현은 `CombatSystem.HandleRoomCollision()` 안에서 목표 함선의 엔진 시스템을 조회해 회피 확률을 계산하는 방식이 낫다.

최소 수정 추천 방향:

- `CombatSystem.HandleRoomCollision()` 내부에서 회피 판정을 먼저 수행한다.
- 빗나감이면 데미지를 주지 않고 `false` 반환.
- 명중이면 기존 `TakeDamageToRoom()` 호출 후 `true` 반환.
- `ProjectileController.Update()`에서는 반환값을 변수로 받되, 명중/빗나감과 관계없이 일단 투사체는 파괴하는 것을 추천한다.
- "빗나간 투사체 유지"는 내일 최소 구현 범위에서는 보류하는 것이 안전하다.

### 2-3. 투사체 피격 이펙트

현재 설계의 장점:

- 방 도착 판정은 `ProjectileController`가 알고 있고, 투사체 자신도 파괴하므로 이펙트 생성 위치를 잡기 쉽다.
- 프로젝트 규모상 `ProjectileController`에 `hitEffectPrefab` SerializeField를 직접 두는 것은 과하지 않다.
- 투사체 프리팹별로 다른 이펙트를 넣을 수 있어 무기별 연출 확장이 쉽다.

위험한 지점:

- 이펙트 생성 전에 `Destroy(gameObject)`를 먼저 호출하면 현재 프레임에서는 대체로 참조가 남아 있더라도, 코드 순서상 실수하기 쉽다.
- `hitEffectPrefab` 주입을 빼먹으면 명중 시 이펙트가 나오지 않는다. 이 경우 조용히 무시할지, 경고를 남길지 정해야 한다.
- 충돌 위치를 방 중심으로 하면 타격감은 약할 수 있고, 투사체 위치로 하면 현재 목표점 기준에서는 자연스럽다.
- 실제 물리 충돌 지점 정보는 현재 방 도착 흐름에서 직접 전달되지 않는다. `OnTriggerEnter2D`의 ContactPoint를 쓰는 구조가 아니므로 "정확한 충돌 지점" 구현은 범위가 커진다.

더 나은 대안:

- 현재 구조에서는 이펙트 위치를 `projectile.transform.position` 또는 `projectile.Target`으로 잡는 것이 가장 작고 안전하다.
- `ProjectileController`가 생성 담당을 해도 된다. 별도 EffectManager는 지금은 과하다.
- 명중 이펙트만 목표라면 `CombatSystem`보다 `ProjectileController`가 `isHit == true`일 때 생성하는 편이 흐름을 읽기 쉽다.

최소 수정 추천 방향:

- `ProjectileController`에 `[SerializeField] private GameObject hitEffectPrefab;` 추가.
- `HandleRoomCollision()` 반환값이 true일 때 `Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);` 실행.
- 이펙트 생성 후 `Destroy(gameObject)`.
- `hitEffectPrefab == null`이면 경고를 남기고 투사체는 정상 파괴하는 방향이 안전하다.

## 3. 수정 예상 지점

| 기능 | 수정 예상 클래스 | 수정 예상 메소드/필드 | 변경 이유 |
|---|---|---|---|
| 무기별 필요 전력 | `WeaponController` | `RequiredPower` 프로퍼티, `requiredPower` SerializeField | 무기 프리팹별 필요 전력 설정 |
| 무기별 필요 전력 | `WeaponSystem` | `UpdateWeaponActivation()` | 시설에 할당된 `Power`를 weapons index 순서로 분배 |
| 무기별 필요 전력 | `WeaponSystem` | `SetSelectedWeapon(int index)` | 직접 기능은 아니지만 index 기반 안정성 때문에 `index >= weapons.Count` 확인 필요 |
| 회피 판정 | `CombatSystem` | `HandleRoomCollision(ProjectileController projectile)` | 명중 전 회피 판정 후 true/false 반환 |
| 회피 판정 | `ProjectileController` | `Update()`의 `IsArrived` 블록 | `HandleRoomCollision()` 반환값을 사용 |
| 회피 판정 | `EngineSystem` | 선택 사항: 회피율 프로퍼티 또는 계산 메소드 | 회피율을 목표 함선 엔진 전력 기반으로 둘 경우 |
| 피격 이펙트 | `ProjectileController` | `hitEffectPrefab` SerializeField | 투사체 프리팹별 명중 이펙트 주입 |
| 피격 이펙트 | `ProjectileController` | `Update()`의 도착 처리 | 명중 시 이펙트 생성 후 투사체 파괴 |

## 4. 호출부 영향

함수 시그니처 변경이 필요한 메소드:

- 필수 변경 없음.
- `CombatSystem.HandleRoomCollision(ProjectileController projectile)`는 이미 `bool` 반환이다.
- `WeaponSystem.UpdateWeaponActivation()`은 private 메소드라 외부 호출부 영향이 없다.
- `WeaponController`에 `RequiredPower` 읽기 프로퍼티를 추가해도 기존 호출부는 깨지지 않는다.

같이 수정해야 할 호출부:

- `ProjectileController.Update()`:
  - 현재는 `CombatSystem.Instance.HandleRoomCollision(this);` 반환값을 무시한다.
  - 내일 구현 시 `bool isHit = CombatSystem.Instance.HandleRoomCollision(this);`처럼 결과를 받아야 한다.
- `ShipController.InitWeapons()`:
  - 직접 수정은 필요 없을 가능성이 높다.
  - 다만 생성된 무기 프리팹의 `WeaponController.requiredPower` 값이 Inspector에 들어 있어야 한다.
- `WeaponSystem.UpdateWeaponTargeting()`:
  - 직접 수정은 필요 없을 가능성이 높다.
  - 무기별 활성화 이후 `IsPowered`인 무기만 타겟을 받는 기존 흐름이 그대로 동작한다.

기존 동작이 깨질 가능성이 있는 지점:

- 무기 전력 부족 시 충전도가 0으로 초기화된다. 기존 코드와 일관되지만, 체감상 "충전이 사라지는 버그"로 보일 수 있다.
- `requiredPower`를 2 이상으로 설정하면 기존 적 함선의 시작 무기 활성 개수가 달라진다. 현재 `EnemyPowerController`는 무기에 2 전력을 할당한다.
- 플레이어 숫자키 `Digit5`, `Digit6`는 `SetSelectedWeapon(1)`, `SetSelectedWeapon(2)`를 호출한다. 무기 수가 부족하면 기존 코드에서 예외 위험이 있다.
- 회피 시 투사체를 파괴하지 않으면 `IsArrived` 반복으로 방 피해 판정이 계속 발생할 수 있다.
- 명중 이펙트 프리팹에 자동 파괴 로직이 없으면 씬에 이펙트 오브젝트가 계속 남을 수 있다.

## 5. 실수하기 쉬운 지점

전력 부족 무기 처리:

- `Power > 0`이면 모든 무기를 켜던 기존 코드를 그대로 두면 기능이 구현되지 않는다.
- 반드시 `Power`를 `remainingPower`로 복사한 뒤 index 순서대로 차감해야 한다.
- `requiredPower`가 0 이하인 프리팹은 반드시 잡아야 한다.

충전도 유지/감소/초기화 처리:

- 현재 무기는 전력이 꺼지는 순간 충전도가 0이 된다.
- 내일 구현에서 이 동작을 바꾸지 않는 것이 최소 변경이다.
- 충전 유지나 서서히 감소는 별도 기능으로 봐야 한다.

회피 판정 후 투사체 중복 충돌:

- "빗나감이면 투사체 유지"는 현재 구조에서 가장 위험하다.
- 유지하려면 도착 판정 재호출 방지 플래그, 새 이동 목표, 충돌 무시 처리 등이 필요하다.
- 내일은 빗나가도 투사체를 파괴하는 쪽을 추천한다.

이펙트 생성 위치:

- 정확한 충돌 지점은 현재 코드에서 얻고 있지 않다.
- 최소 구현은 `transform.position` 또는 `Target` 사용.
- 현재 `IsArrived` 시점에는 `transform.position == Target`이므로 둘은 사실상 같다.

Unity 인스펙터 주입 누락:

- 새로 추가한 `WeaponController.requiredPower`는 모든 무기 프리팹에서 확인해야 한다.
- 새로 추가한 `ProjectileController.hitEffectPrefab`은 모든 투사체 프리팹에서 확인해야 한다.
- 이펙트 프리팹에 자동 파괴 스크립트나 ParticleSystem stop action이 없으면 오브젝트가 누적될 수 있다.

기타 현재 코드 기준 위험 요소:

- `ShipController.TakeDamageToRoom()`은 `room == null` 검사 전에 `room.TakeDamageToCrews(crewDamage)`를 호출한다.
- `CombatSystem.MoveToEnemyArea()`는 딕셔너리에 collider 키가 반드시 있다고 가정한다.
- `ProjectileController.StartCol`이 세팅되지 않은 상태에서 충돌 판정이 들어오면 `CompareTag`에서 문제가 날 수 있다.
- `WeaponViewer.LateUpdate()`는 `weaponChargeBar`가 세팅된다는 전제에 기대고 있다.

## 6. 권장 구현 순서

1. 무기별 필요 전력부터 구현한다.
   - `WeaponController`에 `requiredPower`와 읽기 프로퍼티만 추가한다.
   - `WeaponSystem.UpdateWeaponActivation()`만 최소 수정한다.
   - 확인 기준: 전력 1/4에서 예시와 같은 무기 활성 결과가 나오는지 Inspector로 확인한다.

2. 전력 부족 시 충전 처리 확인.
   - 전력 부족으로 꺼진 무기의 `chargeProgress`가 0이 되는 것이 의도와 맞는지 먼저 확인한다.
   - 의도와 다르면 그때 별도 작업으로 분리한다.

3. 회피 판정을 `CombatSystem.HandleRoomCollision()`에 추가한다.
   - 우선 회피율 출처를 하나로 정한다.
   - 최소 구현이면 임시 고정값 또는 목표 함선 엔진 기반 계산 중 하나를 선택한다.
   - 확인 기준: miss일 때 `TakeDamageToRoom()`이 호출되지 않고 false가 반환되는지 확인한다.

4. `ProjectileController.Update()`에서 room collision 반환값을 사용한다.
   - 명중이면 이펙트 생성 후보.
   - 빗나감이어도 현재 구조에서는 투사체 파괴를 추천한다.
   - 확인 기준: miss가 반복 데미지로 이어지지 않는지 확인한다.

5. 명중 이펙트를 추가한다.
   - `ProjectileController`에 `hitEffectPrefab` 필드를 추가한다.
   - `isHit == true`일 때만 이펙트를 생성한다.
   - 이펙트 생성 후 투사체 파괴 순서로 둔다.

6. Unity Inspector 세팅 후 실제 씬에서 확인한다.
   - 플레이어 무기, 적 무기, 실드 충돌, 방 명중, 회피, 이펙트 생성 순서로 테스트한다.

## 7. 내가 Unity에서 직접 해야 할 작업

인스펙터에 주입해야 할 필드:

- 각 무기 프리팹의 `WeaponController.requiredPower`
  - 예: 0번 무기 1, 1번 무기 2, 2번 무기 3
- 각 투사체 프리팹의 `ProjectileController.hitEffectPrefab`
- 회피율을 투사체에 둘 경우, 각 투사체 프리팹의 회피/명중 관련 값
- 회피율을 엔진에 둘 경우, 플레이어/적 함선의 `EngineSystem`에 추가된 회피 관련 값

프리팹에 추가해야 할 컴포넌트/필드:

- 명중 이펙트 프리팹
  - ParticleSystem 또는 Animator 기반 이펙트
  - 자동 Destroy 방식 필요
  - ParticleSystem을 쓴다면 Stop Action을 Destroy로 설정하거나, 별도 자동 파괴 컴포넌트가 필요하다.
- 투사체 프리팹
  - `hitEffectPrefab` 연결
  - 기존 `Damage`, `CrewDamage`, `shipLayer`, `shieldLayer` 유지 확인
- 무기 프리팹
  - `requiredPower` 값 확인
  - 기존 `projectilePrefab` 연결 유지 확인

테스트해야 할 씬 상황:

- 무기 시스템 전력 0: 모든 무기 비활성, 충전 0
- 무기 시스템 전력 1: 필요 전력 1짜리 첫 무기만 활성
- 무기 시스템 전력 2: 첫 무기가 1이면 남은 1로 필요 전력 2 무기는 비활성
- 무기 시스템 전력 3: 필요 전력 1, 2 무기 활성
- 무기 시스템 전력 4: 필요 전력 1, 2 활성, 필요 전력 3 비활성
- 적 함선 시작 전력 2에서 적 무기가 몇 개 활성화되는지 확인
- 실드가 있을 때 투사체가 방 판정보다 먼저 실드에 막히는지 확인
- 회피 성공 시 HP, 시설, 승무원 피해가 들어가지 않는지 확인
- 회피 성공 시 투사체가 중복 판정을 만들지 않는지 확인
- 명중 성공 시 이펙트가 한 번만 생성되고 투사체가 파괴되는지 확인
- 이펙트 프리팹이 일정 시간 후 사라지는지 확인

## 8. 최종 결론

지금 설계대로 가도 되는 부분:

- 무기별 필요 전력은 지금 설계대로 가도 된다.
- `requiredPower`는 현재 구조에서는 `WeaponController`에 두는 것이 가장 작고 안전하다.
- index 순서 기반 전력 할당도 현재 `weapons` 리스트와 UI index 구조를 고려하면 괜찮다.
- 피격 이펙트를 `ProjectileController`가 직접 생성하는 것도 현재 프로젝트 규모에서는 적절하다.

반드시 바꾸는 것이 좋은 부분:

- 회피 성공 시 투사체를 파괴하지 않는 설계는 내일 최소 구현에서는 피하는 것이 좋다.
- `CombatSystem.HandleRoomCollision()`은 true/false를 반환하되, `ProjectileController`는 그 결과를 사용하고 투사체는 명중/빗나감과 관계없이 파괴하는 흐름을 추천한다.
- 회피율은 가능하면 투사체보다 목표 함선의 엔진/상태 기반으로 계산하는 편이 자연스럽다.

시간이 부족할 때 우선 구현해야 할 최소 범위:

1. `WeaponController.requiredPower` 추가
2. `WeaponSystem.UpdateWeaponActivation()`의 순차 전력 분배 구현
3. `CombatSystem.HandleRoomCollision()`에 간단한 회피 판정 추가
4. `ProjectileController.Update()`에서 반환값 사용
5. 명중 시 `hitEffectPrefab` 생성

내일 구현 시 가장 중요한 판단:

- 회피율 출처를 먼저 정해야 한다.
- "빗나간 투사체 유지"는 나중에 별도 기능으로 분리하는 것이 안전하다.
- 충전도 초기화 정책은 기존 동작을 유지할지 먼저 확정해야 한다.

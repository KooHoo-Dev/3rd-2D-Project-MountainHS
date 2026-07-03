# 카메라 흔들림 / 커스텀 커서 구현 조사

작성일: 2026-07-03  
범위: 코드 수정 없이 현재 프로젝트 구조 기준으로 구현 방법만 정리

## 1. 전체 평가

- 두 기능 모두 현재 프로젝트에 추가 가능하다.
- 피격 시 카메라 흔들림은 `CombatSystem.HandleRoomCollision()`에서 방 피해가 실제로 들어간 직후 트리거하는 흐름이 가장 자연스럽다.
- 커서 변경은 Unity의 `Cursor.SetCursor(Texture2D texture, Vector2 hotspot, CursorMode cursorMode)`를 쓰는 방식이 가장 작다.
- 단, 커서 API는 `Sprite`를 직접 받지 않고 `Texture2D`를 받는다. "내가 지정한 스프라이트"를 그대로 쓰려면 Import 설정 또는 변환 방식에 주의가 필요하다.
- 현재 프로젝트에는 카메라 흔들림 전용 코드와 커서 전용 코드가 없다.
- `Main Camera.prefab`에는 `Camera`, `AudioListener`, URP Additional Camera Data, `AspectRatioController`가 붙어 있다.
- `AspectRatioController`는 카메라 viewport rect만 바꾸고, Transform 위치는 건드리지 않는다. 따라서 Transform 위치를 흔드는 방식과 충돌 가능성은 낮다.

가장 중요한 제약:

- 내일 실제 구현에서 새 파일/새 클래스 생성이 허용되지 않으면 `CameraShakeController`, `CursorController` 같은 깔끔한 전용 컴포넌트를 만들 수 없다.
- 새 파일/새 클래스 생성을 허용하면 두 기능은 각각 작은 MonoBehaviour 1개씩으로 분리하는 것이 가장 읽기 쉽다.
- 새 파일/새 클래스 생성을 허용하지 않으면 기존 파일에 넣어야 한다. 이 경우 카메라 흔들림은 `AspectRatioController`에 넣는 것이 최소 변경이고, 커서는 `MainUIController` 또는 `GameManager`에 넣는 방식이 후보지만 책임이 섞인다.

## 2. 피격 시 카메라 흔들림

### 현재 구조에서 연결 지점

현재 피격 흐름:

1. `ProjectileController.Update()`
2. 투사체가 `Target`에 도착하면 `CombatSystem.Instance.HandleRoomCollision(this)` 호출
3. `CombatSystem.HandleRoomCollision()`에서 공격자/피격자 판별
4. `ShipController.TakeDamageToRoom(projectile, roomId)` 호출
5. 방, 시설, 승무원, 함선 HP 피해 적용

추천 트리거 위치:

- 명중 판정이 확정되고 `TakeDamageToRoom()`이 호출된 직후.
- 즉 `CombatSystem.HandleRoomCollision()` 내부에서 피해 적용 후 카메라 흔들림을 호출하는 방식.

피해야 할 위치:

- `ProjectileController`에서 무조건 흔들기.
  - 실드에 막힌 경우, 회피한 경우, 방 명중한 경우를 구분하기 어려워질 수 있다.
- `ShipController.TakeDamage()` 안에서 흔들기.
  - 함선 HP가 줄어드는 모든 상황에서 흔들릴 수 있어 범위가 넓어진다.
- `Room.TakeDamageToFacility()` 안에서 흔들기.
  - 시설 없는 방 피격, 승무원 피해, 함선 HP 피해와 분리되어 일관성이 떨어진다.

### 구현 방식 후보

#### A안: 새 `CameraShakeController` 생성

가장 추천하는 방식이다. 단, 내일 작업 지시에 "새 파일을 생성해도 됨", "필요하다면 새 클래스를 생성해도 됨"이 있어야 한다.

예상 구조:

- `CameraShakeController : MonoBehaviour`
- `Main Camera.prefab`에 컴포넌트 추가
- SerializeField:
  - `duration`
  - `strength`
  - `frequency` 또는 `shakeInterval`
- public 메소드:
  - `Shake()`
  - 또는 `Shake(float duration, float strength)`

장점:

- 카메라 흔들림 책임이 명확하다.
- `AspectRatioController`와 섞이지 않는다.
- 나중에 피격 강도별 흔들림을 추가하기 쉽다.

위험:

- 새 파일/새 클래스가 필요하다.
- 호출을 위해 `CombatSystem`에서 참조를 찾아야 한다.
- Singleton으로 만들면 편하지만, 이 기능 하나 때문에 새 Singleton 구조를 추가하는 것은 과할 수 있다.

최소 호출 방식:

- `CombatSystem`에 `[SerializeField] private CameraShakeController cameraShake;`를 두고 Inspector에서 연결.
- 또는 `InitCombatSystem()` 때 `FindFirstObjectByType<CameraShakeController>()`로 찾기.

이 프로젝트 기준 추천:

- Inspector 연결보다 `FindFirstObjectByType<CameraShakeController>()`가 초기 연결 실수를 줄일 수 있다.
- 다만 피격 때마다 Find를 호출하지 말고, 초기화 때 한 번만 캐싱하는 편이 좋다.

#### B안: `AspectRatioController`에 흔들림 기능 추가

새 클래스 생성이 허용되지 않을 때의 최소 변경 대안이다.

장점:

- 새 파일/새 클래스가 필요 없다.
- 이미 `Main Camera.prefab`에 붙어 있다.
- 카메라 Transform을 직접 다루기 쉽다.

위험:

- 원래 역할은 화면 비율 보정인데, 흔들림까지 담당하게 되어 책임이 섞인다.
- 외부에서 호출하려면 public 메소드를 추가해야 할 가능성이 높다.
- 내일 작업에서 public 메소드 추가 허용 여부를 명확히 해야 한다.

추천도:

- 빠른 포트폴리오 구현이면 가능.
- 장기 유지 관점에서는 A안이 더 낫다.

### 흔들림 방식

가장 단순한 방식:

- 코루틴 시작 시 원래 위치 저장.
- duration 동안 매 프레임 랜덤 offset 적용.
- 끝나면 원래 위치로 복구.

주의점:

- 카메라 기본 z값은 `-10`이다. 흔들림은 x, y만 바꾸고 z는 유지해야 한다.
- 여러 번 피격될 때 코루틴이 중복 실행되면 카메라 위치가 꼬일 수 있다.
- 중복 피격 시 기존 shake 코루틴을 멈추고 새로 시작하거나, 남은 시간/강도만 갱신하는 단순 정책이 필요하다.
- Time.timeScale이 0일 때도 흔들릴 필요가 있으면 `Time.unscaledDeltaTime`을 써야 한다. 현재 전투 중 피격 연출이면 `Time.deltaTime`으로 충분해 보인다.

추천 정책:

- 내일 최소 구현은 "새 흔들림 요청이 오면 기존 흔들림 코루틴을 중지하고 새로 시작"이 가장 단순하다.
- 카메라 위치 복구는 반드시 마지막에 수행한다.
- 전투 결과 UI가 뜨거나 씬 전환 중에는 흔들림이 남지 않도록 OnDisable에서 원래 위치 복구를 고려한다.

### 호출 타이밍

추천:

- 회피 판정까지 추가한다면 `CombatSystem.HandleRoomCollision()`에서 명중일 때만 흔들기.
- 실드 피격도 흔들고 싶다면 `HandleShieldCollision()`에서 실드가 실제로 피해를 받았을 때 약한 흔들림을 별도로 호출할 수 있다.

최소 범위:

- 방 명중 시에만 흔들기.
- 실드 피격 흔들림은 나중에 별도 작업으로 분리.

## 3. 커서를 지정한 스프라이트로 바꾸기

### Unity 기본 방식

Unity 기본 커서 변경은 `Cursor.SetCursor()`를 사용한다.

필요한 값:

- `Texture2D cursorTexture`
- `Vector2 hotspot`
- `CursorMode.Auto`

핫스팟 의미:

- 커서 이미지 안에서 실제 클릭 지점으로 사용할 픽셀 좌표다.
- 일반 화살표 커서는 보통 좌상단 근처가 클릭 지점이다.
- 조준점 커서는 보통 이미지 중앙이 클릭 지점이다.

예시 기준:

- 32x32 조준점이면 hotspot은 `(16, 16)`.
- 32x32 화살표면 hotspot은 `(0, 0)` 또는 실제 끝점 좌표.

### Sprite를 바로 쓰면 생기는 문제

중요:

- `Cursor.SetCursor()`는 `Sprite`가 아니라 `Texture2D`를 받는다.
- `sprite.texture`를 넘길 수는 있지만, Sprite가 큰 atlas 또는 sliced texture의 일부라면 전체 텍스처가 커서로 들어갈 수 있다.
- 현재 프로젝트의 `Assets/zz_External Assets/ui/FTL_raw/PC _ Computer - FTL_ Faster Than Light - Miscellaneous - Cursors.png`는 32x32 단위로 여러 개 slice된 Sprite Mode Multiple 텍스처다.
- 이 파일의 meta 기준 `isReadable: 0`이다. 런타임에서 특정 Sprite rect만 잘라 Texture2D를 만들려면 Read/Write Enabled가 필요할 수 있다.

가장 안전한 방식:

- 사용하고 싶은 커서 스프라이트를 별도 32x32 또는 64x64 PNG 파일로 분리한다.
- Texture Type을 Cursor 또는 Default/Texture2D로 사용할 수 있게 설정한다.
- 그 PNG를 `Texture2D` 필드에 직접 연결한다.

사용자가 꼭 Sprite를 지정하고 싶다면:

- 코드 필드는 `Sprite cursorSprite`로 받을 수 있다.
- 단, Sprite가 단일 이미지일 때만 `cursorSprite.texture`를 그대로 쓰는 방식이 안전하다.
- Multiple Sprite atlas의 일부라면 전체 atlas 문제가 생길 수 있으므로 추천하지 않는다.

대안:

- OS 커서를 바꾸지 않고, Unity UI Image를 마우스 위치에 따라 움직이는 "소프트웨어 커서"를 만든다.
- 이 방식은 Sprite를 그대로 쓸 수 있고 애니메이션도 쉽다.
- 대신 `Cursor.visible = false` 처리와 Canvas 정렬, UI 클릭 방해 여부를 신경 써야 한다.

### 구현 방식 후보

#### A안: 새 `CursorController` 생성

가장 추천하는 방식이다. 단, 새 파일/새 클래스 허용이 필요하다.

예상 구조:

- `CursorController : MonoBehaviour`
- 시작 씬 또는 공통 UI 오브젝트에 붙임
- SerializeField:
  - `Texture2D defaultCursor`
  - `Vector2 defaultHotspot`
  - 필요하면 `Texture2D targetingCursor`, `Vector2 targetingHotspot`
- `Start()`에서 기본 커서 적용
- 필요한 시점에 `SetDefaultCursor()`, `SetTargetingCursor()` 호출

장점:

- 책임이 명확하다.
- 나중에 무기 선택 중 조준 커서, 일반 커서, 이동 커서 등으로 확장하기 쉽다.
- `MainUIController`나 `GameManager`에 커서 세부 설정이 섞이지 않는다.

위험:

- 새 파일/새 클래스 필요.
- 씬마다 오브젝트가 있어야 하는지, `DontDestroyOnLoad`로 유지할지 결정해야 한다.

추천:

- 처음에는 BattleScene에서만 적용한다면 BattleScene UI 오브젝트에 붙이는 것으로 충분하다.
- 모든 씬에 같은 커서를 쓰고 싶다면 GameManager 오브젝트에 붙이는 방식이 낫다.

#### B안: `MainUIController`에 커서 적용 추가

새 클래스 생성이 허용되지 않을 때의 후보.

장점:

- 이미 씬 UI에 존재할 가능성이 높다.
- `Start()`에서 커서 적용하기 쉽다.

위험:

- UI 컨트롤러가 커서 리소스까지 관리하게 된다.
- 전투 씬 외 다른 씬에도 같은 커서를 적용하려면 씬별 UI 상태를 확인해야 한다.

추천도:

- "기본 커서만 바꾸기"라면 가능.
- 조준 상태에 따라 커서가 바뀌는 기능까지 생각하면 전용 `CursorController`가 더 낫다.

#### C안: `GameManager`에 커서 적용 추가

장점:

- 씬 전환 후에도 유지하기 쉽다.
- 공통 설정처럼 다룰 수 있다.

위험:

- `GameManager`가 이미 전투 결과, 씬 전환, GameContext 등 많은 책임을 가진다.
- 커서까지 넣으면 더 커진다.

추천도:

- 모든 씬에서 무조건 같은 커서를 적용하는 최소 기능이면 가능.
- 상태별 커서 변경은 별도 컨트롤러가 낫다.

## 4. 수정 예상 지점

| 기능 | 수정 예상 클래스/오브젝트 | 수정 예상 메소드/필드 | 변경 이유 |
|---|---|---|---|
| 피격 카메라 흔들림 | 새 `CameraShakeController` 추천 | `Shake()`, `duration`, `strength` | 카메라 위치 흔들림 담당 |
| 피격 카메라 흔들림 | `Main Camera.prefab` | `CameraShakeController` 컴포넌트 추가 | 흔들림 컴포넌트 배치 |
| 피격 카메라 흔들림 | `CombatSystem` | `HandleRoomCollision()` | 방 명중 후 흔들림 호출 |
| 피격 카메라 흔들림 | 선택 사항: `CombatSystem` | `HandleShieldCollision()` | 실드 피격도 흔들고 싶을 때 |
| 커서 변경 | 새 `CursorController` 추천 | `defaultCursor`, `hotspot`, `Start()` | 시작 시 커서 적용 |
| 커서 변경 | 대체: `MainUIController` | `Texture2D cursorTexture`, `Vector2 hotspot`, `Start()` | 새 클래스 불가 시 기본 커서 적용 |
| 커서 변경 | Unity Import 설정 | 커서 이미지 Texture 설정 | `Cursor.SetCursor()`에 맞는 Texture2D 준비 |

## 5. 호출부 영향

카메라 흔들림:

- `CombatSystem.HandleRoomCollision()`에서 피해 적용 후 흔들림 호출이 추가될 가능성이 높다.
- 회피 기능을 같이 구현한다면 `HandleRoomCollision()`의 bool 결과와 흔들림 타이밍을 맞춰야 한다.
- 명중하지 않은 경우에는 흔들림이 없어야 한다.
- 기존 함수 시그니처 변경은 필수는 아니다.

커서 변경:

- 기본 커서만 바꾸는 경우 기존 호출부 영향은 거의 없다.
- 무기 선택 중 커서를 바꾸려면 `WeaponSystem.SetSelectedWeapon()`, `ResetSelectedWeapon()` 같은 지점과 연결될 수 있다.
- 이 경우 커서 컨트롤러를 호출하는 지점이 늘어나므로, 내일은 "항상 기본 커서 교체"만 먼저 구현하는 것이 안전하다.

## 6. 실수하기 쉬운 지점

카메라 흔들림:

- 흔들림 후 원래 위치로 복구하지 않으면 카메라가 조금씩 밀릴 수 있다.
- 코루틴이 중복 실행되면 원래 위치 기준이 꼬일 수 있다.
- z값을 흔들면 2D 카메라 거리나 렌더링이 이상해질 수 있다.
- `Camera.main`을 매 프레임 찾는 방식은 피하는 것이 좋다.
- 씬 전환이나 오브젝트 비활성화 시 흔들림 코루틴이 끊기면서 위치가 복구되지 않을 수 있다.
- `AspectRatioController`는 rect를 바꾸므로 위치 흔들림과 직접 충돌하지 않지만, 같은 카메라에 여러 스크립트가 붙는다는 점은 확인해야 한다.

커서 변경:

- `Cursor.SetCursor()`에는 Sprite가 아니라 Texture2D가 필요하다.
- sliced sprite의 `sprite.texture`를 넘기면 전체 atlas가 커서로 보일 수 있다.
- 현재 FTL cursor sprite sheet는 Multiple Sprite이고 `isReadable`이 꺼져 있다.
- hotspot을 잘못 잡으면 클릭 위치가 시각적 커서와 어긋난다.
- 커서 이미지가 너무 크면 OS나 플랫폼에 따라 표시가 어색할 수 있다.
- 게임 창 밖으로 나가거나 에디터 Play 종료 시 커서가 원복되는지 확인해야 한다.
- 소프트웨어 커서 방식은 UI raycast를 막지 않도록 Image의 Raycast Target을 끄는 것이 중요하다.

## 7. Unity에서 직접 해야 할 작업

카메라 흔들림:

- 새 클래스 허용 시:
  - `CameraShakeController` 스크립트 생성 허용 필요
  - `Main Camera.prefab`에 컴포넌트 추가
  - `duration`, `strength` 값 설정
- 새 클래스 불허 시:
  - `AspectRatioController`에 흔들림 기능을 넣어도 되는지 결정 필요
- 테스트:
  - 방 명중 시 흔들림
  - 회피 시 흔들리지 않음
  - 실드 피격 시 흔들릴지 여부 확인
  - 연속 피격 시 카메라 위치가 밀리지 않는지 확인
  - 전투 종료 UI가 뜬 뒤 카메라 위치가 정상인지 확인

커서 변경:

- 추천 방식:
  - 사용할 커서를 별도 PNG로 분리
  - 가능하면 32x32 또는 64x64 크기
  - 투명 배경 유지
  - Texture2D로 Inspector에 넣을 수 있게 Import 설정
- 현재 FTL cursor sheet를 쓰고 싶다면:
  - Sprite Editor에서 원하는 커서를 확인
  - 가능하면 선택한 커서만 별도 PNG로 추출
  - Runtime에서 Sprite rect를 잘라 쓰는 방식은 Read/Write 설정과 코드가 늘어나므로 최소 구현에는 비추천
- 테스트:
  - 기본 커서가 게임 시작 시 바뀌는지
  - 클릭 지점이 hotspot과 맞는지
  - BattleScene, BattlePrepare, MainMenu 중 어디까지 적용되는지
  - 빌드에서도 커서가 보이는지

## 8. 권장 구현 순서

1. 커서 이미지 준비부터 한다.
   - 지정할 스프라이트를 별도 PNG로 분리하는 것이 가장 안전하다.
   - hotspot을 먼저 정한다.

2. 기본 커서 교체만 구현한다.
   - 상태별 커서 변경은 나중에 한다.
   - 확인 기준: Play 시작 시 커서가 지정 이미지로 보이고 클릭 위치가 맞는지.

3. 카메라 흔들림 컴포넌트 방식을 확정한다.
   - 새 파일/새 클래스 허용이면 `CameraShakeController`.
   - 불허이면 `AspectRatioController`에 최소 기능 추가.

4. 방 명중 시 흔들림만 연결한다.
   - `CombatSystem.HandleRoomCollision()`에서 실제 피해 적용 후 호출.
   - 회피 기능과 같이 구현한다면 명중일 때만 호출.

5. 연속 피격 테스트를 한다.
   - 흔들림 중 다시 흔들림 요청이 들어와도 카메라가 원래 위치로 돌아오는지 확인한다.

6. 실드 피격 흔들림은 나중에 결정한다.
   - 방 명중 흔들림보다 약하게 줄 수 있지만, 내일 최소 구현에서는 제외해도 된다.

## 9. 최종 결론

지금 바로 가도 되는 설계:

- 커서는 `Cursor.SetCursor()` 기반으로 구현하는 것이 가장 작다.
- 카메라 흔들림은 방 명중 확정 후 `CombatSystem.HandleRoomCollision()`에서 호출하는 것이 가장 안전하다.

반드시 조심해야 할 부분:

- 커서에 Sprite를 직접 넣을 수 없다는 점.
- 현재 cursor sheet는 여러 Sprite로 잘린 atlas라, 그대로 `sprite.texture`를 쓰면 원하는 한 칸만 나오지 않을 수 있다는 점.
- 카메라 흔들림 코루틴 중복과 위치 복구.

내일 구현 전 결정할 것:

- 새 파일/새 클래스 생성을 허용할지.
- 커서를 모든 씬에 적용할지, BattleScene에만 적용할지.
- 커서 이미지를 별도 PNG로 분리할지, 기존 sprite sheet에서 잘라 쓸지.
- 실드 피격에도 카메라 흔들림을 줄지.

시간이 부족할 때 최소 범위:

1. 커서용 별도 PNG 준비
2. 기본 커서만 `Cursor.SetCursor()`로 적용
3. 방 명중 시에만 카메라 흔들림
4. 실드 흔들림, 상태별 커서, 소프트웨어 커서는 후순위

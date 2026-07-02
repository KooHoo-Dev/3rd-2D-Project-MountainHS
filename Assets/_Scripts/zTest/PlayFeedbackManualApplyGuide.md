# Unity 수동 반영 작업 정리

## 1. 전투 결과 UI

### 관련 코드
- `Assets/_Scripts/zTest/PlayFeedbackTestBootstrap.cs`

### 코드상 구현된 내용
- `CreateResultPanelTest()`에서 Victory / Defeat / Escape 테스트 버튼을 생성함.
- `CreateResultOverlay()`에서 화면 전체 클릭 차단용 오버레이와 결과 패널을 생성함.
- `ShowResult()`에서 결과 텍스트를 바꾸고 결과 패널을 최상단으로 올림.
- `OnBehindClicked()`로 결과 패널 표시 중 뒤쪽 UI 클릭이 막히는지 확인할 수 있음.

### 코드상으로 직접 만든 지점
- `#region 전투 결과 UI`
- `Result Blocking Overlay`
- `Result Panel`
- `Result Text`
- `Victory Button`, `Defeat Button`, `Escape Button`

### Unity Editor에서 직접 만들어야 하는 지점
- BattleScene용 결과 패널 GameObject.
- 화면 전체를 덮는 반투명 클릭 차단 Image.
- Victory / Defeat / Escape 상태를 표시할 TMP 텍스트.
- 결과 후 이동 버튼이 필요하다면 Next Battle / Main Menu / Retry 버튼.

### BattleScene에 반영해야 하는 작업
- 기존 전투 UI Canvas 하위 또는 별도 Result Canvas에 결과 패널 배치.
- 결과 패널이 비활성 상태로 시작하는지 확인.
- 전투 종료 판정 지점에서 결과 패널 표시 메소드를 호출.
- 결과 패널 표시 중 기존 무기/타겟/디버그 버튼 입력이 막히는지 확인.

### 연결해야 하는 Prefab / GameObject / Inspector 필드
- ResultPanel GameObject.
- ResultText TMP.
- ClickBlocker Image.
- 결과 버튼 Button 컴포넌트.
- 전투 종료를 판단하는 컨트롤러의 결과 표시 호출 지점.

### 추가로 필요한 Asset
- 결과 패널 배경 이미지 또는 기존 UI 패널 스프라이트.
- 버튼 스프라이트 또는 기존 UI 버튼 에셋.
- 결과 상태별 색상/아이콘이 필요하면 Victory / Defeat / Escape 표시용 에셋.

### 아직 확인해야 할 주의사항
- 결과 패널이 뜬 뒤 전투 입력, 타겟 선택, 무기 선택 입력이 남지 않는지 확인 필요.
- 결과 표시 후 TimeScale을 멈출지, 전투 루프만 멈출지 결정 필요.
- 패널이 여러 Canvas 위에 제대로 올라오는지 Sorting Order 확인 필요.

## 2. 무기 선택 / 타겟 하이라이트

### 관련 코드
- `Assets/_Scripts/zTest/PlayFeedbackTestBootstrap.cs`

### 코드상 구현된 내용
- `CreateWeaponSelectionTest()`에서 무기 버튼 3개를 만들고 선택된 버튼만 강조함.
- `SelectWeapon()`에서 이전 선택을 원복하고 새 선택만 노란색으로 표시함.
- `CreateTargetHighlightTest()`와 `CreateTargetTile()`에서 타겟 타일 6개를 생성함.
- `PlayFeedbackTargetTileTest`에서 hover / exit / click 입력을 받아 타일 색상을 갱신함.

### 코드상으로 직접 만든 지점
- `#region 무기 선택 / 타겟 하이라이트`
- `Weapon Button 1`, `Weapon Button 2`, `Weapon Button 3`
- `Target Tile 1`부터 `Target Tile 6`
- `PlayFeedbackTargetTileTest`

### Unity Editor에서 직접 만들어야 하는 지점
- 실제 무기 버튼 선택 강조용 Image 또는 Outline.
- 실제 방/타일 하이라이트용 별도 Highlight GameObject.
- 선택 해제 버튼 또는 선택 취소 입력 규칙.

### BattleScene에 반영해야 하는 작업
- 기존 WeaponController는 바로 수정하지 말고, 먼저 무기 버튼 UI 쪽에서 선택 상태 표시만 연결.
- RoomViewer 본체를 직접 바꾸기보다 방 위에 별도 Highlight 오브젝트를 얹는 방식 검토.
- hover와 선택 상태가 동시에 있을 때 우선순위 정리.

### 연결해야 하는 Prefab / GameObject / Inspector 필드
- 무기 버튼 GameObject / Button / Image.
- 방 타일 또는 RoomViewer GameObject.
- Highlight 전용 자식 GameObject 또는 별도 프리팹.
- 선택 상태를 알려주는 테스트용 또는 전투용 선택 컨트롤러.

### 추가로 필요한 Asset
- 무기 선택 강조 프레임 또는 색상 오버레이.
- 타겟 hover / selected / disabled 상태별 스프라이트 또는 색상.
- 선택 해제 효과가 필요하면 간단한 테두리/플래시 에셋.

### 아직 확인해야 할 주의사항
- RoomViewer의 기존 SpriteRenderer, Collider, Room 연결을 건드리지 않는 방식이 안전함.
- Highlight 오브젝트가 클릭을 가로막지 않도록 Raycast Target 또는 Collider 설정 확인 필요.
- 무기 선택 상태와 타겟 선택 상태가 전투 발사 로직과 언제 연결될지 결정 필요.

## 3. 피격 타일 / 시설 피드백

### 관련 코드
- `Assets/_Scripts/zTest/PlayFeedbackTestBootstrap.cs`

### 코드상 구현된 내용
- `CreateChargeDecayTest()`에서 가짜 무기 충전바와 Toggle Power 버튼을 생성함.
- `UpdateChargeValue()`에서 전력 ON이면 충전 증가, 전력 OFF이면 즉시 0이 아니라 서서히 감소하도록 테스트함.
- 피격 타일 자체의 실제 전투 연동은 아직 구현하지 않고, 타겟 하이라이트와 Floating Text 테스트로 분리해 둠.

### 코드상으로 직접 만든 지점
- `#region 피격 타일 / 시설 피드백`
- `Charge Frame`
- `Charge Fill`
- `Power Toggle Button`

### Unity Editor에서 직접 만들어야 하는 지점
- 실제 Weapon Charge UI에 감소 상태를 보여줄 Fill Image.
- 시설 전력 ON/OFF 상태를 시각적으로 보여줄 아이콘 또는 색상.
- 피격된 방/시설에 일시적으로 표시할 Damage Highlight 오브젝트.

### BattleScene에 반영해야 하는 작업
- WeaponController 수정 전, 현재 충전값이 어디에서 증가/초기화되는지 먼저 확인.
- 전력 OFF일 때 충전 감소가 발사 예약, 쿨다운, 무기 비활성화 규칙과 충돌하지 않는지 판단.
- 시설 피격 피드백은 FacilitySystem 또는 Room 쪽 기존 상태와 연결 지점을 정해야 함.

### 연결해야 하는 Prefab / GameObject / Inspector 필드
- Weapon Charge Bar Image.
- Weapon Charge Frame Image.
- 전력 상태를 가진 시설 또는 무기 시스템 참조.
- 피격 하이라이트용 방/시설 자식 오브젝트.

### 추가로 필요한 Asset
- 충전 감소 상태용 색상 또는 애니메이션.
- 시설 피격 표시용 짧은 플래시 스프라이트.
- 방 피해/화재/고장 표시와 겹치지 않는 별도 피드백 이미지.

### 아직 확인해야 할 주의사항
- 전력 OFF를 충전 감소로 처리할지, 충전 정지 후 일정 시간 유지할지 결정 필요.
- 적 무기에도 같은 규칙을 적용할지 별도 판단 필요.
- 충전 UI의 Fill 방식이 현재 프리팹 구조와 맞는지 확인 필요.

## 4. Floating Text

### 관련 코드
- `Assets/_Scripts/zTest/PlayFeedbackTestBootstrap.cs`

### 코드상 구현된 내용
- `CreateFloatingTextTest()`에서 Hit / Shield / -1 테스트 버튼을 생성함.
- `SpawnFloatingText()`에서 지정 텍스트와 색상으로 Floating Text를 생성함.
- `FloatingTextRoutine()`에서 텍스트가 위로 이동하며 페이드아웃된 뒤 삭제됨.

### 코드상으로 직접 만든 지점
- `#region Floating Text`
- `Floating Text Root`
- `Floating Hit Button`
- `Floating Shield Button`
- `Floating Damage Button`

### Unity Editor에서 직접 만들어야 하는 지점
- BattleScene Canvas 하위에 FloatingTextRoot 배치.
- 전투 위치를 UI 좌표로 변환할 기준 Canvas / Camera 정리.
- 실제 전투에서 호출할 Floating Text 프리팹 또는 생성 스크립트 배치.

### BattleScene에 반영해야 하는 작업
- 피해 발생, 실드 흡수, 빗나감 등 이벤트 지점에서 Floating Text 호출.
- 월드 좌표 방 위치를 Canvas 좌표로 바꾸는 처리 추가.
- 동시에 여러 텍스트가 뜰 때 겹침 보정이 필요한지 확인.

### 연결해야 하는 Prefab / GameObject / Inspector 필드
- FloatingTextRoot RectTransform.
- FloatingText 프리팹 또는 Text/TMP 오브젝트.
- 전투 이벤트 호출 지점.
- 필요 시 UI Camera 또는 Main Camera.

### 추가로 필요한 Asset
- TMP 폰트 또는 기존 UI 폰트.
- Hit / Shield / Damage 상태별 색상 정의.
- 더 강한 피드백이 필요하면 작은 팝 애니메이션.

### 아직 확인해야 할 주의사항
- 테스트 코드는 Unity 기본 `Text`를 사용하므로 실제 적용 시 TMP로 바꿀지 결정 필요.
- 전투 중 TimeScale 변경이 코루틴 속도에 영향을 주는지 확인 필요.
- 화면 밖 위치에서 생성되지 않도록 좌표 변환 검증 필요.

## 5. 해상도 UI 안정화

### 관련 코드
- `Assets/_Scripts/zTest/PlayFeedbackTestBootstrap.cs`
- `Assets/_Scenes/BattleScene.unity`
- `Assets/_Scenes/BattlePrepare.unity`

### 코드상 구현된 내용
- 테스트 Canvas는 `Scale With Screen Size`, 기준 해상도 `1920x1080`, Match `0.5`로 생성함.
- `SetRect()`를 통해 테스트 UI의 Anchor / Position / Size를 일관되게 배치함.
- 원본 BattleScene / BattlePrepare는 수정하지 않고 위험 후보만 확인함.

### 코드상으로 직접 만든 지점
- `#region 해상도 UI 안정화`
- `Play Feedback Test Canvas`
- `CanvasScaler`
- `SetRect()`

### Unity Editor에서 직접 만들어야 하는 지점
- BattleScene/BattlePrepare의 주요 Canvas Scaler 설정 점검.
- Anchor가 중앙 고정인 버튼/패널의 작은 해상도 겹침 여부 확인.
- 여러 Canvas의 Scale Mode가 섞여 있는 지점 정리.

### BattleScene에 반영해야 하는 작업
- `Canvas` 중 일부는 `1920x1080`, Match `0` 기준이고, 일부는 `Constant Pixel Size`/`800x600` 기준이므로 우선순위 정리.
- Weapon Charge UI 그룹이 중앙 Anchor + 고정 좌표라 16:9 외 해상도에서 위치 확인.
- Debug UI와 실제 전투 UI의 Sorting Order / Scale Mode 분리 확인.

### 연결해야 하는 Prefab / GameObject / Inspector 필드
- BattleScene의 주요 Canvas.
- Weapon Charge Group / Frame / Bar.
- Reactor Canvas.
- Hp Canvas.
- Debug UI.

### 추가로 필요한 Asset
- 새 Asset은 필수 아님.
- UI 배경/프레임을 조정한다면 9-slice 가능한 패널 스프라이트가 있으면 안전함.

### 아직 확인해야 할 주의사항
- 자동 수정 금지. 먼저 Game 뷰에서 1920x1080, 1600x900, 1366x768, 1280x720, 세로/좁은 비율을 직접 확인 필요.
- MatchWidthOrHeight를 바꾸면 기존 UI 전체 위치가 움직일 수 있음.
- Anchor를 바꾸기 전에 Prefab Instance override가 많은지 확인 필요.

## 6. 배경 / 시각 효과 관련 작업

### 관련 코드
- `Assets/_Scripts/zTest/PlayFeedbackTestBootstrap.cs`

### 코드상 구현된 내용
- `ConfigureCamera()`에서 테스트 카메라 배경색을 어둡게 맞춤.
- `CreateBackgroundTest()`에서 화면 전체 배경 Image를 생성함.
- `ApplyRandomBackground()`에서 세 후보 중 하나를 랜덤 색상 배경으로 표시함.

### 코드상으로 직접 만든 지점
- `#region 배경 / 시각 효과 관련 작업`
- `Random Static Background Test`
- `Background Label`
- `Randomize Background`

### Unity Editor에서 직접 만들어야 하는 지점
- 실제 배경 후보 Sprite 또는 Prefab 선정.
- BattleScene에서 배경을 담을 부모 GameObject 지정.
- 랜덤 선택이 씬 진입마다 바뀔지, 전투 시작 시점마다 바뀔지 결정.

### BattleScene에 반영해야 하는 작업
- `Background` 오브젝트 또는 별도 `Battle Background Root`에 랜덤 배경 스크립트 부착.
- 후보 배경 SpriteRenderer/Image 배열을 Inspector에 연결.
- 배경이 전투 UI와 선박 Sprite를 가리지 않도록 Sorting Layer / Order 확인.

### 연결해야 하는 Prefab / GameObject / Inspector 필드
- Background GameObject.
- 배경 후보 SpriteRenderer 또는 Image 목록.
- 필요 시 Main Camera.
- 배경 전환을 담당할 테스트용 또는 실제 적용용 스크립트.

### 추가로 필요한 Asset
- 전투 배경 후보 이미지 2개 이상.
- 필요하다면 별/먼지 같은 정적 장식 스프라이트.
- 배경별 색감 보정을 위한 Material은 선택 사항.

### 아직 확인해야 할 주의사항
- 정적 랜덤 배경만 먼저 적용하고, 애니메이션/패럴랙스는 다음 단계로 분리하는 것이 안전함.
- 배경 이미지 해상도와 카메라/Canvas 기준이 맞지 않으면 늘어짐이 생길 수 있음.
- 실제 BattleScene에 적용하기 전 테스트 씬에서 후보 이미지 비율을 먼저 확인 필요.

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

public class PlayFeedbackTestBootstrap : MonoBehaviour
{
    [SerializeField] private string sceneTitle = "Play Feedback Test";

    private Canvas canvas;
    private Font defaultFont;

    private Image backgroundImage;
    private Text backgroundLabel;

    private GameObject resultOverlay;
    private Text resultText;
    private Text behindClickText;
    private int behindClickCount;

    private Image[] weaponButtonImages;
    private int selectedWeaponIndex = -1;

    private Image[] tileImages;
    private bool[] tileSelectedStates;

    private RectTransform floatingTextRoot;

    private Image chargeFillImage;
    private Text chargeStateText;
    private bool powerOn = true;
    private float chargeValue;

    private void Awake()
    {
        EnsureEventSystem();
        ConfigureCamera();
        CreateCanvas();

        CreateBackgroundTest();
        CreateResultPanelTest();
        CreateWeaponSelectionTest();
        CreateTargetHighlightTest();
        CreateFloatingTextTest();
        CreateChargeDecayTest();
    }

    private void Update()
    {
        UpdateChargeValue();
    }

#region 해상도 UI 안정화

    // 1. 테스트 씬에 필요한 EventSystem과 기준 Canvas를 준비함
    private void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("Play Feedback Test EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
        eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Play Feedback Test Canvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        Text title = CreateText("Play Feedback Test", canvas.transform, sceneTitle, 34, TextAnchor.MiddleLeft, Color.white);
        SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -32f), new Vector2(720f, 56f));
    }

    private Button CreateButton(string name, Transform parent, string label)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.22f, 0.27f, 0.98f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.18f, 0.22f, 0.27f, 0.98f);
        colors.highlightedColor = new Color(0.3f, 0.38f, 0.45f, 1f);
        colors.pressedColor = new Color(0.08f, 0.14f, 0.17f, 1f);
        colors.selectedColor = new Color(0.25f, 0.32f, 0.38f, 1f);
        button.colors = colors;

        Text text = CreateText(name + " Text", buttonObject.transform, label, 18, TextAnchor.MiddleCenter, Color.white);
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        return button;
    }

    private Image CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panelObject = new GameObject(name);
        panelObject.transform.SetParent(parent, false);
        Image image = panelObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private Text CreateText(string name, Transform parent, string value, int size, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = GetDefaultFont();
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private Font GetDefaultFont()
    {
        if (defaultFont != null)
        {
            return defaultFont;
        }

        defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null)
        {
            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return defaultFont;
    }

    private void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        rectTransform.localScale = Vector3.one;
    }

#endregion

#region 배경 / 시각 효과 관련 작업

    // 1. 테스트 씬 카메라와 랜덤 정적 배경 후보를 표시함
    private void ConfigureCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.02f, 0.025f, 0.035f, 1f);
    }

    private void CreateBackgroundTest()
    {
        GameObject backgroundObject = new GameObject("Random Static Background Test");
        backgroundObject.transform.SetParent(canvas.transform, false);
        backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.raycastTarget = false;
        SetRect(backgroundImage.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        backgroundObject.transform.SetAsFirstSibling();

        backgroundLabel = CreateText("Background Label", backgroundObject.transform, "", 28, TextAnchor.MiddleCenter, Color.white);
        SetRect(backgroundLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -96f), new Vector2(520f, 54f));

        Button randomButton = CreateButton("Randomize Background", canvas.transform, "Random BG");
        SetRect(randomButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -104f), new Vector2(180f, 46f));
        randomButton.onClick.AddListener(OnRandomBackgroundClicked);

        ApplyRandomBackground();
    }

    private void ApplyRandomBackground()
    {
        int index = Random.Range(0, 3);

        if (index == 0)
        {
            backgroundImage.color = new Color(0.04f, 0.07f, 0.13f, 1f);
            backgroundLabel.text = "Background Candidate A";
            return;
        }

        if (index == 1)
        {
            backgroundImage.color = new Color(0.13f, 0.04f, 0.08f, 1f);
            backgroundLabel.text = "Background Candidate B";
            return;
        }

        backgroundImage.color = new Color(0.04f, 0.12f, 0.09f, 1f);
        backgroundLabel.text = "Background Candidate C";
    }

    private void OnRandomBackgroundClicked()
    {
        ApplyRandomBackground();
    }

#endregion

#region 전투 결과 UI

    // 1. 전투 결과 패널을 표시하고 뒤쪽 UI 클릭 차단을 테스트함
    private void CreateResultPanelTest()
    {
        Text heading = CreateText("Result Test Heading", canvas.transform, "Result Panel", 24, TextAnchor.MiddleLeft, Color.white);
        SetRect(heading.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -180f), new Vector2(320f, 42f));

        Button victoryButton = CreateButton("Victory Button", canvas.transform, "Victory");
        SetRect(victoryButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -236f), new Vector2(130f, 44f));
        victoryButton.onClick.AddListener(OnVictoryClicked);

        Button defeatButton = CreateButton("Defeat Button", canvas.transform, "Defeat");
        SetRect(defeatButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(178f, -236f), new Vector2(130f, 44f));
        defeatButton.onClick.AddListener(OnDefeatClicked);

        Button escapeButton = CreateButton("Escape Button", canvas.transform, "Escape");
        SetRect(escapeButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, -236f), new Vector2(130f, 44f));
        escapeButton.onClick.AddListener(OnEscapeClicked);

        Button behindButton = CreateButton("Behind Click Test Button", canvas.transform, "Behind Click Test");
        SetRect(behindButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -260f), new Vector2(230f, 52f));
        behindButton.onClick.AddListener(OnBehindClicked);

        behindClickText = CreateText("Behind Click Count", canvas.transform, "Behind clicks: 0", 20, TextAnchor.MiddleCenter, Color.white);
        SetRect(behindClickText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -318f), new Vector2(260f, 36f));

        CreateResultOverlay();
    }

    private void CreateResultOverlay()
    {
        resultOverlay = new GameObject("Result Blocking Overlay");
        resultOverlay.transform.SetParent(canvas.transform, false);

        Image blocker = resultOverlay.AddComponent<Image>();
        blocker.color = new Color(0f, 0f, 0f, 0.66f);
        blocker.raycastTarget = true;
        SetRect(blocker.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Image panel = CreatePanel("Result Panel", resultOverlay.transform, new Color(0.06f, 0.08f, 0.1f, 0.98f));
        SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(480f, 250f));

        resultText = CreateText("Result Text", panel.transform, "Result", 34, TextAnchor.MiddleCenter, Color.white);
        SetRect(resultText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 34f), new Vector2(-56f, -110f));

        Button closeButton = CreateButton("Close Result Panel", panel.transform, "Close");
        SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(150f, 48f));
        closeButton.onClick.AddListener(OnCloseResultClicked);

        resultOverlay.SetActive(false);
    }

    private void ShowResult(string result)
    {
        resultText.text = result;
        resultOverlay.SetActive(true);
        resultOverlay.transform.SetAsLastSibling();
    }

    private void OnVictoryClicked()
    {
        ShowResult("Victory");
    }

    private void OnDefeatClicked()
    {
        ShowResult("Defeat");
    }

    private void OnEscapeClicked()
    {
        ShowResult("Escape");
    }

    private void OnCloseResultClicked()
    {
        resultOverlay.SetActive(false);
    }

    private void OnBehindClicked()
    {
        behindClickCount++;
        behindClickText.text = "Behind clicks: " + behindClickCount;
    }

#endregion

#region 무기 선택 / 타겟 하이라이트

    // 1. 선택된 무기와 타겟 타일의 하이라이트를 갱신함
    private void CreateWeaponSelectionTest()
    {
        Text heading = CreateText("Weapon Test Heading", canvas.transform, "Weapon Select", 24, TextAnchor.MiddleLeft, Color.white);
        SetRect(heading.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -322f), new Vector2(320f, 42f));

        weaponButtonImages = new Image[3];

        Button weaponOne = CreateButton("Weapon Button 1", canvas.transform, "Laser");
        SetRect(weaponOne.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -380f), new Vector2(130f, 52f));
        weaponOne.transition = Selectable.Transition.None;
        weaponButtonImages[0] = weaponOne.GetComponent<Image>();
        weaponOne.onClick.AddListener(OnWeaponOneClicked);

        Button weaponTwo = CreateButton("Weapon Button 2", canvas.transform, "Beam");
        SetRect(weaponTwo.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(178f, -380f), new Vector2(130f, 52f));
        weaponTwo.transition = Selectable.Transition.None;
        weaponButtonImages[1] = weaponTwo.GetComponent<Image>();
        weaponTwo.onClick.AddListener(OnWeaponTwoClicked);

        Button weaponThree = CreateButton("Weapon Button 3", canvas.transform, "Missile");
        SetRect(weaponThree.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, -380f), new Vector2(130f, 52f));
        weaponThree.transition = Selectable.Transition.None;
        weaponButtonImages[2] = weaponThree.GetComponent<Image>();
        weaponThree.onClick.AddListener(OnWeaponThreeClicked);

        SelectWeapon(0);
    }

    private void CreateTargetHighlightTest()
    {
        Text heading = CreateText("Target Test Heading", canvas.transform, "Target Highlight", 24, TextAnchor.MiddleLeft, Color.white);
        SetRect(heading.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -470f), new Vector2(360f, 42f));

        tileImages = new Image[6];
        tileSelectedStates = new bool[6];

        int index = 0;
        int row = 0;
        while (row < 2)
        {
            int column = 0;
            while (column < 3)
            {
                CreateTargetTile(index, column, row);
                index++;
                column++;
            }

            row++;
        }

        Button clearButton = CreateButton("Clear Target Selection", canvas.transform, "Clear");
        SetRect(clearButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, -646f), new Vector2(130f, 44f));
        clearButton.onClick.AddListener(OnClearTargetsClicked);
    }

    private void CreateTargetTile(int index, int column, int row)
    {
        GameObject tileObject = new GameObject("Target Tile " + (index + 1));
        tileObject.transform.SetParent(canvas.transform, false);

        Image tileImage = tileObject.AddComponent<Image>();
        tileImage.color = new Color(0.16f, 0.24f, 0.29f, 0.94f);
        tileImage.raycastTarget = true;
        SetRect(tileImage.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f + column * 92f, -540f - row * 72f), new Vector2(78f, 58f));

        Text label = CreateText("Target Tile Text " + (index + 1), tileObject.transform, "T" + (index + 1), 20, TextAnchor.MiddleCenter, Color.white);
        SetRect(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        PlayFeedbackTargetTileTest tile = tileObject.AddComponent<PlayFeedbackTargetTileTest>();
        tile.Setup(this, index);

        tileImages[index] = tileImage;
    }

    private void SelectWeapon(int index)
    {
        selectedWeaponIndex = index;

        int i = 0;
        while (i < weaponButtonImages.Length)
        {
            if (i == selectedWeaponIndex)
            {
                weaponButtonImages[i].color = new Color(0.95f, 0.72f, 0.18f, 1f);
            }
            else
            {
                weaponButtonImages[i].color = new Color(0.18f, 0.22f, 0.27f, 0.98f);
            }

            i++;
        }
    }

    private void ClearTargets()
    {
        int i = 0;
        while (i < tileSelectedStates.Length)
        {
            tileSelectedStates[i] = false;
            SetTileNormal(i);
            i++;
        }
    }

    private void SetTileNormal(int index)
    {
        if (tileImages[index] == null)
        {
            return;
        }

        tileImages[index].color = new Color(0.16f, 0.24f, 0.29f, 0.94f);
    }

    public void OnTileHover(int index)
    {
        if (tileSelectedStates[index])
        {
            return;
        }

        tileImages[index].color = new Color(0.28f, 0.48f, 0.58f, 1f);
    }

    public void OnTileExit(int index)
    {
        if (tileSelectedStates[index])
        {
            return;
        }

        SetTileNormal(index);
    }

    public void OnTileSelected(int index)
    {
        ClearTargets();
        tileSelectedStates[index] = true;
        tileImages[index].color = new Color(0.92f, 0.24f, 0.16f, 1f);
    }

    private void OnWeaponOneClicked()
    {
        SelectWeapon(0);
    }

    private void OnWeaponTwoClicked()
    {
        SelectWeapon(1);
    }

    private void OnWeaponThreeClicked()
    {
        SelectWeapon(2);
    }

    private void OnClearTargetsClicked()
    {
        ClearTargets();
    }

#endregion

#region 피격 타일 / 시설 피드백

    // 1. 전력 OFF 상태에서 가짜 무기 충전도가 서서히 감소하는지 확인함
    private void CreateChargeDecayTest()
    {
        Text heading = CreateText("Charge Test Heading", canvas.transform, "Charge Decay", 24, TextAnchor.MiddleLeft, Color.white);
        SetRect(heading.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-526f, -338f), new Vector2(320f, 42f));

        Image frame = CreatePanel("Charge Frame", canvas.transform, new Color(0.04f, 0.05f, 0.06f, 1f));
        SetRect(frame.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-526f, -418f), new Vector2(330f, 42f));

        GameObject fillObject = new GameObject("Charge Fill");
        fillObject.transform.SetParent(frame.transform, false);
        chargeFillImage = fillObject.AddComponent<Image>();
        chargeFillImage.color = new Color(0.2f, 0.78f, 0.95f, 1f);
        chargeFillImage.type = Image.Type.Filled;
        chargeFillImage.fillMethod = Image.FillMethod.Horizontal;
        chargeFillImage.fillOrigin = 0;
        chargeFillImage.fillAmount = 0f;
        SetRect(chargeFillImage.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-8f, -8f));

        chargeStateText = CreateText("Charge State Text", canvas.transform, "Power ON / Charge 0%", 20, TextAnchor.MiddleLeft, Color.white);
        SetRect(chargeStateText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-526f, -478f), new Vector2(330f, 34f));

        Button toggleButton = CreateButton("Power Toggle Button", canvas.transform, "Toggle Power");
        SetRect(toggleButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-526f, -540f), new Vector2(180f, 48f));
        toggleButton.onClick.AddListener(OnTogglePowerClicked);
    }

    private void UpdateChargeValue()
    {
        float speed;
        if (powerOn)
        {
            speed = 0.22f;
            chargeValue += speed * Time.deltaTime;
        }
        else
        {
            speed = 0.16f;
            chargeValue -= speed * Time.deltaTime;
        }

        chargeValue = Mathf.Clamp01(chargeValue);
        chargeFillImage.fillAmount = chargeValue;

        string state;
        if (powerOn)
        {
            state = "Power ON";
        }
        else
        {
            state = "Power OFF";
        }

        chargeStateText.text = state + " / Charge " + Mathf.RoundToInt(chargeValue * 100f) + "%";
    }

    private void OnTogglePowerClicked()
    {
        powerOn = !powerOn;
    }

#endregion

#region Floating Text

    // 1. 버튼 입력으로 전투 피드백용 Floating Text를 생성하고 사라지게 함
    private void CreateFloatingTextTest()
    {
        Text heading = CreateText("Floating Text Heading", canvas.transform, "Floating Text", 24, TextAnchor.MiddleLeft, Color.white);
        SetRect(heading.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-526f, -180f), new Vector2(320f, 42f));

        Button hitButton = CreateButton("Floating Hit Button", canvas.transform, "Hit");
        SetRect(hitButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-526f, -236f), new Vector2(130f, 44f));
        hitButton.onClick.AddListener(OnFloatingHitClicked);

        Button shieldButton = CreateButton("Floating Shield Button", canvas.transform, "Shield");
        SetRect(shieldButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-384f, -236f), new Vector2(130f, 44f));
        shieldButton.onClick.AddListener(OnFloatingShieldClicked);

        Button missButton = CreateButton("Floating Damage Button", canvas.transform, "-1");
        SetRect(missButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-242f, -236f), new Vector2(130f, 44f));
        missButton.onClick.AddListener(OnFloatingDamageClicked);

        GameObject root = new GameObject("Floating Text Root");
        root.transform.SetParent(canvas.transform, false);
        floatingTextRoot = root.AddComponent<RectTransform>();
        SetRect(floatingTextRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    private void SpawnFloatingText(string value, Color color)
    {
        Text text = CreateText("Floating Text " + value, floatingTextRoot, value, 34, TextAnchor.MiddleCenter, color);
        SetRect(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(420f, -40f), new Vector2(180f, 52f));
        StartCoroutine(FloatingTextRoutine(text));
    }

    private IEnumerator FloatingTextRoutine(Text text)
    {
        float time = 0f;
        float duration = 1.1f;
        RectTransform rectTransform = text.rectTransform;
        Color startColor = text.color;
        Vector2 startPosition = rectTransform.anchoredPosition;

        while (time < duration)
        {
            time += Time.deltaTime;
            float rate = time / duration;
            rectTransform.anchoredPosition = startPosition + new Vector2(0f, 80f * rate);
            text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - rate);
            yield return null;
        }

        Destroy(text.gameObject);
    }

    private void OnFloatingHitClicked()
    {
        SpawnFloatingText("Hit", new Color(1f, 0.85f, 0.24f, 1f));
    }

    private void OnFloatingShieldClicked()
    {
        SpawnFloatingText("Shield", new Color(0.28f, 0.74f, 1f, 1f));
    }

    private void OnFloatingDamageClicked()
    {
        SpawnFloatingText("-1", new Color(1f, 0.24f, 0.18f, 1f));
    }

#endregion
}

public class PlayFeedbackTargetTileTest : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private PlayFeedbackTestBootstrap owner;
    private int tileIndex;

    public void Setup(PlayFeedbackTestBootstrap newOwner, int newTileIndex)
    {
        owner = newOwner;
        tileIndex = newTileIndex;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        owner.OnTileHover(tileIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        owner.OnTileExit(tileIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        owner.OnTileSelected(tileIndex);
    }
}

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public const string GameTitle = "Epic Group 6 game";

    private enum GameState { Menu, Playing, Paused, Won }

    public static GameUI Instance { get; private set; }

    // Gameplay is allowed when no UI exists (e.g. testing scripts in another scene).
    public static bool IsPlaying => Instance == null || Instance.state == GameState.Playing;

    private static bool skipMenuOnNextLoad;

    private static readonly Color PanelColor = new Color(0.06f, 0.08f, 0.13f, 0.92f);
    private static readonly Color CardColor = new Color(0.11f, 0.14f, 0.22f, 0.97f);
    private static readonly Color HudColor = new Color(0.05f, 0.07f, 0.12f, 0.75f);
    private static readonly Color AccentColor = new Color(1f, 0.76f, 0.2f);
    private static readonly Color PrimaryButton = new Color(0.16f, 0.62f, 0.38f);
    private static readonly Color SecondaryButton = new Color(0.22f, 0.36f, 0.68f);
    private static readonly Color DangerButton = new Color(0.7f, 0.24f, 0.24f);
    public static readonly Color GoodColor = new Color(0.4f, 0.95f, 0.5f);
    public static readonly Color BadColor = new Color(1f, 0.4f, 0.4f);
    public static readonly Color InfoColor = new Color(0.55f, 0.8f, 1f);

    private GameState state;
    private float elapsedTime;

    private GameObject menuPanel;
    private GameObject controlsPanel;
    private GameObject pausePanel;
    private GameObject winPanel;
    private GameObject hudRoot;
    private GameObject returnPanelAfterControls;

    private TMP_Text scoreText;
    private TMP_Text timeText;
    private TMP_Text objectiveText;
    private TMP_Text winStatsText;
    private GameObject promptRoot;
    private TMP_Text promptText;
    private CanvasGroup toastGroup;
    private TMP_Text toastText;
    private Coroutine toastRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        CreateIfNeeded();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => CreateIfNeeded();

    private static void CreateIfNeeded()
    {
        if (Instance != null || FindAnyObjectByType<ObjectiveManager>() == null)
        {
            return;
        }

        new GameObject("GameUI").AddComponent<GameUI>();
    }

    private void Awake()
    {
        Instance = this;
        EnsureEventSystem();
        BuildUI();

        if (skipMenuOnNextLoad)
        {
            skipMenuOnNextLoad = false;
            StartGame();
        }
        else
        {
            ShowMenu();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;

        if (state == GameState.Playing)
        {
            elapsedTime += Time.deltaTime;
            timeText.text = FormatTime(elapsedTime);

            if (Cursor.lockState != CursorLockMode.Locked &&
                Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                SetCursorLocked(true);
            }
        }

        if (kb == null)
        {
            return;
        }

        if (controlsPanel.activeSelf)
        {
            if (kb.escapeKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame)
            {
                CloseControls();
            }
            return;
        }

        switch (state)
        {
            case GameState.Menu:
                if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) StartGame();
                else if (kb.cKey.wasPressedThisFrame) OpenControls(menuPanel);
                break;
            case GameState.Playing:
                if (kb.escapeKey.wasPressedThisFrame) Pause();
                break;
            case GameState.Paused:
                if (kb.escapeKey.wasPressedThisFrame) Resume();
                break;
            case GameState.Won:
                if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) PlayAgain();
                break;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && state == GameState.Playing)
        {
            SetCursorLocked(true);
        }
    }

    // ---------- Public API used by gameplay scripts ----------

    public void SetScore(int score, int winScore)
    {
        scoreText.text = $"<size=60%><color=#AAB4C8>SCORE</color></size>\n<b>{score}</b><size=60%> / {winScore}</size>";
    }

    public void SetObjective(string text)
    {
        objectiveText.text = text;
    }

    public void ShowPrompt(string text)
    {
        promptText.text = text;
        promptRoot.SetActive(true);
    }

    public void HidePrompt()
    {
        promptRoot.SetActive(false);
    }

    public void ShowToast(string text, Color color)
    {
        toastText.text = text;
        toastText.color = color;
        if (toastRoutine != null)
        {
            StopCoroutine(toastRoutine);
        }
        toastRoutine = StartCoroutine(ToastRoutine());
    }

    public void ShowWin(int score)
    {
        state = GameState.Won;
        Time.timeScale = 0f;
        SetCursorLocked(false);
        HidePrompt();
        winStatsText.text = $"Score: <b>{score}</b>\nTime: <b>{FormatTime(elapsedTime)}</b>";
        ShowOnly(winPanel);
        hudRoot.SetActive(true);
    }

    // ---------- State changes ----------

    private void ShowMenu()
    {
        state = GameState.Menu;
        Time.timeScale = 0f;
        SetCursorLocked(false);
        hudRoot.SetActive(false);
        ShowOnly(menuPanel);
    }

    private void StartGame()
    {
        state = GameState.Playing;
        Time.timeScale = 1f;
        elapsedTime = 0f;
        ShowOnly(null);
        hudRoot.SetActive(true);
        SetCursorLocked(true);
        ShowToast("Go to the Home Base!", AccentColor);
    }

    private void Pause()
    {
        state = GameState.Paused;
        Time.timeScale = 0f;
        SetCursorLocked(false);
        ShowOnly(pausePanel);
    }

    private void Resume()
    {
        state = GameState.Playing;
        Time.timeScale = 1f;
        ShowOnly(null);
        SetCursorLocked(true);
    }

    private void OpenControls(GameObject returnTo)
    {
        returnPanelAfterControls = returnTo;
        ShowOnly(controlsPanel);
    }

    private void CloseControls()
    {
        ShowOnly(returnPanelAfterControls);
    }

    private void PlayAgain()
    {
        skipMenuOnNextLoad = true;
        ReloadScene();
    }

    private void QuitToMenu()
    {
        skipMenuOnNextLoad = false;
        ReloadScene();
    }

    private static void ReloadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowOnly(GameObject panel)
    {
        menuPanel.SetActive(panel == menuPanel);
        controlsPanel.SetActive(panel == controlsPanel);
        pausePanel.SetActive(panel == pausePanel);
        winPanel.SetActive(panel == winPanel);

        // Prevent Space/Enter from re-triggering the last clicked button.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private static string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        return $"{total / 60:00}:{total % 60:00}";
    }

    private IEnumerator ToastRoutine()
    {
        RectTransform rt = (RectTransform)toastGroup.transform;
        const float fadeIn = 0.15f, hold = 1.8f, fadeOut = 0.5f;

        for (float t = 0f; t < fadeIn; t += Time.unscaledDeltaTime)
        {
            float k = t / fadeIn;
            toastGroup.alpha = k;
            rt.localScale = Vector3.one * Mathf.Lerp(1.25f, 1f, k);
            yield return null;
        }
        toastGroup.alpha = 1f;
        rt.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(hold);

        for (float t = 0f; t < fadeOut; t += Time.unscaledDeltaTime)
        {
            toastGroup.alpha = 1f - t / fadeOut;
            yield return null;
        }
        toastGroup.alpha = 0f;
        toastRoutine = null;
    }

    // ---------- UI construction ----------

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
        }

        InputSystemUIInputModule module = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (module == null)
        {
            module = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        if (module.actionsAsset == null || module.point == null || module.point.action == null)
        {
            module.AssignDefaultActions();
        }
    }

    private void BuildUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        BuildHud();
        BuildMenu();
        BuildControls();
        BuildPause();
        BuildWin();
    }

    private void BuildHud()
    {
        hudRoot = CreateRect("HUD", transform);
        Stretch(hudRoot);

        // Score + timer, top right
        GameObject scoreBox = CreatePanel("ScoreBox", hudRoot.transform, HudColor);
        Place(scoreBox, new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(220f, 150f));
        AddAccentBar(scoreBox, AccentColor);
        scoreText = CreateText(scoreBox.transform, "", 56, FontStyles.Normal, Color.white, TextAlignmentOptions.Center);
        Place(scoreText.gameObject, new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(200f, 100f));
        timeText = CreateText(scoreBox.transform, "00:00", 26, FontStyles.Normal, new Color(0.75f, 0.8f, 0.9f), TextAlignmentOptions.Center);
        Place(timeText.gameObject, new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(200f, 34f));

        // Current objective, top center
        GameObject objectiveBox = CreatePanel("ObjectiveBox", hudRoot.transform, HudColor);
        Place(objectiveBox, new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(900f, 120f));
        AddAccentBar(objectiveBox, InfoColor);
        TMP_Text label = CreateText(objectiveBox.transform, "CURRENT TASK", 22, FontStyles.Bold, InfoColor, TextAlignmentOptions.Center);
        label.characterSpacing = 8f;
        Place(label.gameObject, new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(860f, 30f));
        objectiveText = CreateText(objectiveBox.transform, "", 34, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
        objectiveText.enableAutoSizing = true;
        objectiveText.fontSizeMin = 20f;
        objectiveText.fontSizeMax = 34f;
        Place(objectiveText.gameObject, new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(860f, 70f));

        // Crosshair
        GameObject crosshair = CreatePanel("Crosshair", hudRoot.transform, new Color(1f, 1f, 1f, 0.8f));
        Place(crosshair, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6f, 6f));

        // Interaction prompt, bottom center
        promptRoot = CreatePanel("Prompt", hudRoot.transform, HudColor);
        Place(promptRoot, new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(640f, 70f));
        promptText = CreateText(promptRoot.transform, "", 30, FontStyles.Normal, Color.white, TextAlignmentOptions.Center);
        Stretch(promptText.gameObject);
        promptRoot.SetActive(false);

        // Toast / feedback message
        GameObject toast = CreateRect("Toast", hudRoot.transform);
        Place(toast, new Vector2(0.5f, 0.5f), new Vector2(0f, 180f), new Vector2(1400f, 120f));
        toastGroup = toast.AddComponent<CanvasGroup>();
        toastGroup.alpha = 0f;
        toastGroup.blocksRaycasts = false;
        toastText = CreateText(toast.transform, "", 64, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
        Stretch(toastText.gameObject);
        toastText.outlineWidth = 0.2f;
        toastText.outlineColor = new Color32(0, 0, 0, 200);
    }

    private void BuildMenu()
    {
        menuPanel = CreatePanel("MainMenu", transform, PanelColor);
        Stretch(menuPanel);

        TMP_Text title = CreateText(menuPanel.transform, GameTitle.ToUpper(), 110, FontStyles.Bold, AccentColor, TextAlignmentOptions.Center);
        title.characterSpacing = 6f;
        title.outlineWidth = 0.15f;
        title.outlineColor = new Color32(0, 0, 0, 255);
        Place(title.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 320f), new Vector2(1700f, 160f));

        GameObject card = CreatePanel("HowToPlay", menuPanel.transform, CardColor);
        Place(card, new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(1100f, 290f));
        AddAccentBar(card, AccentColor);
        TMP_Text howTo = CreateText(card.transform,
            "<b><color=#FFC233>HOW TO PLAY</color></b>\n" +
            "1. Walk to the <b>Home Base</b> and press <b>SPACE</b> to get a route.\n" +
            "2. The route tells you two stalls, e.g. <i>\"From the red to the blue\"</i>.\n" +
            "3. Visit the <b>first</b> stall, then the <b>second</b> one, and press <b>SPACE</b> at each.\n" +
            "<color=#66F280>Completed route: +1 point</color>   <color=#FF6666>Wrong stall: -1 point</color>\n" +
            "Reach <b>10 points</b> to win!",
            30, FontStyles.Normal, Color.white, TextAlignmentOptions.Left);
        howTo.lineSpacing = 12f;
        Place(howTo.gameObject, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1020f, 260f));

        CreateButton(menuPanel.transform, "START GAME", new Vector2(-210f, -250f), new Vector2(380f, 100f), PrimaryButton, StartGame);
        CreateButton(menuPanel.transform, "CONTROLS", new Vector2(210f, -250f), new Vector2(380f, 100f), SecondaryButton, () => OpenControls(menuPanel));

        TMP_Text footer = CreateText(menuPanel.transform, "ENTER = Start     C = Controls", 24, FontStyles.Normal, new Color(0.6f, 0.65f, 0.75f), TextAlignmentOptions.Center);
        Place(footer.gameObject, new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(1000f, 40f));
    }

    private void BuildControls()
    {
        controlsPanel = CreatePanel("Controls", transform, PanelColor);
        Stretch(controlsPanel);

        GameObject card = CreatePanel("Card", controlsPanel.transform, CardColor);
        Place(card, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(960f, 680f));
        AddAccentBar(card, SecondaryButton);

        TMP_Text title = CreateText(card.transform, "CONTROLS", 70, FontStyles.Bold, AccentColor, TextAlignmentOptions.Center);
        title.characterSpacing = 8f;
        Place(title.gameObject, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(900f, 100f));

        TMP_Text list = CreateText(card.transform,
            ControlRow("W  A  S  D", "Move") +
            ControlRow("MOUSE", "Look around") +
            ControlRow("SPACE", "Interact (Home Base / Stall)") +
            ControlRow("ESC", "Pause menu") +
            ControlRow("LEFT CLICK", "Re-capture mouse"),
            36, FontStyles.Normal, Color.white, TextAlignmentOptions.Left);
        list.lineSpacing = 30f;
        Place(list.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(780f, 380f));

        CreateButton(card.transform, "BACK", new Vector2(0f, -270f), new Vector2(300f, 90f), SecondaryButton, CloseControls);
    }

    private static string ControlRow(string key, string action)
    {
        return $"<color=#FFC233><b>{key}</b></color><pos=42%>{action}\n";
    }

    private void BuildPause()
    {
        pausePanel = CreatePanel("Pause", transform, PanelColor);
        Stretch(pausePanel);

        TMP_Text title = CreateText(pausePanel.transform, "PAUSED", 100, FontStyles.Bold, AccentColor, TextAlignmentOptions.Center);
        Place(title.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(1000f, 140f));

        CreateButton(pausePanel.transform, "RESUME", new Vector2(0f, 70f), new Vector2(420f, 95f), PrimaryButton, Resume);
        CreateButton(pausePanel.transform, "CONTROLS", new Vector2(0f, -50f), new Vector2(420f, 95f), SecondaryButton, () => OpenControls(pausePanel));
        CreateButton(pausePanel.transform, "MAIN MENU", new Vector2(0f, -170f), new Vector2(420f, 95f), DangerButton, QuitToMenu);
    }

    private void BuildWin()
    {
        winPanel = CreatePanel("Win", transform, PanelColor);
        Stretch(winPanel);

        TMP_Text title = CreateText(winPanel.transform, "YOU WON!", 140, FontStyles.Bold, AccentColor, TextAlignmentOptions.Center);
        title.outlineWidth = 0.15f;
        title.outlineColor = new Color32(0, 0, 0, 255);
        Place(title.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(1400f, 180f));

        winStatsText = CreateText(winPanel.transform, "", 48, FontStyles.Normal, Color.white, TextAlignmentOptions.Center);
        winStatsText.lineSpacing = 20f;
        Place(winStatsText.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1000f, 160f));

        CreateButton(winPanel.transform, "PLAY AGAIN", new Vector2(-230f, -150f), new Vector2(400f, 100f), PrimaryButton, PlayAgain);
        CreateButton(winPanel.transform, "QUIT", new Vector2(230f, -150f), new Vector2(400f, 100f), DangerButton, QuitGame);
    }

    // ---------- UI helpers ----------

    private static GameObject CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject go = CreateRect(name, parent);
        Image image = go.AddComponent<Image>();
        image.color = color;
        return go;
    }

    private static TMP_Text CreateText(Transform parent, string text, float size, FontStyles style, Color color, TextAlignmentOptions alignment)
    {
        GameObject go = CreateRect("Text", parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }

    private static Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, Color color, UnityAction onClick)
    {
        GameObject go = CreateRect("Button_" + label, parent);
        Place(go, new Vector2(0.5f, 0.5f), position, size);

        Image image = go.AddComponent<Image>();
        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        shadow.effectDistance = new Vector2(0f, -6f);

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.25f);
        colors.selectedColor = color;
        colors.pressedColor = Color.Lerp(color, Color.black, 0.25f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.onClick.AddListener(onClick);

        TMP_Text text = CreateText(go.transform, label, 38, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
        text.characterSpacing = 4f;
        Stretch(text.gameObject);
        return button;
    }

    private static void AddAccentBar(GameObject panel, Color color)
    {
        GameObject bar = CreatePanel("Accent", panel.transform, color);
        RectTransform rt = (RectTransform)bar.transform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, 6f);
    }

    private static void Place(GameObject go, Vector2 anchor, Vector2 position, Vector2 size)
    {
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    private static void Stretch(GameObject go)
    {
        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}

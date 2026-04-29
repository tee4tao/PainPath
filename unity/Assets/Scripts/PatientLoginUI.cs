using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// World-space numpad → enter 6-digit patient number → confirm "Are you Patient {id}?".
/// In demo mode any 6-digit input is accepted (no Firebase lookup yet).
/// On confirm: writes id + display name to PainDataStore, hides itself, and
/// invokes onLoginComplete which OnboardingUI listens for.
public class PatientLoginUI : MonoBehaviour
{
    [Header("References")]
    public PainDataStore painDataStore;
    public Transform     cameraTransform;

    [Header("Settings")]
    public float distanceFromCamera = 0.40f;
    public float heightOffset       = -0.05f;
    public int   patientIdLength    = 6;

    /// Subscribed to by OnboardingUI — fires when patient confirms identity.
    public System.Action onLoginComplete;

    private GameObject       loginPanel;
    private GameObject       confirmPanel;
    private TextMeshProUGUI  displayText;
    private TextMeshProUGUI  errorText;
    private string           inputBuffer = "";

    void Start()
    {
        BuildLoginPanel();
    }

    void Update()
    {
        if (loginPanel   != null && loginPanel.activeSelf)   PositionPanel(loginPanel);
        if (confirmPanel != null && confirmPanel.activeSelf) PositionPanel(confirmPanel);
    }

    void PositionPanel(GameObject p)
    {
        if (cameraTransform == null) return;
        Vector3 fwd = cameraTransform.forward; fwd.y = 0; fwd.Normalize();
        p.transform.position = cameraTransform.position + fwd * distanceFromCamera + Vector3.up * heightOffset;
        p.transform.LookAt(cameraTransform);
        p.transform.Rotate(0, 180f, 0);
    }

    // ── Login panel (numpad) ─────────────────────────────────────────────

    void BuildLoginPanel()
    {
        loginPanel = MakeCanvas("PatientLoginPanel", new Vector2(420, 480));

        AddBackground(loginPanel.transform, new Vector2(420, 480));

        AddLabel(loginPanel.transform, "Enter your patient number",
            new Vector2(380, 30), new Vector2(0, 215), 16, Color.white, FontStyles.Bold);

        AddLabel(loginPanel.transform, $"({patientIdLength} digits)",
            new Vector2(380, 20), new Vector2(0, 188), 11,
            new Color(0.65f, 0.65f, 0.65f), FontStyles.Normal);

        // Display window
        GameObject display = CreateRect("Display", loginPanel.transform,
            new Vector2(360, 50), new Vector2(0, 142));
        display.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.04f, 0.95f);

        displayText = CreateRect("DisplayText", display.transform,
            new Vector2(360, 50), Vector2.zero).AddComponent<TextMeshProUGUI>();
        displayText.text      = "";
        displayText.fontSize  = 28;
        displayText.fontStyle = FontStyles.Bold;
        displayText.alignment = TextAlignmentOptions.Center;
        displayText.color     = Color.white;

        // Error message slot (hidden until needed)
        errorText = CreateRect("Error", loginPanel.transform,
            new Vector2(380, 22), new Vector2(0, 108)).AddComponent<TextMeshProUGUI>();
        errorText.fontSize  = 11;
        errorText.alignment = TextAlignmentOptions.Center;
        errorText.color     = new Color(0.95f, 0.40f, 0.35f);
        errorText.text      = "";

        // ── 3×4 numpad — phone layout ────────────────────────────────────
        // 1 2 3 / 4 5 6 / 7 8 9 / ⌫ 0 ✓
        float keyW    = 95f;
        float keyH    = 55f;
        float spacing = 105f;
        float startX  = -spacing;            // 3 columns centred
        float startY  = 50f;                 // top row Y, drops by spacing per row

        string[] rows =
        {
            "1", "2", "3",
            "4", "5", "6",
            "7", "8", "9",
            "BACK", "0", "OK"
        };

        for (int i = 0; i < rows.Length; i++)
        {
            int col = i % 3;
            int row = i / 3;
            float x = startX + col * spacing;
            float y = startY - row * (keyH + 8f);

            string label = rows[i];
            Color  color = new Color(0.20f, 0.20f, 0.22f);
            string display_ = label;
            UnityAction act;

            if (label == "BACK")      { display_ = "⌫";  color = new Color(0.45f, 0.20f, 0.20f); act = OnBackspace; }
            else if (label == "OK")   { display_ = "✓";  color = new Color(0.18f, 0.52f, 0.30f); act = OnSubmit; }
            else                      { string captured = label; act = () => OnDigit(captured); }

            CreateButton(display_, loginPanel.transform,
                new Vector2(keyW, keyH), new Vector2(x, y), color, act);
        }
    }

    void OnDigit(string digit)
    {
        if (inputBuffer.Length >= patientIdLength) return;
        inputBuffer  += digit;
        UpdateDisplay();

        // Auto-submit when full length reached
        if (inputBuffer.Length == patientIdLength) OnSubmit();
    }

    void OnBackspace()
    {
        if (inputBuffer.Length == 0) return;
        inputBuffer = inputBuffer.Substring(0, inputBuffer.Length - 1);
        UpdateDisplay();
        if (errorText != null) errorText.text = "";
    }

    void OnSubmit()
    {
        if (inputBuffer.Length != patientIdLength)
        {
            if (errorText != null)
                errorText.text = $"Please enter all {patientIdLength} digits.";
            return;
        }

        // DEMO MODE: any 6-digit number is accepted. No Firebase call.
        ShowConfirmPanel(inputBuffer);
    }

    void UpdateDisplay()
    {
        // Show entered digits + dots for remaining slots, e.g. "12 3 · · ·"
        string shown = "";
        for (int i = 0; i < patientIdLength; i++)
        {
            shown += i < inputBuffer.Length ? inputBuffer[i].ToString() : "·";
            if (i < patientIdLength - 1) shown += "  ";
        }
        if (displayText != null) displayText.text = shown;
    }

    // ── Confirm panel ────────────────────────────────────────────────────

    void ShowConfirmPanel(string id)
    {
        if (loginPanel != null) loginPanel.SetActive(false);

        string displayName = $"Patient {id}";

        if (confirmPanel != null) Destroy(confirmPanel);
        confirmPanel = MakeCanvas("PatientConfirmPanel", new Vector2(380, 200));

        AddBackground(confirmPanel.transform, new Vector2(380, 200));

        AddLabel(confirmPanel.transform, "Are you",
            new Vector2(340, 24), new Vector2(0, 60), 13,
            new Color(0.75f, 0.75f, 0.75f), FontStyles.Normal);

        AddLabel(confirmPanel.transform, displayName,
            new Vector2(340, 36), new Vector2(0, 28), 22, Color.white, FontStyles.Bold);

        AddLabel(confirmPanel.transform, "?", new Vector2(340, 24),
            new Vector2(0, 4), 14, new Color(0.75f, 0.75f, 0.75f), FontStyles.Normal);

        CreateButton("No", confirmPanel.transform,
            new Vector2(150, 36), new Vector2(-90, -55),
            new Color(0.30f, 0.30f, 0.30f), () => OnConfirmNo());

        CreateButton("Yes, that's me", confirmPanel.transform,
            new Vector2(180, 36), new Vector2(85, -55),
            new Color(0.18f, 0.52f, 0.30f), () => OnConfirmYes(id, displayName));
    }

    void OnConfirmNo()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
        inputBuffer = "";
        UpdateDisplay();
        if (errorText != null) errorText.text = "";
        if (loginPanel != null) loginPanel.SetActive(true);
    }

    void OnConfirmYes(string id, string displayName)
    {
        if (painDataStore != null)
            painDataStore.SetPatient(id, displayName);

        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (loginPanel   != null) loginPanel.SetActive(false);

        onLoginComplete?.Invoke();
    }

    // ── UI helpers (same pattern as PainTypeUI / CompletionUI) ───────────

    GameObject MakeCanvas(string name, Vector2 size)
    {
        GameObject go = new GameObject(name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode  = RenderMode.WorldSpace;
        c.worldCamera = Camera.main;

        var rc = go.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
        rc.checkFor3DOcclusion = false;
        go.AddComponent<CanvasScaler>();

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta  = size;
        rt.localScale = Vector3.one * 0.001f;
        return go;
    }

    void AddBackground(Transform parent, Vector2 size)
    {
        GameObject bg = CreateRect("BG", parent, size, Vector2.zero);
        bg.AddComponent<Image>().color = new Color(0.07f, 0.07f, 0.07f, 0.95f);
    }

    void AddLabel(Transform parent, string text, Vector2 size, Vector2 pos,
                  float fontSize, Color color, FontStyles style)
    {
        TextMeshProUGUI t = CreateRect("Lbl", parent, size, pos)
            .AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.color     = color;
    }

    GameObject CreateButton(string label, Transform parent,
        Vector2 size, Vector2 pos, Color btnColor, UnityAction action)
    {
        GameObject btn = CreateRect(label, parent, size, pos);

        Image img = btn.AddComponent<Image>();
        img.color = btnColor;

        Button button = btn.AddComponent<Button>();
        button.targetGraphic = img;
        button.onClick.AddListener(action);

        BoxCollider col = btn.AddComponent<BoxCollider>();
        col.size   = new Vector3(size.x * 0.001f, size.y * 0.001f, 0.012f);
        col.center = new Vector3(0, 0, 0.006f);

        TextMeshProUGUI t = CreateRect("L", btn.transform, size, Vector2.zero)
            .AddComponent<TextMeshProUGUI>();
        t.text      = label;
        t.fontSize  = 18;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color     = Color.white;

        return btn;
    }

    GameObject CreateRect(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
        return go;
    }
}

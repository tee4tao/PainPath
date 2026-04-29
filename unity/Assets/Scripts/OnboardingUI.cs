using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OnboardingUI : MonoBehaviour
{
    [Header("References")]
    public PainDataStore      painDataStore;
    public PainTypeUI         painTypeUI;
    public HeightCalibration  heightCalibration;
    public PatientLoginUI     patientLoginUI;   // gates onboarding until login finishes
    public Transform          cameraTransform;

    [Header("Settings")]
    public float distanceFromCamera = 0.40f;
    public float heightOffset       = 0.05f;

    private GameObject panel;

    void Start()
    {
        // Hide main UI until onboarding is dismissed
        if (painTypeUI != null) painTypeUI.SetPanelVisible(false);

        if (patientLoginUI != null)
        {
            // Gate onboarding behind patient login. The login UI builds itself
            // on its own Start() — we just listen for completion.
            patientLoginUI.onLoginComplete = StartOnboarding;
        }
        else
        {
            // No login wired — fall back to old behaviour
            StartOnboarding();
        }
    }

    void StartOnboarding()
    {
        // Welcome panel stays up until patient taps Begin — no auto-dismiss
        BuildPanel();
    }

    void Update()
    {
        if (panel == null || !panel.activeSelf || cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        panel.transform.position = cameraTransform.position
            + forward * distanceFromCamera
            + Vector3.up * heightOffset;

        panel.transform.LookAt(cameraTransform);
        panel.transform.Rotate(0, 180f, 0);
    }

    void BuildPanel()
    {
        // Taller panel to fit fuller instructions
        Vector2 panelSize = new Vector2(420, 320);

        panel = new GameObject("OnboardingPanel");
        Canvas c = panel.AddComponent<Canvas>();
        c.renderMode  = RenderMode.WorldSpace;
        c.worldCamera = Camera.main;

        panel.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
        panel.AddComponent<CanvasScaler>();

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.sizeDelta  = panelSize;
        rt.localScale = Vector3.one * 0.001f;

        // Background
        GameObject bg = CreateRect("BG", panel.transform, panelSize, Vector2.zero);
        bg.AddComponent<Image>().color = new Color(0.07f, 0.07f, 0.07f, 0.95f);

        // Greeting
        string name    = painDataStore != null ? painDataStore.patientName : "there";
        string priorStr = "";
        if (painDataStore != null && painDataStore.priorAppointments > 0)
            priorStr = $"  This builds on your {painDataStore.priorAppointments} previous appointment(s).";

        AddLabel($"Welcome, {name}.",
            new Vector2(380, 36), new Vector2(0, 125), 18, Color.white, FontStyles.Bold);

        AddLabel("This goes directly to your specialist." + priorStr,
            new Vector2(380, 30), new Vector2(0, 92), 12,
            new Color(0.82f, 0.82f, 0.82f), FontStyles.Normal);

        // Instruction block — divider + bullets
        AddLabel("HOW TO USE",
            new Vector2(380, 22), new Vector2(0, 56),
            10, new Color(0.55f, 0.65f, 0.85f), FontStyles.Bold);

        AddLabel("👉  Use either index finger to touch the body where it hurts.",
            new Vector2(380, 26), new Vector2(0, 30), 12,
            new Color(0.92f, 0.92f, 0.92f), FontStyles.Normal);

        AddLabel("🎯  Pick the pain TYPE and INTENSITY first, then touch.",
            new Vector2(380, 26), new Vector2(0, 4), 12,
            new Color(0.92f, 0.92f, 0.92f), FontStyles.Normal);

        AddLabel("↩  Use Undo / Rotate / Submit when ready.",
            new Vector2(380, 26), new Vector2(0, -22), 12,
            new Color(0.92f, 0.92f, 0.92f), FontStyles.Normal);

        AddLabel("There are no wrong answers — take your time.",
            new Vector2(380, 24), new Vector2(0, -58), 11,
            new Color(0.60f, 0.60f, 0.60f), FontStyles.Italic);

        // Begin button
        GameObject btn = CreateRect("Begin", panel.transform, new Vector2(300, 42), new Vector2(0, -110));
        Image img = btn.AddComponent<Image>();
        img.color = new Color(0.18f, 0.45f, 0.72f);

        Button button = btn.AddComponent<Button>();
        button.targetGraphic = img;
        button.onClick.AddListener(Dismiss);

        BoxCollider col = btn.AddComponent<BoxCollider>();
        col.size   = new Vector3(0.300f, 0.042f, 0.012f);
        col.center = new Vector3(0, 0, 0.006f);

        GameObject lbl = CreateRect("L", btn.transform, new Vector2(300, 42), Vector2.zero);
        TextMeshProUGUI t = lbl.AddComponent<TextMeshProUGUI>();
        t.text      = "Begin";
        t.fontSize  = 16;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color     = Color.white;
    }

    void Dismiss()
    {
        if (panel != null) panel.SetActive(false);

        if (painTypeUI != null)       painTypeUI.SetPanelVisible(true);
        if (heightCalibration != null) heightCalibration.Calibrate();
    }

    void AddLabel(string text, Vector2 size, Vector2 pos,
                  float fontSize, Color color, FontStyles style)
    {
        GameObject go = CreateRect("Label", panel.transform, size, pos);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = color;
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

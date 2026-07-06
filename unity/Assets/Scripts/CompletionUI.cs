using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class CompletionUI : MonoBehaviour
{
    [Header("References")]
    public PainTypeUI           painTypeUI;
    public PainDataStore        painDataStore;
    public FirebaseUploader     firebaseUploader;     // legacy, kept for compatibility
    public SessionUploader      sessionUploader;      // POST to Next.js API
    public HandRaycastPainter   handRaycastPainter;   // disabled after submit so user can't draw
    public PainDetailsRecapUI   recapUI;              // pre-review enrichment form (optional)
    public Transform            cameraTransform;

    [Header("Panel Settings")]
    public float distanceFromCamera = 0.35f;
    public float heightOffset       = 0.0f;

    private GameObject reviewPanel;
    private GameObject exitPanel;

    void Start()
    {
        // When the patient hits Submit (or "I'm done" from the idle prompt),
        // route through the recap form first if it's wired, then show the
        // final Review panel. If no recap UI is assigned, go straight to Review.
        if (painTypeUI != null)
            painTypeUI.onDoneRequested = OnSubmitTapped;

        if (recapUI != null)
            recapUI.onRecapComplete = ShowReviewPanel;
    }

    void OnSubmitTapped()
    {
        // Hide painting UI + lock the painter from this point on
        if (painTypeUI != null)         painTypeUI.SetPanelVisible(false);
        if (handRaycastPainter != null) handRaycastPainter.enabled = false;

        if (recapUI != null)
            recapUI.Show();    // fires onRecapComplete → ShowReviewPanel
        else
            ShowReviewPanel(); // no recap wired → skip straight to review
    }

    void Update()
    {
        PositionPanel(reviewPanel);
        PositionPanel(exitPanel);
    }

    void PositionPanel(GameObject p)
    {
        if (p == null || !p.activeSelf || cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        p.transform.position = cameraTransform.position
            + forward * distanceFromCamera
            + Vector3.up * heightOffset;

        p.transform.LookAt(cameraTransform);
        p.transform.Rotate(0, 180f, 0);
    }

    // ── Review panel ─────────────────────────────────────────────────────

    public void ShowReviewPanel()
    {
        // Defensive: painting UI hidden + painter disabled. Usually already done by
        // OnSubmitTapped, but guard against direct calls (e.g. recap → review path).
        if (painTypeUI != null)         painTypeUI.SetPanelVisible(false);
        if (handRaycastPainter != null) handRaycastPainter.enabled = false;

        if (reviewPanel != null) { reviewPanel.SetActive(true); return; }

        reviewPanel = BuildCanvas("ReviewPanel", new Vector2(340, 160));

        // Background
        AddBackground(reviewPanel, new Vector2(340, 160));

        // Heading
        AddLabel(reviewPanel, "Is this your pain?",
            new Vector2(300, 32), new Vector2(0, 55), 18, Color.white, FontStyles.Bold);

        // Sub-message
        AddLabel(reviewPanel, "Your specialist will review this before seeing you.",
            new Vector2(300, 36), new Vector2(0, 15), 12,
            new Color(0.75f, 0.75f, 0.75f), FontStyles.Normal);

        // Edit button
        AddButton(reviewPanel, "Edit", new Vector2(140, 34), new Vector2(-85, -38),
            new Color(0.30f, 0.30f, 0.30f), OnEdit);

        // Confirm button
        AddButton(reviewPanel, "Confirm & Submit", new Vector2(160, 34), new Vector2(85, -38),
            new Color(0.18f, 0.52f, 0.32f), OnConfirm);
    }

    void OnEdit()
    {
        if (reviewPanel != null) reviewPanel.SetActive(false);
        if (recapUI     != null) recapUI.Hide();
        if (painTypeUI  != null) painTypeUI.SetPanelVisible(true);

        // Allow drawing again — the next Submit will re-show the recap
        // (preserving any selections the patient already made — see
        // PainDataStore.regionDetails which the recap reads on Show())
        if (handRaycastPainter != null) handRaycastPainter.enabled = true;
    }

    void OnConfirm()
    {
        // Painter stays disabled — session is locked once confirmed
        if (handRaycastPainter != null) handRaycastPainter.enabled = false;

        // Prefer the new Next.js uploader if assigned, fall back to FirebaseUploader, then local
        if (sessionUploader != null)
            sessionUploader.Upload(painDataStore);
        else if (firebaseUploader != null)
            firebaseUploader.Upload(painDataStore);
        else
            SaveLocal();

        if (reviewPanel != null) reviewPanel.SetActive(false);
        ShowExitPanel();
    }

    void SaveLocal()
    {
        if (painDataStore == null) return;
        string json = painDataStore.GetSessionJSON();
        string path = Path.Combine(Application.persistentDataPath,
                                   $"session_{painDataStore.currentSession.sessionId}.json");
        File.WriteAllText(path, json);
    }

    // ── Exit panel ───────────────────────────────────────────────────────

    void ShowExitPanel()
    {
        if (exitPanel != null) { exitPanel.SetActive(true); return; }

        exitPanel = BuildCanvas("ExitPanel", new Vector2(340, 130));

        AddBackground(exitPanel, new Vector2(340, 130));

        string specialist   = painDataStore != null ? painDataStore.specialistName    : "your specialist";
        string appointment  = painDataStore != null ? painDataStore.appointmentTime   : "";

        string line1 = $"Dr {specialist} will review your map";
        string line2 = appointment.Length > 0
            ? $"before your appointment at {appointment}."
            : "before your appointment.";

        AddLabel(exitPanel, line1,
            new Vector2(310, 30), new Vector2(0, 28), 14, Color.white, FontStyles.Bold);

        AddLabel(exitPanel, line2,
            new Vector2(310, 26), new Vector2(0, -2), 13,
            new Color(0.80f, 0.80f, 0.80f), FontStyles.Normal);

        AddLabel(exitPanel, "You can now remove the headset.",
            new Vector2(310, 22), new Vector2(0, -32), 12,
            new Color(0.55f, 0.55f, 0.55f), FontStyles.Normal);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    GameObject BuildCanvas(string name, Vector2 size)
    {
        GameObject go = new GameObject(name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode  = RenderMode.WorldSpace;
        c.worldCamera = Camera.main;

        go.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
        go.AddComponent<CanvasScaler>();

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta  = size;
        rt.localScale = Vector3.one * 0.001f;
        return go;
    }

    void AddBackground(GameObject parent, Vector2 size)
    {
        GameObject bg = CreateRect("BG", parent.transform, size, Vector2.zero);
        Image img = bg.AddComponent<Image>();
        img.color = new Color(0.08f, 0.08f, 0.08f, 0.93f);
    }

    void AddLabel(GameObject parent, string text, Vector2 size, Vector2 pos,
                  float fontSize, Color color, FontStyles style)
    {
        GameObject go = CreateRect("Label", parent.transform, size, pos);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = color;
    }

    void AddButton(GameObject parent, string label, Vector2 size, Vector2 pos,
                   Color btnColor, System.Action action)
    {
        GameObject btn = CreateRect(label, parent.transform, size, pos);

        Image img = btn.AddComponent<Image>();
        img.color = btnColor;

        Button button = btn.AddComponent<Button>();
        button.targetGraphic = img;
        button.onClick.AddListener(() => action());

        BoxCollider col = btn.AddComponent<BoxCollider>();
        col.size   = new Vector3(size.x * 0.001f, size.y * 0.001f, 0.01f);
        col.center = new Vector3(0, 0, 0.005f);

        GameObject labelObj = CreateRect("L", btn.transform, size, Vector2.zero);
        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 13;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
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

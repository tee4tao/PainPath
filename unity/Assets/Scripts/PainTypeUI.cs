using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class PainTypeUI : MonoBehaviour
{
    public PainDataStore      painDataStore;
    public HandRaycastPainter handRaycastPainter;
    public BodyRotator        bodyRotator;
    public Transform          cameraTransform;

    [Header("Panel Settings")]
    public float distanceFromCamera = 0.32f;
    public float heightOffset       = -0.12f;

    // Callback hooked by CompletionUI
    public System.Action onDoneRequested;

    private GameObject   panel;
    private GameObject   areaPrompt;
    private GameObject[] segments     = new GameObject[10];
    private GameObject[] typeButtons  = new GameObject[3];
    private readonly string[] painTypes  = { "ache", "stiff", "sharp" };
    private readonly string[] typeLabels = { "Ache", "Stiff", "Sharp" };

    // ── Lifecycle ─────────────────────────────────────────────────────────

    public void SetPanelVisible(bool visible)
    {
        if (panel != null) panel.SetActive(visible);
    }

    void Start()  => BuildPanel();

    void Update()
    {
        if (panel != null && panel.activeSelf)
            PositionPanel(panel, heightOffset);
    }

    void LateUpdate()
    {
        // Area prompt floats above the main panel using the same camera logic
        // Panel height = 290px * 0.001 = 0.29m  →  half = 0.145m
        // Prompt height =  90px * 0.001 = 0.09m  →  half = 0.045m
        // Gap = 0.01m  →  total offset = 0.145 + 0.045 + 0.01 = 0.20m above panel centre
        if (areaPrompt != null && areaPrompt.activeSelf)
            PositionPanel(areaPrompt, heightOffset + 0.20f);
    }

    void PositionPanel(GameObject p, float yOffset)
    {
        if (p == null || cameraTransform == null) return;
        Vector3 fwd = cameraTransform.forward; fwd.y = 0; fwd.Normalize();
        p.transform.position = cameraTransform.position + fwd * distanceFromCamera + Vector3.up * yOffset;
        p.transform.LookAt(cameraTransform);
        p.transform.Rotate(0, 180f, 0);
    }

    // ── Panel build ───────────────────────────────────────────────────────

    void BuildPanel()
    {
        // Panel grew to fit a pain-type row above the intensity bar
        Vector2 panelSize = new Vector2(390, 290);
        panel = MakeCanvas("PainTypePanel", panelSize);

        // Background
        CreateRect("BG", panel.transform, panelSize, Vector2.zero)
            .AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.92f);

        // ── Pain type row ────────────────────────────────────────────────
        AddLabel(panel.transform, "PAIN TYPE",
            new Vector2(380, 22), new Vector2(0, 125),
            11, new Color(0.60f, 0.60f, 0.60f), FontStyles.Bold);

        // 3 buttons: Aching | Burning | Sharp
        float typeBtnW = 118f;
        float typeBtnH = 38f;
        float typeGap  = 6f;
        float typeStartX = -(typeBtnW + typeGap);   // centre row of 3
        float typeY      = 92f;

        for (int i = 0; i < 3; i++)
        {
            int captured = i;
            string id    = painTypes[i];
            string label = typeLabels[i];

            GameObject b = CreateButton(label, panel.transform,
                new Vector2(typeBtnW, typeBtnH),
                new Vector2(typeStartX + i * (typeBtnW + typeGap), typeY),
                new Color(0.20f, 0.22f, 0.28f),
                () => SelectPainType(captured));

            typeButtons[i] = b;
        }

        // ── "INTENSITY" label ────────────────────────────────────────────
        AddLabel(panel.transform, "INTENSITY",
            new Vector2(380, 22), new Vector2(0, 60),
            11, new Color(0.60f, 0.60f, 0.60f), FontStyles.Bold);

        // ── 10 gradient segments ─────────────────────────────────────────
        float segW    = 28f;
        float segH    = 50f;
        float spacing = 33f;
        float startX  = -(spacing * 4.5f);   // -148.5
        float segY    = 22f;

        for (int i = 0; i < 10; i++)
        {
            int   intensity = i + 1;
            float x         = startX + i * spacing;
            Color col       = IntensityColour(intensity);

            GameObject seg = CreateRect("Seg" + intensity, panel.transform,
                new Vector2(segW, segH), new Vector2(x, segY));

            Image img = seg.AddComponent<Image>();
            img.color = new Color(col.r, col.g, col.b, 0.45f);

            BoxCollider bc = seg.AddComponent<BoxCollider>();
            bc.size   = new Vector3(segW * 0.001f, segH * 0.001f, 0.012f);
            bc.center = new Vector3(0, 0, 0.006f);

            Button btn = seg.AddComponent<Button>();
            btn.targetGraphic = img;
            int captured = intensity;
            btn.onClick.AddListener(() => SelectIntensity(captured));

            segments[i] = seg;
        }

        // "1 — mild" and "10 — severe" end labels
        AddLabel(panel.transform, "1 — mild",
            new Vector2(100, 18), new Vector2(-144f, -10f),
            9, new Color(0.50f, 0.50f, 0.50f), FontStyles.Normal);

        AddLabel(panel.transform, "10 — severe",
            new Vector2(100, 18), new Vector2(144f, -10f),
            9, new Color(0.50f, 0.50f, 0.50f), FontStyles.Normal);

        // ── Undo + Rotate ────────────────────────────────────────────────
        CreateButton("↩ Undo", panel.transform,
            new Vector2(175, 30), new Vector2(-98f, -52f),
            new Color(0.22f, 0.30f, 0.50f), () => handRaycastPainter?.UndoLastMarker());

        CreateButton("↻ Rotate", panel.transform,
            new Vector2(175, 30), new Vector2(98f, -52f),
            new Color(0.15f, 0.35f, 0.28f), () => bodyRotator?.ToggleView());

        // ── Submit ───────────────────────────────────────────────────────
        CreateButton("Submit", panel.transform,
            new Vector2(360, 38), new Vector2(0, -100f),
            new Color(0.14f, 0.52f, 0.26f), () => onDoneRequested?.Invoke());

        // Highlight defaults
        int defaultIntensity = painDataStore != null ? painDataStore.currentIntensity : 5;
        SelectIntensity(defaultIntensity);

        string defaultType = painDataStore != null && !string.IsNullOrEmpty(painDataStore.currentPainType)
            ? painDataStore.currentPainType : "ache";
        int defaultTypeIdx = System.Array.IndexOf(painTypes, defaultType);
        SelectPainType(defaultTypeIdx >= 0 ? defaultTypeIdx : 0);
    }

    // ── Pain type selection ───────────────────────────────────────────────

    void SelectPainType(int index)
    {
        if (index < 0 || index >= painTypes.Length) return;

        if (painDataStore != null)
            painDataStore.currentPainType = painTypes[index];

        // Highlight chosen, dim the rest
        for (int i = 0; i < typeButtons.Length; i++)
        {
            if (typeButtons[i] == null) continue;
            Image img = typeButtons[i].GetComponent<Image>();
            if (img == null) continue;

            img.color = (i == index)
                ? new Color(0.38f, 0.50f, 0.78f)        // selected — bright blue
                : new Color(0.20f, 0.22f, 0.28f);       // dimmed
        }
    }

    // ── Intensity selection ───────────────────────────────────────────────

    void SelectIntensity(int intensity)
    {
        if (painDataStore != null)
            painDataStore.currentIntensity = intensity;

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null) continue;

            bool       selected = (i + 1) == intensity;
            Color      c        = IntensityColour(i + 1);
            Image      img      = segments[i].GetComponent<Image>();
            RectTransform rt    = segments[i].GetComponent<RectTransform>();

            img.color    = selected ? c : new Color(c.r, c.g, c.b, 0.40f);
            rt.sizeDelta = selected ? new Vector2(28f, 60f) : new Vector2(28f, 50f);
        }
    }

    // ── Area prompt (idle / "another area?") ─────────────────────────────

    public void ShowAreaPrompt()
    {
        if (areaPrompt != null) { areaPrompt.SetActive(true); return; }

        areaPrompt = MakeCanvas("AreaPrompt", new Vector2(310, 90));

        CreateRect("BG", areaPrompt.transform, new Vector2(310, 90), Vector2.zero)
            .AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.93f);

        AddLabel(areaPrompt.transform, "Is there another area?",
            new Vector2(290, 26), new Vector2(0, 22f),
            14, Color.white, FontStyles.Bold);

        CreateButton("Yes, continue", areaPrompt.transform,
            new Vector2(132, 28), new Vector2(-76f, -16f),
            new Color(0.18f, 0.52f, 0.28f), () => {
                HideAreaPrompt();
                handRaycastPainter?.ResetIdleTimer();
            });

        CreateButton("I'm done", areaPrompt.transform,
            new Vector2(132, 28), new Vector2(76f, -16f),
            new Color(0.25f, 0.32f, 0.55f), () => {
                HideAreaPrompt();
                onDoneRequested?.Invoke();
            });
    }

    public void HideAreaPrompt()
    {
        if (areaPrompt != null) areaPrompt.SetActive(false);
    }

    // ── Shared helpers ────────────────────────────────────────────────────

    // Journey-map colour bar: 1 = cool blue → 5 = orange → 10 = deep red/brown
    Color IntensityColour(int intensity)
    {
        float t      = Mathf.Clamp01((intensity - 1) / 9f);
        Color blue   = new Color(0.20f, 0.50f, 0.90f);
        Color orange = new Color(0.95f, 0.60f, 0.10f);
        Color red    = new Color(0.45f, 0.10f, 0.05f);

        return t <= 0.5f
            ? Color.Lerp(blue, orange, t * 2f)
            : Color.Lerp(orange, red, (t - 0.5f) * 2f);
    }

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

    void AddLabel(Transform parent, string text, Vector2 size, Vector2 pos,
                  float fontSize, Color color, FontStyles style)
    {
        TextMeshProUGUI t = CreateRect("Lbl", parent, size, pos).AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.color     = color;
    }

    public GameObject CreateButton(string label, Transform parent,
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
        t.fontSize  = 13;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color     = Color.white;

        return btn;
    }

    public GameObject CreateRect(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = size;
        rt.anchoredPosition = pos;
        return go;
    }
}

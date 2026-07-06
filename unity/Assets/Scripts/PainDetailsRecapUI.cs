using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections.Generic;

/// Recap form that appears between Submit and the final Review panel.
/// For each affected body region, the patient picks pattern + duration + triggers
/// using preset chips. All fields are skippable — defaults are "unspecified" / empty.
///
/// Single-card pagination: shows one region at a time with Back / Next buttons.
/// On the last region, Next becomes "Continue →" and commits selections.
/// On Continue, region details are written to PainDataStore and onRecapComplete fires.
public class PainDetailsRecapUI : MonoBehaviour
{
    [Header("References")]
    public PainDataStore painDataStore;
    public Transform     cameraTransform;

    [Header("Panel Settings")]
    public float distanceFromCamera = 0.42f;
    public float heightOffset       = -0.05f;

    /// Subscribed to by CompletionUI — fires when patient hits Continue.
    /// Also fires immediately (without showing the panel) if there are no zones to enrich.
    public System.Action onRecapComplete;

    private GameObject panel;

    // Static option lists
    private readonly string[] patternIds   = { "constant", "comes_and_goes", "worse_with_movement", "worse_at_rest" };
    private readonly string[] patternLabel = { "Constant", "Comes & Goes",   "Worse w/ Movement",   "Worse at Rest" };

    private readonly string[] durationIds   = { "today", "few_days", "weeks", "months", "years" };
    private readonly string[] durationLabel = { "Today", "Few days", "Weeks", "Months", "Years" };

    private readonly string[] triggerIds   = { "morning", "night", "exercise", "sitting", "standing", "stress" };
    private readonly string[] triggerLabel = { "Morning", "Night", "Exercise", "Sitting", "Standing", "Stress" };

    // Per-region state — selections kept in memory until Continue commits them
    private class RegionEdit
    {
        public string       bodyPart;
        public string       pattern  = "unspecified";
        public string       duration = "unspecified";
        public List<string> triggers = new List<string>();
        public List<GameObject> patternChips  = new List<GameObject>();
        public List<GameObject> durationChips = new List<GameObject>();
        public List<GameObject> triggerChips  = new List<GameObject>();
    }
    private List<RegionEdit> regionEdits = new List<RegionEdit>();

    // Pagination state
    private List<GameObject>  regionCards     = new List<GameObject>();
    private int               currentIndex    = 0;
    private TextMeshProUGUI   regionIndicator;
    private GameObject        backButton;
    private TextMeshProUGUI   nextButtonLabel;

    // ── Public entry ──────────────────────────────────────────────────────

    public void Show()
    {
        if (painDataStore == null) { onRecapComplete?.Invoke(); return; }

        List<string> regions = painDataStore.GetAffectedBodyParts();
        if (regions.Count == 0) { onRecapComplete?.Invoke(); return; }

        if (panel != null) Destroy(panel);
        BuildPanel(regions);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (panel == null || !panel.activeSelf || cameraTransform == null) return;

        Vector3 fwd = cameraTransform.forward; fwd.y = 0; fwd.Normalize();
        panel.transform.position = cameraTransform.position
            + fwd * distanceFromCamera
            + Vector3.up * heightOffset;
        panel.transform.LookAt(cameraTransform);
        panel.transform.Rotate(0, 180f, 0);
    }

    // ── Panel build ───────────────────────────────────────────────────────

    void BuildPanel(List<string> regions)
    {
        regionEdits.Clear();
        regionCards.Clear();
        currentIndex = 0;

        // Compact panel — only one card visible at a time
        Vector2 panelSize = new Vector2(560, 540);
        panel = MakeCanvas("PainDetailsRecapPanel", panelSize);

        // Background
        CreateRect("BG", panel.transform, panelSize, Vector2.zero)
            .AddComponent<Image>().color = new Color(0.07f, 0.07f, 0.07f, 0.96f);

        // ── Header ───────────────────────────────────────────────────────
        AddLabel(panel.transform, "Tell us a bit more",
            new Vector2(540, 30), new Vector2(0, 235), 18, Color.white, FontStyles.Bold);

        AddLabel(panel.transform, "Optional — helps your physio understand your pain.",
            new Vector2(540, 20), new Vector2(0, 208), 11,
            new Color(0.70f, 0.70f, 0.70f), FontStyles.Italic);

        // Region indicator "Region X of N"
        GameObject indicator = CreateRect("Indicator", panel.transform,
            new Vector2(540, 22), new Vector2(0, 180));
        regionIndicator = indicator.AddComponent<TextMeshProUGUI>();
        regionIndicator.text      = "";
        regionIndicator.fontSize  = 12;
        regionIndicator.fontStyle = FontStyles.Bold;
        regionIndicator.alignment = TextAlignmentOptions.Center;
        regionIndicator.color     = new Color(0.95f, 0.78f, 0.20f);

        // ── Card stack — all cards overlap, only the current one is visible ─
        // Each card is parented directly to the panel at the same Y position.
        // ShowCard(i) toggles SetActive between them.
        float cardHeight = 270f;
        float cardCenterY = 10f;   // card centered slightly below panel centre

        for (int i = 0; i < regions.Count; i++)
        {
            GameObject card = BuildRegionCard(panel.transform, regions[i],
                cardCenterY + cardHeight * 0.5f,   // yTop relative to panel
                cardHeight);
            regionCards.Add(card);
            card.SetActive(false);
        }

        // ── Navigation buttons (bottom row) ──────────────────────────────
        // Back on the left, Next/Continue on the right
        backButton = CreateButton("← Back", panel.transform,
            new Vector2(160, 42), new Vector2(-110, -215),
            new Color(0.30f, 0.30f, 0.30f), OnBack);

        GameObject nextBtn = CreateButton("Next →", panel.transform,
            new Vector2(220, 42), new Vector2(80, -215),
            new Color(0.18f, 0.52f, 0.30f), OnNext);

        // Cache the Next button's label so we can rename it to "Continue →" on the last card
        Transform lbl = nextBtn.transform.Find("L");
        if (lbl != null) nextButtonLabel = lbl.GetComponent<TextMeshProUGUI>();

        // Show the first card
        ShowCard(0);
    }

    // ── Pagination ────────────────────────────────────────────────────────

    void ShowCard(int index)
    {
        currentIndex = Mathf.Clamp(index, 0, regionCards.Count - 1);

        for (int i = 0; i < regionCards.Count; i++)
            if (regionCards[i] != null)
                regionCards[i].SetActive(i == currentIndex);

        // Update header indicator
        if (regionIndicator != null)
            regionIndicator.text = $"Region {currentIndex + 1} of {regionCards.Count}";

        // Hide Back on the first card
        if (backButton != null)
            backButton.SetActive(currentIndex > 0);

        // Rename Next → Continue on the last card
        bool isLast = currentIndex == regionCards.Count - 1;
        if (nextButtonLabel != null)
            nextButtonLabel.text = isLast ? "Continue →" : "Next →";
    }

    void OnNext()
    {
        if (currentIndex == regionCards.Count - 1)
            OnContinue();
        else
            ShowCard(currentIndex + 1);
    }

    void OnBack()
    {
        if (currentIndex > 0)
            ShowCard(currentIndex - 1);
    }

    // ── Card build ────────────────────────────────────────────────────────

    GameObject BuildRegionCard(Transform parent, string bodyPart, float yTop, float cardHeight)
    {
        RegionEdit edit = new RegionEdit { bodyPart = bodyPart };

        // Pre-populate from existing details if this is a re-visit
        RegionDetails existing = painDataStore != null ? painDataStore.GetRegionDetails(bodyPart) : null;
        if (existing != null)
        {
            edit.pattern  = existing.pattern  ?? "unspecified";
            edit.duration = existing.duration ?? "unspecified";
            edit.triggers = existing.triggers != null ? new List<string>(existing.triggers) : new List<string>();
        }
        regionEdits.Add(edit);

        // Card container — placed so its TOP is at yTop
        GameObject card = CreateRect("Card_" + bodyPart, parent,
            new Vector2(500, cardHeight),
            new Vector2(0, yTop - cardHeight * 0.5f));

        card.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.14f, 0.95f);

        int markerCount = painDataStore != null ? painDataStore.MarkerCountForRegion(bodyPart) : 0;
        string header = $"{PrettyName(bodyPart)}  ·  {markerCount} marker{(markerCount == 1 ? "" : "s")}";

        AddLabel(card.transform, header,
            new Vector2(480, 24), new Vector2(0, cardHeight * 0.5f - 16),
            14, Color.white, FontStyles.Bold);

        float rowY = cardHeight * 0.5f - 50;

        // Pattern row
        AddLabel(card.transform, "PATTERN",
            new Vector2(480, 16), new Vector2(0, rowY),
            9, new Color(0.55f, 0.65f, 0.85f), FontStyles.Bold);

        BuildSingleSelectRow(card.transform, edit, patternIds, patternLabel,
            new Vector2(0, rowY - 22), 110f, 28f, isPattern: true);

        rowY -= 64;

        // Duration row
        AddLabel(card.transform, "HOW LONG",
            new Vector2(480, 16), new Vector2(0, rowY),
            9, new Color(0.55f, 0.65f, 0.85f), FontStyles.Bold);

        BuildSingleSelectRow(card.transform, edit, durationIds, durationLabel,
            new Vector2(0, rowY - 22), 86f, 28f, isPattern: false);

        rowY -= 64;

        // Triggers row (multi-select)
        AddLabel(card.transform, "TRIGGERS (tap any that apply)",
            new Vector2(480, 16), new Vector2(0, rowY),
            9, new Color(0.55f, 0.65f, 0.85f), FontStyles.Bold);

        BuildMultiSelectRow(card.transform, edit,
            new Vector2(0, rowY - 22), 74f, 28f);

        return card;
    }

    void BuildSingleSelectRow(Transform parent, RegionEdit edit,
        string[] ids, string[] labels, Vector2 pos,
        float chipW, float chipH, bool isPattern)
    {
        float gap = 6f;
        float totalWidth = ids.Length * chipW + (ids.Length - 1) * gap;
        float startX = -totalWidth * 0.5f + chipW * 0.5f;

        for (int i = 0; i < ids.Length; i++)
        {
            string id = ids[i];
            string label = labels[i];

            GameObject chip = CreateChip(parent, label,
                new Vector2(chipW, chipH),
                new Vector2(startX + i * (chipW + gap), pos.y),
                () => {
                    if (isPattern) { edit.pattern  = id; UpdateChipVisuals(edit.patternChips,  id, ids); }
                    else           { edit.duration = id; UpdateChipVisuals(edit.durationChips, id, ids); }
                });

            if (isPattern) edit.patternChips.Add(chip);
            else           edit.durationChips.Add(chip);
        }

        UpdateChipVisuals(isPattern ? edit.patternChips : edit.durationChips,
            isPattern ? edit.pattern : edit.duration, ids);
    }

    void BuildMultiSelectRow(Transform parent, RegionEdit edit,
        Vector2 pos, float chipW, float chipH)
    {
        float gap = 6f;
        float totalWidth = triggerIds.Length * chipW + (triggerIds.Length - 1) * gap;
        float startX = -totalWidth * 0.5f + chipW * 0.5f;

        for (int i = 0; i < triggerIds.Length; i++)
        {
            string id = triggerIds[i];
            string label = triggerLabel[i];

            GameObject chip = CreateChip(parent, label,
                new Vector2(chipW, chipH),
                new Vector2(startX + i * (chipW + gap), pos.y),
                () => {
                    if (edit.triggers.Contains(id)) edit.triggers.Remove(id);
                    else                            edit.triggers.Add(id);
                    UpdateMultiChipVisuals(edit.triggerChips, edit.triggers, triggerIds);
                });

            edit.triggerChips.Add(chip);
        }

        UpdateMultiChipVisuals(edit.triggerChips, edit.triggers, triggerIds);
    }

    void UpdateChipVisuals(List<GameObject> chips, string selectedId, string[] ids)
    {
        for (int i = 0; i < chips.Count; i++)
        {
            Image img = chips[i].GetComponent<Image>();
            if (img == null) continue;
            img.color = ids[i] == selectedId
                ? new Color(0.38f, 0.50f, 0.78f)
                : new Color(0.20f, 0.22f, 0.28f);
        }
    }

    void UpdateMultiChipVisuals(List<GameObject> chips, List<string> selectedIds, string[] ids)
    {
        for (int i = 0; i < chips.Count; i++)
        {
            Image img = chips[i].GetComponent<Image>();
            if (img == null) continue;
            img.color = selectedIds.Contains(ids[i])
                ? new Color(0.38f, 0.50f, 0.78f)
                : new Color(0.20f, 0.22f, 0.28f);
        }
    }

    // ── Continue handler ──────────────────────────────────────────────────

    void OnContinue()
    {
        foreach (RegionEdit edit in regionEdits)
            painDataStore.SetRegionDetails(edit.bodyPart, edit.pattern, edit.duration, edit.triggers);

        if (panel != null) panel.SetActive(false);
        onRecapComplete?.Invoke();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    static string PrettyName(string snake)
    {
        if (string.IsNullOrEmpty(snake)) return "Unknown";
        string[] parts = snake.Split('_');
        for (int i = 0; i < parts.Length; i++)
            if (parts[i].Length > 0)
                parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
        return string.Join(" ", parts);
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
        TextMeshProUGUI t = CreateRect("Lbl", parent, size, pos)
            .AddComponent<TextMeshProUGUI>();
        t.text      = text;
        t.fontSize  = fontSize;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.color     = color;
    }

    GameObject CreateChip(Transform parent, string label,
        Vector2 size, Vector2 pos, UnityAction action)
    {
        GameObject chip = CreateRect("Chip_" + label, parent, size, pos);

        Image img = chip.AddComponent<Image>();
        img.color = new Color(0.20f, 0.22f, 0.28f);

        Button btn = chip.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(action);

        BoxCollider col = chip.AddComponent<BoxCollider>();
        col.size   = new Vector3(size.x * 0.001f, size.y * 0.001f, 0.012f);
        col.center = new Vector3(0, 0, 0.006f);

        TextMeshProUGUI t = CreateRect("L", chip.transform, size, Vector2.zero)
            .AddComponent<TextMeshProUGUI>();
        t.text      = label;
        t.fontSize  = 11;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color     = Color.white;

        return chip;
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
        t.fontSize  = 15;
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

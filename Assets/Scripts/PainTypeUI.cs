using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class PainTypeUI : MonoBehaviour
{
    public PainDataStore painDataStore;
    public Transform cameraTransform;

    [Header("Panel Settings")]
    public float distanceFromCamera = 0.30f;
    public float heightOffset = -0.1f;

    private GameObject panel;
    private string selectedType = "sharp";

    void Start()
    {
        BuildPanel();
    }

    void Update()
    {
        if (panel == null || cameraTransform == null) return;

        // Panel floats in front of camera
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
        // Root canvas
        panel = new GameObject("PainTypePanel");
        Canvas canvas = panel.AddComponent<Canvas>();

        // Make canvas interactable with XR hands
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        // This is the critical line for hand interaction
        UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster raycaster =
            panel.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
        raycaster.checkFor3DOcclusion = false;

        panel.AddComponent<CanvasScaler>();

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 180);
        rt.localScale = Vector3.one * 0.001f;

        // Background
        GameObject bg = CreateRect("Background", panel.transform,
            new Vector2(300, 180), Vector2.zero);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        AddRoundedAppearance(bgImg);

        // Title
        GameObject title = CreateRect("Title", panel.transform,
            new Vector2(280, 30), new Vector2(0, 65));
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "Select pain type";
        titleText.fontSize = 16;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // Buttons row
        string[] types  = { "sharp",  "ache",   "stiff"  };
        Color[]  colors = {
            new Color(0.89f, 0.29f, 0.29f),
            new Color(0.94f, 0.62f, 0.15f),
            new Color(0.22f, 0.54f, 0.86f)
        };
        string[] labels = { "Sharp", "Ache", "Stiff" };
        float[]  xPos   = { -95f, 0f, 95f };

        for (int i = 0; i < 3; i++)
        {
            string painType = types[i];
            CreateButton(labels[i], panel.transform,
                new Vector2(80, 60), new Vector2(xPos[i], 0),
                colors[i], () => SelectType(painType));
        }

        // Intensity label
        GameObject intLabel = CreateRect("IntLabel", panel.transform,
            new Vector2(280, 20), new Vector2(0, -52));
        TextMeshProUGUI intText = intLabel.AddComponent<TextMeshProUGUI>();
        intText.text = "Intensity: 5";
        intText.fontSize = 12;
        intText.alignment = TextAlignmentOptions.Center;
        intText.color = new Color(0.8f, 0.8f, 0.8f);

        // Intensity buttons
        string[] intLabels = { "-", "5", "+" };
        float[]  intXPos   = { -40f, 0f, 40f };

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            GameObject btn = CreateRect("Int" + i, panel.transform,
                new Vector2(30, 24), new Vector2(intXPos[i], -72));

            Image btnImg = btn.AddComponent<Image>();
            btnImg.color = new Color(0.25f, 0.25f, 0.25f);

            Button button = btn.AddComponent<Button>();
            button.onClick.AddListener(() => {
                if (index == 0) AdjustIntensity(-1, intText);
                if (index == 2) AdjustIntensity(1, intText);
            });

            GameObject btnLabel = CreateRect("L", btn.transform,
                new Vector2(30, 24), Vector2.zero);
            TextMeshProUGUI t = btnLabel.AddComponent<TextMeshProUGUI>();
            t.text = intLabels[index];
            t.fontSize = 14;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
        }

        // Clear button
        CreateButton("Clear all", panel.transform,
            new Vector2(260, 28), new Vector2(0, -105),
            new Color(0.3f, 0.3f, 0.3f), () => ClearAllMarkers());
    }

    void SelectType(string type)
    {
        selectedType = type;
        if (painDataStore != null)
            painDataStore.currentPainType = type;
        Debug.Log("Pain type selected: " + type);
    }

    void AdjustIntensity(int delta, TextMeshProUGUI label)
    {
        if (painDataStore == null) return;
        painDataStore.currentIntensity =
            Mathf.Clamp(painDataStore.currentIntensity + delta, 1, 10);
        label.text = "Intensity: " + painDataStore.currentIntensity;
    }

    void ClearAllMarkers()
    {
        GameObject body = GameObject.FindWithTag("BodyMesh");
        if (body == null) return;

        // Find and destroy all sphere markers parented to the body
        foreach (Transform child in body.transform)
        {
            if (child.name == "Sphere")
            {
                Destroy(child.gameObject);
            }
        }

        // Reset pain zones in data store
        if (painDataStore != null)
        {
            painDataStore.currentSession.painZones.Clear();
            Debug.Log("All pain markers cleared");
        }
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

        // XR poke requires a collider on the button
        BoxCollider col = btn.AddComponent<BoxCollider>();
        col.size = new Vector3(size.x * 0.001f, size.y * 0.001f, 0.01f);
        col.center = new Vector3(0, 0, 0.005f);

        GameObject labelObj = CreateRect("Label", btn.transform, size, Vector2.zero);
        TextMeshProUGUI text = labelObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 14;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        return btn;
    }

    GameObject CreateRect(string name, Transform parent,
        Vector2 size, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return go;
    }

    void AddRoundedAppearance(Image img)
    {
        img.type = Image.Type.Sliced;
    }
}
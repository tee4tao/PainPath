using UnityEngine;

public class PainPainter : MonoBehaviour
{
    public SkinnedMeshRenderer bodyRenderer;
    public int textureSize = 512;
    public float brushSize = 0.05f;
    public Color painColor = new Color(0.85f, 0.2f, 0.1f, 1f);

    private Texture2D paintTexture;
    private Material paintMaterial;

    void Start()
    {
        // Create texture
        paintTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

        // Fill with transparent blue base
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color(0.4f, 0.7f, 1f, 0.3f);
        paintTexture.SetPixels(pixels);
        paintTexture.Apply();

        // Create a new material instance so we don't edit the shared material
        paintMaterial = new Material(bodyRenderer.sharedMaterial);
        paintMaterial.SetTexture("_BaseMap", paintTexture);

        // Apply to the renderer
        bodyRenderer.material = paintMaterial;

        Debug.Log("PainPainter initialised. Material: " + paintMaterial.name);
    }

    public void Paint(Vector2 uv)
    {
        if (paintTexture == null || paintMaterial == null)
        {
            Debug.LogError("PainPainter: texture or material is null");
            return;
        }

        int x = Mathf.Clamp((int)(uv.x * textureSize), 0, textureSize - 1);
        int y = Mathf.Clamp((int)(uv.y * textureSize), 0, textureSize - 1);
        int radius = Mathf.Max(1, (int)(brushSize * textureSize));

        Debug.Log("Painting at UV: " + uv + " pixel: " + x + "," + y);

        for (int px = x - radius; px <= x + radius; px++)
        {
            for (int py = y - radius; py <= y + radius; py++)
            {
                if (px < 0 || px >= textureSize || py < 0 || py >= textureSize) continue;
                float dist = Vector2.Distance(new Vector2(px, py), new Vector2(x, y));
                if (dist <= radius)
                    paintTexture.SetPixel(px, py, painColor);
            }
        }

        paintTexture.Apply();
        paintMaterial.SetTexture("_BaseMap", paintTexture);
        Debug.Log("Paint applied to texture");
    }
}
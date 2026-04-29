using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// Enables Meta Quest 3 passthrough using ARFoundation + Meta OpenXR.
/// Works with the XR Interaction Toolkit / MR Interaction Setup scene (no OVR SDK needed).
///
/// One-time Unity settings you must do first — see comments below.
public class PassthroughEnabler : MonoBehaviour
{
    void Start()
    {
        SetupPassthrough();
    }

    void SetupPassthrough()
    {
        Camera cam = Camera.main;
        if (cam == null) { Debug.LogWarning("PassthroughEnabler: Main Camera not found."); return; }

        // ── 1. Add ARCameraBackground to the Main Camera if not already there ──
        // This component renders the live passthrough feed behind your scene.
        ARCameraBackground bg = cam.GetComponent<ARCameraBackground>();
        if (bg == null)
            bg = cam.gameObject.AddComponent<ARCameraBackground>();

        bg.enabled = true;

        // ── 2. Camera clear flags ─────────────────────────────────────────────
        // Depth only — ARCameraBackground writes the passthrough colour itself,
        // so we must NOT clear the colour buffer.
        cam.clearFlags      = CameraClearFlags.Depth;
        cam.backgroundColor = Color.clear;

        // ── 3. Make sure the AR Session is running ────────────────────────────
        ARSession session = FindObjectOfType<ARSession>();
        if (session != null)
            session.enabled = true;
        else
            Debug.LogWarning("PassthroughEnabler: ARSession not found in scene. " +
                "The AR Session GameObject must be active.");
    }
}

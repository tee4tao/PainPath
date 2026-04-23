using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PassthroughEnabler : MonoBehaviour
{
    void Start()
    {
        // Enable AR camera background
        ARCameraBackground bg = FindObjectOfType<ARCameraBackground>();
        if (bg != null)
        {
            bg.enabled = true;
            Debug.Log("AR Camera Background enabled");
        }
        else
        {
            Debug.LogWarning("ARCameraBackground not found");
        }

        // Set camera clear flags to transparent
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
            Debug.Log("Camera set to transparent");
        }
    }
}
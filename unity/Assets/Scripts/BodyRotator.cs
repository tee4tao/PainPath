using UnityEngine;

public class BodyRotator : MonoBehaviour
{
    // Drag the root of the body model here.
    // If left empty, this script's own transform is rotated instead.
    public Transform targetToRotate;

    private bool showingBack = false;

    public void ToggleView()
    {
        showingBack = !showingBack;
        Transform t = targetToRotate != null ? targetToRotate : transform;
        t.rotation = Quaternion.Euler(0f, showingBack ? 180f : 0f, 0f);
    }

    public bool IsShowingBack => showingBack;
}

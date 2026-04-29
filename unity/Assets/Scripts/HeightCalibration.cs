using UnityEngine;
using System.Collections;

public class HeightCalibration : MonoBehaviour
{
    [Header("References")]
    public Transform bodyTransform;

    [Header("Settings")]
    // Eye height the body model represents at its ORIGINAL prefab scale
    public float modelReferenceEyeHeight = 1.62f;

    // Seconds to wait before measuring — lets Quest 3 tracking stabilise
    public float calibrationDelay = 1.5f;

    // Optional clamps so we never blow the body up or shrink it to nothing
    public float minScaleMultiplier = 0.6f;
    public float maxScaleMultiplier = 2.0f;

    // Applied AFTER the height-based multiplier. Use to nudge the model
    // larger/smaller without touching the prefab. 1.0 = no change, 1.3 = +30%.
    public float extraScaleMultiplier = 1.3f;

    private Vector3 originalScale = Vector3.one;
    private bool    captured      = false;

    void Awake()
    {
        // Capture the prefab/scene scale BEFORE anything modifies it
        if (bodyTransform != null)
        {
            originalScale = bodyTransform.localScale;
            captured      = true;
        }
    }

    void Start()
    {
        StartCoroutine(CalibrateAfterDelay());
    }

    IEnumerator CalibrateAfterDelay()
    {
        yield return new WaitForSeconds(calibrationDelay);
        Calibrate();
    }

    public void Calibrate()
    {
        if (bodyTransform == null || Camera.main == null) return;

        // Late capture — Awake may have run before bodyTransform was wired
        if (!captured)
        {
            originalScale = bodyTransform.localScale;
            captured      = true;
        }

        float userEyeHeight = Camera.main.transform.position.y;

        // Ignore implausible values — tracking may not be ready yet
        if (userEyeHeight < 0.5f || userEyeHeight > 2.5f) return;

        float multiplier = userEyeHeight / modelReferenceEyeHeight;
        multiplier = Mathf.Clamp(multiplier, minScaleMultiplier, maxScaleMultiplier);

        // Apply the extra inspector-tweakable nudge on top
        multiplier *= extraScaleMultiplier;

        // Multiply against the model's original scale instead of overwriting it.
        // This preserves any non-uniform scale set on the prefab.
        bodyTransform.localScale = originalScale * multiplier;
    }
}

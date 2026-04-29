using UnityEngine;

/// Tag a child collider on the body with this component to give it a label.
/// Add Box/Sphere colliders for: head, torso, left_shoulder, right_shoulder,
/// left_arm, right_arm, lower_back, left_leg, right_leg, etc.
///
/// At paint time HandRaycastPainter finds the BodyPartZone whose collider
/// is closest to the touch point and stores its name on the PainZone JSON.
[RequireComponent(typeof(Collider))]
public class BodyPartZone : MonoBehaviour
{
    [Tooltip("Label sent to the backend for any pain zone placed inside this collider, e.g. 'left_shoulder'")]
    public string partName = "unknown";

    public Collider Collider { get; private set; }

    void Awake()
    {
        Collider = GetComponent<Collider>();
        // These colliders are LABEL-ONLY — they must not block the touch raycast.
        // Make them triggers so Physics.OverlapSphere/Raycast on BodyMesh still works.
        Collider.isTrigger = true;
    }
}

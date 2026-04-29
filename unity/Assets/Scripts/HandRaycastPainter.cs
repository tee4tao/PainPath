using UnityEngine;
using System.Collections.Generic;

public class HandRaycastPainter : MonoBehaviour
{
    public PainPainter painter;
    public PainTypeUI  painTypeUI;

    [Header("Hand Tracking")]
    [Tooltip("Right index fingertip Transform (drag from XR rig). Optional if leftIndexTip is assigned.")]
    public Transform   rightIndexTip;
    [Tooltip("Left index fingertip Transform (drag from XR rig). Optional if rightIndexTip is assigned.")]
    public Transform   leftIndexTip;

    [Header("Touch Detection")]
    public float touchDistance        = 0.005f;   // OverlapSphere radius around fingertip
    public float fingerRayStartOffset = 0.03f;
    public float fingerRayDistance    = 0.08f;
    public float markerSurfaceOffset  = 0.004f;   // sphere offset from body surface (lens depth alignment)

    [Header("Marker Visual")]
    public float markerScale          = 0.012f;   // sphere diameter (~12mm — 50% larger than initial 8mm)

    [Header("Timing")]
    public float paintInterval        = 0.6f;     // per-finger cooldown between marker spawns
    public float idleTimeout          = 8f;       // seconds of no touches before "another area?" prompt

    // Per-finger cooldowns so dual-hand use isn't penalised
    private float rightCooldown   = 0f;
    private float leftCooldown    = 0f;
    private float idleTimer       = 0f;
    private bool  idlePromptShown = false;

    private Shader     markerShader;
    private GameObject cachedBodyObject;
    private PainDataStore painDataStore;
    private Stack<GameObject> markerHistory = new Stack<GameObject>();
    private BodyPartZone[]    bodyPartZones;

    void Start()
    {
        markerShader     = Shader.Find("Universal Render Pipeline/Lit");
        cachedBodyObject = GameObject.FindWithTag("BodyMesh");
        painDataStore    = GetComponent<PainDataStore>();

        // Cache once — these are static children of the body
        bodyPartZones = cachedBodyObject != null
            ? cachedBodyObject.GetComponentsInChildren<BodyPartZone>(true)
            : new BodyPartZone[0];
    }

    void Update()
    {
        // No fingers wired = nothing to do
        if (rightIndexTip == null && leftIndexTip == null) return;

        rightCooldown -= Time.deltaTime;
        leftCooldown  -= Time.deltaTime;

        bool painted = false;

        // Right hand — fires only if its own cooldown is up
        if (rightIndexTip != null && rightCooldown <= 0f && TryPaintAt(rightIndexTip))
        {
            rightCooldown = paintInterval;
            painted       = true;
        }

        // Left hand — independent cooldown
        if (leftIndexTip != null && leftCooldown <= 0f && TryPaintAt(leftIndexTip))
        {
            leftCooldown = paintInterval;
            painted      = true;
        }

        if (painted)
        {
            idleTimer       = 0f;
            idlePromptShown = false;
            painTypeUI?.HideAreaPrompt();
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTimeout && !idlePromptShown && markerHistory.Count > 0)
            {
                idlePromptShown = true;
                painTypeUI?.ShowAreaPrompt();
            }
        }
    }

    /// Returns true if a marker was placed at this finger's position.
    bool TryPaintAt(Transform fingerTip)
    {
        Collider[] hits = Physics.OverlapSphere(fingerTip.position, touchDistance);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("BodyMesh")) continue;

            RaycastHit rayHit;
            bool hasSurface = TryGetSurfaceHit(hit, fingerTip, out rayHit);

            Vector3 point  = hasSurface ? rayHit.point        : hit.ClosestPoint(fingerTip.position);
            Vector3 normal = hasSurface ? rayHit.normal       : Vector3.zero;
            Vector2 uv     = hasSurface ? rayHit.textureCoord : Vector2.zero;

            SpawnMarker(point, normal, uv);
            return true;
        }
        return false;
    }

    public void ResetIdleTimer()
    {
        idleTimer       = 0f;
        idlePromptShown = false;
    }

    bool TryGetSurfaceHit(Collider bodyCollider, Transform fingerTip, out RaycastHit rayHit)
    {
        Vector3 rayOrigin    = fingerTip.position - (fingerTip.forward * fingerRayStartOffset);
        Vector3 closestPoint = bodyCollider.ClosestPoint(fingerTip.position);
        Vector3 rayDir       = (closestPoint - rayOrigin).normalized;

        if (Physics.Raycast(new Ray(rayOrigin, rayDir), out rayHit, fingerRayDistance))
            return rayHit.collider.CompareTag("BodyMesh");

        return false;
    }

    /// Returns the label of the closest BodyPartZone collider to a given point,
    /// or "unknown" if no zones are configured / out of range.
    string LookupBodyPart(Vector3 worldPoint)
    {
        if (bodyPartZones == null || bodyPartZones.Length == 0) return "unknown";

        float  bestDist = float.MaxValue;
        string bestName = "unknown";

        for (int i = 0; i < bodyPartZones.Length; i++)
        {
            BodyPartZone z = bodyPartZones[i];
            if (z == null || z.Collider == null) continue;

            Vector3 cp   = z.Collider.ClosestPoint(worldPoint);
            float   dist = (cp - worldPoint).sqrMagnitude;

            if (dist < bestDist)
            {
                bestDist = dist;
                bestName = z.partName;
            }
        }
        return bestName;
    }

    void SpawnMarker(Vector3 surfacePoint, Vector3 surfaceNormal, Vector2 uv)
    {
        if (cachedBodyObject == null) return;

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name                 = "Sphere";
        sphere.transform.position   = surfacePoint + surfaceNormal * markerSurfaceOffset;
        sphere.transform.localScale = Vector3.one * markerScale;
        sphere.transform.SetParent(cachedBodyObject.transform, true);

        int      intensity = painDataStore != null ? painDataStore.currentIntensity : 5;
        Renderer r         = sphere.GetComponent<Renderer>();
        Material m         = new Material(markerShader);
        m.color            = IntensityColour(intensity);
        r.material         = m;

        Destroy(sphere.GetComponent<Collider>());

        markerHistory.Push(sphere);

        if (painDataStore != null)
        {
            string bodyPart = LookupBodyPart(surfacePoint);
            painDataStore.AddPainZone(surfacePoint, bodyPart, uv.x, uv.y);
        }
    }

    public void UndoLastMarker()
    {
        if (markerHistory.Count == 0) return;

        GameObject last = markerHistory.Pop();
        if (last != null) Destroy(last);

        if (painDataStore != null && painDataStore.currentSession.painZones.Count > 0)
            painDataStore.currentSession.painZones.RemoveAt(
                painDataStore.currentSession.painZones.Count - 1);
    }

    public void ClearAllMarkers()
    {
        while (markerHistory.Count > 0)
        {
            GameObject m = markerHistory.Pop();
            if (m != null) Destroy(m);
        }
        if (painDataStore != null)
            painDataStore.currentSession.painZones.Clear();
    }

    public Color IntensityColour(int intensity)
    {
        float t      = Mathf.Clamp01((intensity - 1) / 9f);
        Color blue   = new Color(0.20f, 0.50f, 0.90f);
        Color orange = new Color(0.95f, 0.60f, 0.10f);
        Color red    = new Color(0.45f, 0.10f, 0.05f);

        return t <= 0.5f
            ? Color.Lerp(blue, orange, t * 2f)
            : Color.Lerp(orange, red, (t - 0.5f) * 2f);
    }
}

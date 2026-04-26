using UnityEngine;

public class HandRaycastPainter : MonoBehaviour
{
    public PainPainter painter;
    public Transform rightIndexTip;
    public float touchDistance = 0.003f;
    public GameObject painMarkerPrefab;
    public float fingerRayStartOffset = 0.03f;
    public float fingerRayDistance = 0.08f;
    public float markerSurfaceOffset = 0.001f;

    private float paintCooldown = 0f;
    private float paintInterval = 0.2f;

    void Update()
    {
        if (rightIndexTip == null) return;

        paintCooldown -= Time.deltaTime;
        if (paintCooldown > 0) return;

        Collider[] hits = Physics.OverlapSphere(
            rightIndexTip.position,
            touchDistance
        );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("BodyMesh"))
            {
                RaycastHit rayHit;
                bool hasSurfaceHit = TryGetSurfaceHit(hit, out rayHit);

                if (hasSurfaceHit)
                {
                    SpawnMarker(rayHit.point, rayHit.normal);
                }
                else
                {
                    SpawnMarker(hit.ClosestPoint(rightIndexTip.position), Vector3.zero);
                }

                paintCooldown = paintInterval;

                if (painter != null && hasSurfaceHit)
                {
                    painter.Paint(rayHit.textureCoord);
                }

                break;
            }
        }
    }

    bool TryGetSurfaceHit(Collider bodyCollider, out RaycastHit rayHit)
    {
        Vector3 rayOrigin = rightIndexTip.position - (rightIndexTip.forward * fingerRayStartOffset);
        Vector3 closestPoint = bodyCollider.ClosestPoint(rightIndexTip.position);
        Vector3 rayDirection = (closestPoint - rayOrigin).normalized;

        return bodyCollider.Raycast(new Ray(rayOrigin, rayDirection), out rayHit, fingerRayDistance);
    }

    void SpawnMarker(Vector3 surfacePoint, Vector3 surfaceNormal)
    {
        GameObject bodyObject = GameObject.FindWithTag("BodyMesh");
        if (bodyObject == null)
        {
            return;
        }

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Sphere";
        sphere.transform.position = surfacePoint + (surfaceNormal * markerSurfaceOffset);
        sphere.transform.localScale = Vector3.one * 0.008f;
        sphere.transform.SetParent(bodyObject.transform, true);

        Renderer r = sphere.GetComponent<Renderer>();
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        PainDataStore store = GetComponent<PainDataStore>();
        if (store != null)
        {
            switch (store.currentPainType)
            {
                case "sharp": m.color = new Color(0.89f, 0.29f, 0.29f); break;
                case "ache": m.color = new Color(0.94f, 0.62f, 0.15f); break;
                case "stiff": m.color = new Color(0.22f, 0.54f, 0.86f); break;
                default: m.color = new Color(0.89f, 0.29f, 0.29f); break;
            }
        }

        r.material = m;
        Destroy(sphere.GetComponent<Collider>());
        Debug.Log("Marker snapped to surface: " + surfacePoint);
    }
}
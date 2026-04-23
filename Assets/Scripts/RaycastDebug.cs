using UnityEngine;

public class RaycastDebug : MonoBehaviour
{
    public Transform rayOrigin;
    public float rayLength = 2f;

    void Update()
    {
        if (rayOrigin == null) return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;

        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * rayLength, Color.red);

        if (Physics.Raycast(ray, out hit, rayLength))
        {
            Debug.Log("HIT: " + hit.collider.gameObject.name + " | Tag: " + hit.collider.tag);
        }
        else
        {
            Debug.Log("NO HIT");
        }
    }
}
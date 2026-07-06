using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class WorldPosition
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class PainZone
{
    public string         zoneId;        // e.g. "zone_left_hand" — prefixed unique key
    public string         bodyPart;      // e.g. "left_hand" — raw label from BodyPartZone
    public float          uvX;           // 0–1 horizontal mesh UV
    public float          uvY;           // 0–1 vertical mesh UV
    public WorldPosition  worldPosition; // metres, Unity world space
    public string         painType;      // "ache" | "stiff" | "sharp"
    public int            intensity;     // 1–10
    public string         timestamp;     // ISO 8601 UTC
}

[System.Serializable]
public class SessionSummary
{
    public int    totalZones;
    public string dominantPainType;
    public int    maxIntensity;
    public float  averageIntensity;
    public float  durationSeconds;
}

[System.Serializable]
public class RegionDetails
{
    public string       bodyPart;       // e.g. "lower_back"
    public int          markerCount;    // how many markers fall in this region
    public string       pattern  = "unspecified"; // constant / comes_and_goes / worse_with_movement / worse_at_rest / unspecified
    public string       duration = "unspecified"; // today / few_days / weeks / months / years / unspecified
    public List<string> triggers = new List<string>(); // any of: morning, night, exercise, sitting, standing, stress
}

[System.Serializable]
public class PainSession
{
    public string         sessionId;
    public string         patientId;
    public string         submittedAt;
    public string              deviceType     = "MetaQuest3";
    public SessionSummary      sessionSummary = new SessionSummary();
    public List<RegionDetails> regionDetails  = new List<RegionDetails>();
    public List<PainZone>      painZones      = new List<PainZone>();
}

public class PainDataStore : MonoBehaviour
{
    public PainSession currentSession   = new PainSession();
    public string      currentPainType  = "ache";   // "ache" | "stiff" | "sharp"
    public int         currentIntensity = 5;

    [Header("Patient Info (filled at runtime by PatientLoginUI)")]
    public string patientName       = "Patient";
    public int    priorAppointments = 0;

    [Header("Specialist Info")]
    public string specialistName  = "Specialist";
    public string appointmentTime = "";   // e.g. "9:30 AM"

    private System.DateTime? sessionStartUtc;

    void Start()
    {
        currentSession.sessionId = System.Guid.NewGuid().ToString();
    }

    /// Called by PatientLoginUI once the patient confirms their identity.
    public void SetPatient(string id, string displayName)
    {
        patientName              = displayName;
        currentSession.patientId = id;
    }

    /// Called by HandRaycastPainter on every successful touch.
    /// uvX/uvY come from RaycastHit.textureCoord — require a non-convex
    /// MeshCollider on the body for accurate values; fall back to 0,0 otherwise.
    public void AddPainZone(Vector3 worldPos, string bodyPart, float uvX, float uvY)
    {
        if (sessionStartUtc == null)
            sessionStartUtc = System.DateTime.UtcNow;

        string label  = string.IsNullOrEmpty(bodyPart) ? "unknown" : bodyPart;
        string zoneId = label.StartsWith("zone_") ? label : $"zone_{label}";

        PainZone zone = new PainZone
        {
            zoneId        = zoneId,
            bodyPart      = label,
            uvX           = uvX,
            uvY           = uvY,
            worldPosition = new WorldPosition { x = worldPos.x, y = worldPos.y, z = worldPos.z },
            painType      = currentPainType,
            intensity     = currentIntensity,
            timestamp     = System.DateTime.UtcNow.ToString("o")
        };
        currentSession.painZones.Add(zone);
    }

    /// Returns the unique body parts present in the current session, ordered by
    /// marker count desc. The recap UI uses this to build one card per region.
    public List<string> GetAffectedBodyParts()
    {
        return currentSession.painZones
            .GroupBy(z => string.IsNullOrEmpty(z.bodyPart) ? "unknown" : z.bodyPart)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .ToList();
    }

    public int MarkerCountForRegion(string bodyPart)
    {
        return currentSession.painZones.Count(z =>
            string.Equals(z.bodyPart, bodyPart, System.StringComparison.OrdinalIgnoreCase));
    }

    /// Called by PainDetailsRecapUI on Continue. Replaces any prior details
    /// for this region so the patient can re-edit if they go back.
    public void SetRegionDetails(string bodyPart, string pattern, string duration, List<string> triggers)
    {
        if (string.IsNullOrEmpty(bodyPart)) return;

        RegionDetails existing = currentSession.regionDetails
            .Find(r => r.bodyPart == bodyPart);

        if (existing == null)
        {
            existing = new RegionDetails { bodyPart = bodyPart };
            currentSession.regionDetails.Add(existing);
        }

        existing.markerCount = MarkerCountForRegion(bodyPart);
        existing.pattern     = string.IsNullOrEmpty(pattern)  ? "unspecified" : pattern;
        existing.duration    = string.IsNullOrEmpty(duration) ? "unspecified" : duration;
        existing.triggers    = triggers ?? new List<string>();
    }

    public RegionDetails GetRegionDetails(string bodyPart)
    {
        return currentSession.regionDetails.Find(r => r.bodyPart == bodyPart);
    }

    public string GetSessionJSON()
    {
        currentSession.submittedAt    = System.DateTime.UtcNow.ToString("o");
        currentSession.sessionSummary = BuildSummary(currentSession.painZones, sessionStartUtc);

        // Drop region details for regions that no longer have markers (patient may have undone them)
        currentSession.regionDetails.RemoveAll(r =>
            !currentSession.painZones.Any(z => z.bodyPart == r.bodyPart));

        // Refresh marker counts in case the patient added/removed markers after first filling the recap
        foreach (var r in currentSession.regionDetails)
            r.markerCount = MarkerCountForRegion(r.bodyPart);

        return JsonUtility.ToJson(currentSession, true);
    }

    static SessionSummary BuildSummary(List<PainZone> zones, System.DateTime? startUtc)
    {
        SessionSummary s = new SessionSummary();
        if (zones == null || zones.Count == 0) return s;

        s.totalZones       = zones.Count;
        s.maxIntensity     = zones.Max(z => z.intensity);
        s.averageIntensity = (float)zones.Average(z => z.intensity);

        s.dominantPainType = zones
            .GroupBy(z => string.IsNullOrEmpty(z.painType) ? "unknown" : z.painType)
            .OrderByDescending(g => g.Count())
            .First().Key;

        if (startUtc.HasValue)
            s.durationSeconds = (float)(System.DateTime.UtcNow - startUtc.Value).TotalSeconds;

        return s;
    }
}

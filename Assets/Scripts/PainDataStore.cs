using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PainZone
{
    public float uvX;
    public float uvY;
    public string painType;   // "ache", "sharp", "stiff"
    public int intensity;     // 1-10
    public string timestamp;
}

[System.Serializable]
public class PainSession
{
    public string sessionId;
    public string patientId = "patient_001";
    public List<PainZone> painZones = new List<PainZone>();
}

public class PainDataStore : MonoBehaviour
{
    public PainSession currentSession = new PainSession();
    public string currentPainType = "ache";
    public int currentIntensity = 5;

    void Start()
    {
        currentSession.sessionId = System.Guid.NewGuid().ToString();
    }

    public void AddPainZone(Vector2 uv)
    {
        PainZone zone = new PainZone
        {
            uvX        = uv.x,
            uvY        = uv.y,
            painType   = currentPainType,
            intensity  = currentIntensity,
            timestamp  = System.DateTime.UtcNow.ToString("o")
        };
        currentSession.painZones.Add(zone);
    }

    public string GetSessionJSON()
    {
        return JsonUtility.ToJson(currentSession, true);
    }
}
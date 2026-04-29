using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Text;

/// POSTs the PainSession JSON to the Next.js API endpoint.
/// The Next.js server is responsible for writing it to Firestore — Unity does
/// not talk to Firestore directly.
///
/// Setup:
///   1. Engineer deploys their Next.js app (Vercel recommended)
///   2. Paste the full endpoint URL into "Api Url" — e.g.
///      https://painpath.vercel.app/api/session
///   3. (Optional) paste an API key into "Api Key" if the engineer requires auth
///
/// On any failure (no URL, network down, server 500) a copy of the JSON is
/// always written to Application.persistentDataPath so no session is ever lost.
public class SessionUploader : MonoBehaviour
{
    [Header("Backend Config")]
    [Tooltip("Full Next.js API endpoint — e.g. https://painpath.vercel.app/api/session")]
    public string apiUrl = "https://YOUR-VERCEL-URL/api/session";

    [Tooltip("Optional — sent as 'x-api-key' header if non-empty.")]
    public string apiKey = "";

    [Tooltip("Request timeout in seconds.")]
    public int timeoutSeconds = 15;

    public enum UploadStatus { Idle, Uploading, Success, Failed }
    public UploadStatus Status    { get; private set; } = UploadStatus.Idle;
    public string       LastError { get; private set; } = "";

    public void Upload(PainDataStore store)
    {
        if (store == null) { Debug.LogWarning("[SessionUploader] No PainDataStore."); return; }
        StartCoroutine(UploadCoroutine(store.GetSessionJSON(), store.currentSession.sessionId));
    }

    IEnumerator UploadCoroutine(string json, string sessionId)
    {
        // Always save locally first — guarantees no data loss even if upload hangs/fails
        SaveLocal(json, sessionId);

        if (string.IsNullOrEmpty(apiUrl) || apiUrl.Contains("YOUR-VERCEL-URL"))
        {
            Status    = UploadStatus.Failed;
            LastError = "API URL not configured";
            Debug.LogWarning("[SessionUploader] API URL not set — saved locally only.");
            yield break;
        }

        Status = UploadStatus.Uploading;
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(apiUrl, "POST"))
        {
            req.uploadHandler   = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept",       "application/json");
            if (!string.IsNullOrEmpty(apiKey))
                req.SetRequestHeader("x-api-key", apiKey);

            req.timeout = timeoutSeconds;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Status = UploadStatus.Success;
                Debug.Log($"[SessionUploader] Session {sessionId} uploaded. Server responded: {req.downloadHandler.text}");
            }
            else
            {
                Status    = UploadStatus.Failed;
                LastError = $"{req.responseCode} {req.error}";
                Debug.LogError($"[SessionUploader] Upload failed: {LastError}\nLocal copy preserved.");
            }
        }
    }

    void SaveLocal(string json, string sessionId)
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, $"session_{sessionId}.json");
            File.WriteAllText(path, json, Encoding.UTF8);
            Debug.Log($"[SessionUploader] Local copy saved: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SessionUploader] Local save failed: {e.Message}");
        }
    }
}

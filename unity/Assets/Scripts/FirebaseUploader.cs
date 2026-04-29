using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Text;

/// Uploads PainMap session JSON to Firebase Realtime Database using
/// the REST API — no Firebase SDK required.
///
/// Setup:
///   1. Create a Firebase project at console.firebase.google.com
///   2. Enable Realtime Database (start in test mode while developing)
///   3. Copy your database URL (e.g. https://painpath-abc12-default-rtdb.firebaseio.com)
///   4. Paste it into the "Database Url" field in the Inspector
///
/// Each session is stored at:  /sessions/{sessionId}.json
///
/// To add auth later: paste your database secret into "Auth Token"
/// (Firebase Console → Project Settings → Service Accounts → Database secrets)
public class FirebaseUploader : MonoBehaviour
{
    [Header("Firebase Config")]
    public string databaseUrl = "https://YOUR-PROJECT-default-rtdb.firebaseio.com";
    [Tooltip("Optional — database secret for write access. Leave blank while in test mode.")]
    public string authToken   = "";

    // Status shown in UI by CompletionUI
    public enum UploadStatus { Idle, Uploading, Success, Failed }
    public UploadStatus Status { get; private set; } = UploadStatus.Idle;
    public string       LastError { get; private set; } = "";

    // ── Public entry point ────────────────────────────────────────────────

    public void Upload(PainDataStore store)
    {
        if (store == null) return;
        StartCoroutine(UploadCoroutine(store.GetSessionJSON(), store.currentSession.sessionId));
    }

    // ── REST upload ───────────────────────────────────────────────────────

    IEnumerator UploadCoroutine(string json, string sessionId)
    {
        Status = UploadStatus.Uploading;

        // PUT to /sessions/{sessionId} so each session gets its own key
        string url = $"{databaseUrl.TrimEnd('/')}/sessions/{sessionId}.json";
        if (!string.IsNullOrEmpty(authToken))
            url += $"?auth={authToken}";

        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(url, "PUT"))
        {
            req.uploadHandler   = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Status = UploadStatus.Success;
                Debug.Log($"[Firebase] Session uploaded: {sessionId}");

                // Always keep a local copy alongside the upload
                SaveLocal(json, sessionId);
            }
            else
            {
                Status    = UploadStatus.Failed;
                LastError = req.error;
                Debug.LogError($"[Firebase] Upload failed: {req.error}");

                // Save locally so no data is ever lost
                SaveLocal(json, sessionId);
            }
        }
    }

    // ── Local fallback ────────────────────────────────────────────────────

    void SaveLocal(string json, string sessionId)
    {
        string path = Path.Combine(Application.persistentDataPath, $"session_{sessionId}.json");
        File.WriteAllText(path, json, Encoding.UTF8);
        Debug.Log($"[Firebase] Local copy saved: {path}");
    }
}

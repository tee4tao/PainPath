# PainPath — Unity Project

![PainPath in mixed reality on Meta Quest 3](/ProjectSettings/select_left.jpeg)

Unity 2022 / Meta Quest 3 mixed-reality client. The patient stands in passthrough, sees a life-sized 3D body model, taps the parts that hurt with their fingers, picks a pain type and intensity, and submits. The app captures every marker as structured JSON.

This README covers the Unity project only — building it, the scripts, the inspector wiring, and the JSON it produces.

---

## Requirements

- Unity **2022.3 LTS** (or newer 2022 LTS)
- Android Build Support module (with OpenJDK + SDK + NDK Tools)
- Meta Quest 3 in Developer Mode
- USB-C cable for Build And Run

## Tech stack

| | |
|---|---|
| Engine | Unity 2022.3 LTS |
| Render pipeline | URP |
| XR framework | XR Interaction Toolkit + ARFoundation + Meta OpenXR |
| Hand tracking | XR Hands subsystem |
| Passthrough | Meta Quest Passthrough OpenXR feature + ARCameraBackground |
| UI | TextMeshPro world-space canvases with `TrackedDeviceGraphicRaycaster` |
| Networking | UnityWebRequest (REST POST) |
| Serialisation | `JsonUtility` |

No paid assets. No Firebase SDK.

---

## Open / build / run

### Clone and open

```
git clone <repo-url>
```

1. Open Unity Hub → Add → select the project folder
2. First import takes ~5–10 min
3. Open `Assets/Scenes/SampleScene.unity`

### Build to Quest 3

1. **File → Build Settings** → switch platform to **Android**
2. **Player Settings:**
   - Scripting Backend: **IL2CPP**
   - Target Architectures: **ARM64 only** (uncheck ARMv7)
   - Minimum API Level: **Android 10 (API 29)**
3. **Project Settings → XR Plug-in Management → Android tab:**
   - ✅ OpenXR
   - Under OpenXR → ✅ Meta Quest Support feature group
   - Under OpenXR features → ✅ Meta Quest: Passthrough, ✅ Hand Tracking Subsystem
4. Plug Quest in → accept the on-headset USB-debug prompt
5. **File → Build And Run** (or **Build** for an APK only)

### Configure the upload endpoint (optional)

The app POSTs the session JSON to a configurable URL. Without it, sessions still save locally — nothing is lost.

1. Hierarchy → select **Managers**
2. **SessionUploader** component → paste your endpoint into **Api Url**
3. (Optional) set **Api Key** for an `x-api-key` header

If `Api Url` is the placeholder, the app falls back to writing JSON to `Application.persistentDataPath`.

---

## Project structure

```
Assets/
├── Scenes/
│   └── SampleScene.unity          single-scene project; UI built at runtime
├── Scripts/
│   ├── HandRaycastPainter.cs      touch detection (OverlapSphere + Physics.Raycast),
│   │                              dual-hand, per-finger cooldown, marker spawning
│   ├── PainTypeUI.cs              world-space UI: pain type buttons, intensity bar,
│   │                              undo / rotate / submit, idle prompt
│   ├── PainDataStore.cs           session data model + summary aggregation + JSON export
│   ├── PatientLoginUI.cs          numpad login (6 digits, demo accepts anything)
│   ├── OnboardingUI.cs            welcome panel with usage instructions
│   ├── CompletionUI.cs            review → Edit/Confirm → exit flow
│   ├── SessionUploader.cs         POST session JSON + local JSON fallback
│   ├── BodyPartZone.cs            tag a child collider with a named region label
│   ├── BodyRotator.cs             rotate body 180° on Y to show front/back
│   ├── HeightCalibration.cs       scale body to user's eye height after tracking settles
│   ├── PassthroughEnabler.cs      optional helper; usually unnecessary if the MR
│   │                              Interaction Setup prefab is in the scene
│   ├── PainPainter.cs             legacy texture overlay; NOT called at runtime
│   └── RaycastDebug.cs            debug helper — draws rays, logs hits
└── MRTemplateAssets/              Unity Meta MR template scaffolding (used selectively)
```

Scripts live on two GameObjects in the scene:

- **PainSystem** → `HandRaycastPainter`, `PainDataStore`, `PainTypeUI`, `RaycastDebug`
- **Managers** → `OnboardingUI`, `PatientLoginUI`, `CompletionUI`, `HeightCalibration`, `SessionUploader`, `PassthroughEnabler`

---

## Scene setup checklist

### XR rig

The scene uses **MR Interaction Setup** (a prefab from Unity's Meta MR template) which contains:

- XR Origin (XR Rig)
- Camera Offset → Main Camera (with Camera + ARCameraManager)
- Hand visualisers + interactor rigs

If you start a fresh scene, drop in `Assets/MRTemplateAssets/Prefabs/MRInteractionSetup.prefab` and an **AR Session** GameObject (`GameObject → XR → AR Session`).

### Body model

The body GameObject (`PainPathBody → BodyMesh` in this project) must:

- Be **tagged** `BodyMesh`
- Have a **non-convex MeshCollider** matching the visible mesh
  - `Convex` unchecked
  - `Is Trigger` unchecked
  - `Mesh` field assigned to the body's mesh asset

Touch detection and `RaycastHit.textureCoord` (UV) both rely on the non-convex MeshCollider. Primitive colliders (Capsule/Box/Sphere) return `(0, 0)` for UV and miss limbs at oblique angles.

### Passthrough

Out of the box, the Meta MR template scene contains an `Environment` GameObject — a fake-room skybox sphere. Its `FadeMaterial` script holds an opaque skybox by default, hiding passthrough.

For a clean MR experience: **delete or disable the `Environment` GameObject**. Also delete `Lighting` if your body model is self-lit.

### URP renderer

The active URP renderer asset must include the **AR Background Renderer Feature** for passthrough to composite correctly. Find your URP renderer (Project Settings → Graphics → Scriptable Render Pipeline Settings → click through to its Renderer asset) and add the feature if it isn't already there.

### Body material for passthrough

The body's material **Surface Type** must be set to `Transparent` with alpha around `0.20` so passthrough shows behind it. Otherwise the body becomes a solid wall blocking the real world.

---

## Inspector wiring

### On `PainSystem`

| Component | Field | Drag in |
|---|---|---|
| `HandRaycastPainter` | Right Index Tip | XR rig → right hand index tip Transform |
| `HandRaycastPainter` | Left Index Tip | XR rig → left hand index tip Transform |
| `HandRaycastPainter` | Pain Type UI | PainSystem (self) |
| `PainTypeUI` | Pain Data Store | PainSystem |
| `PainTypeUI` | Hand Raycast Painter | PainSystem |
| `PainTypeUI` | Body Rotator | wherever `BodyRotator` is attached |
| `PainTypeUI` | Camera Transform | Main Camera |

### On `Managers`

| Component | Field | Drag in |
|---|---|---|
| `HeightCalibration` | Body Transform | PainPathBody (root of the body model) |
| `PatientLoginUI` | Pain Data Store | PainSystem |
| `PatientLoginUI` | Camera Transform | Main Camera |
| `OnboardingUI` | Pain Data Store / Pain Type UI | PainSystem |
| `OnboardingUI` | Height Calibration / Patient Login UI | Managers (self) |
| `OnboardingUI` | Camera Transform | Main Camera |
| `CompletionUI` | Pain Type UI / Pain Data Store / Hand Raycast Painter | PainSystem |
| `CompletionUI` | Session Uploader | Managers |
| `CompletionUI` | Camera Transform | Main Camera |
| `SessionUploader` | Api Url | your backend endpoint (or leave placeholder) |

---

## Body part zones

Each region of the body is a child GameObject of the BodyMesh-tagged GameObject with:

- A **Box** or **Sphere** collider (auto-set to `IsTrigger = true` by `BodyPartZone.Awake`)
- A `BodyPartZone` component with `Part Name` set to a lowercase snake_case label

Recommended 22 zones:

```
head, neck,
left_shoulder, right_shoulder,
chest, abdomen, upper_back, lower_back,
left_arm, right_arm,
left_elbow, right_elbow,
left_forearm, right_forearm,
left_hand, right_hand,
left_leg, right_leg,
left_knee, right_knee,
left_foot, right_foot
```

At paint time, `HandRaycastPainter` finds the **closest** zone collider to the touch point and writes its label to the marker's JSON.

> ⚠️ **Avoid front-back overlap** in the torso. Make `chest` and `abdomen` cover only the front half (Z ≥ 0); `upper_back` / `lower_back` cover only the back half (Z ≤ 0). Otherwise a back-of-body touch can fall inside a front-of-body collider and mislabel.

---

## JSON output

Submitted sessions are POSTed as `application/json`. Example:

```json
{
  "sessionId": "8c2f4d1e-9b3a-4e7f-a1c2-7d8e9f0a1b2c",
  "patientId": "123456",
  "submittedAt": "2026-04-25T14:32:18.4521000Z",
  "deviceType": "MetaQuest3",
  "sessionSummary": {
    "totalZones": 3,
    "dominantPainType": "ache",
    "maxIntensity": 10,
    "averageIntensity": 7.33,
    "durationSeconds": 47.8
  },
  "painZones": [
    {
      "zoneId": "zone_right_hand",
      "bodyPart": "right_hand",
      "uvX": 0.7821,
      "uvY": 0.4137,
      "worldPosition": { "x": 0.918, "y": 1.421, "z": 0.012 },
      "painType": "sharp",
      "intensity": 10,
      "timestamp": "2026-04-25T14:31:48.0114000Z"
    }
  ]
}
```

| Field | Notes |
|---|---|
| `sessionId` | UUID generated client-side at scene start |
| `patientId` | 6-digit number from numpad login |
| `deviceType` | Constant `"MetaQuest3"` |
| `painType` | One of `"ache"`, `"stiff"`, `"sharp"` |
| `intensity` | Integer 1–10 |
| `bodyPart` | Region label, or `"unknown"` if no zone matched |
| `uvX` / `uvY` | 0–1 mesh UV (requires non-convex MeshCollider on the body) |
| `worldPosition` | Unity world-space metres |
| `sessionSummary` | Aggregated client-side on Submit so consumers don't have to roll up themselves |

If the upload fails (no internet, bad URL, server 500) the JSON is still written to `Application.persistentDataPath/session_<sessionId>.json` on the device. Pull these manually via:

```
adb pull /storage/emulated/0/Android/data/<package-name>/files/
```

---

## Tunable runtime values

All exposed on the `HandRaycastPainter` Inspector for live tweaking on-device:

| Field | Default | Tweak if... |
|---|---|---|
| Touch Distance | `0.0` | Spheres spawn in mid-air (lower) or you have to press hard (raise to ~0.002) |
| Marker Surface Offset | `0.004` | Spheres clip into body (raise) or float (lower) |
| Marker Scale | `0.012` | Spheres look too big or too small |
| Paint Interval | `0.6` | Touches cluster (raise) or feel sluggish (lower) |
| Idle Timeout | `8.0` | "Another area?" prompt fires too soon / too late |

`HeightCalibration` also exposes:

| Field | Default | Notes |
|---|---|---|
| Model Reference Eye Height | `1.62` | Eye height the body model represents at its prefab scale |
| Calibration Delay | `1.5` | Seconds to wait at scene start before measuring (lets tracking settle) |
| Min / Max Scale Multiplier | `0.6` / `2.0` | Safety clamps |
| Extra Scale Multiplier | `1.3` | Post-calibration nudge — `1.0` = no change, `1.3` = +30% |

---

## Common gotchas

| Problem | Cause | Fix |
|---|---|---|
| Body turns red / becomes pure white texture | `PainPainter.Paint()` accidentally wired into a touch event | It must NOT be called. Sphere markers are the visual indicator. |
| Spheres don't appear on certain limbs | Body has a Capsule/Box collider | Swap to non-convex MeshCollider |
| `bodyPart` always reports `"unknown"` | BodyPartZones aren't children of the BodyMesh-tagged object | Re-parent them under the tagged GameObject |
| `uvX` / `uvY` are 0 | Body has primitive collider OR mesh has no UVs | Use non-convex MeshCollider; ensure the FBX export includes UVs |
| Sphere appears in mid-air before touch | `Touch Distance` too lenient + Quest fingertip prediction overshoot | Set `Touch Distance` to `0.0` |
| Body too big / too small after calibration | `HeightCalibration` clamps or `extraScaleMultiplier` off | Tweak the multiplier on `HeightCalibration` |
| No passthrough on device | `Environment` skybox sphere from MR template still active | Delete or disable `Environment` GameObject |
| Rotate button only spins markers, not the mesh | `BodyRotator.targetToRotate` not assigned | Drag the body mesh root into `Target To Rotate` |

---

## Known limitations

- **Patient login is dummy** — any 6 digits accepted, no Firebase lookup. To wire real lookup, replace the `OnSubmit()` body in `PatientLoginUI.cs` with a `UnityWebRequest GET` to `/api/patient/{id}` returning the patient name.
- **Single language (English)**.
- **No audio cues** — silent app.
- **`PainPainter.cs` is dead code** — kept in the project for reference but never called. Do not wire `Paint()` to touch events; it overwrites the body's `_BaseMap` texture.

## License

MIT.

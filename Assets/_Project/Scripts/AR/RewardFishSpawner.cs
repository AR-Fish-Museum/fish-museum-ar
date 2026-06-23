using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Reward Mode'da seçili balığı (veya debug küpü) gösterir.
/// URP'de AR kamera görüntüsünün üstüne çizmek için ayrı bir Overlay kamera
/// oluşturup AR kamerasının camera stack'ine ekler ve balığı "RewardFish"
/// layer'ında render eder.
/// </summary>
public class RewardFishSpawner : MonoBehaviour
{
    [Header("Fallback Prefab")]
    [SerializeField] private GameObject defaultFishPrefab;

    [Header("Reward Mode'da devre dışı bırakılacaklar")]
    [SerializeField] private FishPlacementController fishPlacementController;
    [SerializeField] private Behaviour[] extraPlacementToDisable;

    [Header("Spawn Ayarları")]
    [SerializeField] private float spawnDistance = 1.8f;
    [SerializeField] private float verticalOffset = -0.2f;
    [SerializeField] private float modelScale = 0.15f;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float spawnDelay = 0.5f;

    [Header("Reward Balık Ölçekleri (balık tipine göre)")]
    [SerializeField] private float clownfishScale = 0.35f;
    [SerializeField] private float sharkScale = 0.07f;
    [SerializeField] private float defaultRewardScale = 0.1f;

    [Header("Reward Mode — Kamera Child Spawn")]
    [SerializeField] private bool parentToCameraInRewardMode = true;
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, -0.15f, 1.2f);
    [SerializeField] private Vector3 cameraLocalEulerAngles = new Vector3(0f, 180f, 0f);

    [Header("Reward Mode — Overlay Camera (URP)")]
    [SerializeField] private bool useOverlayCameraInRewardMode = true;
    [SerializeField] private string rewardLayerName = "RewardFish";
    [SerializeField] private int overlayCameraDepth = 10;

    [Header("Debug")]
    [SerializeField] private bool spawnDebugCubeInsteadOfFish = false;

    private GameObject _spawnedFish;
    private Camera _overlayCamera;

    private void Start()
    {
        Debug.Log("[RewardFishSpawner] Start çalıştı.");

        if (spawnOnStart)
            StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        bool rewardMode = SelectedFishSession.SelectedFishPrefab != null;
        Debug.Log($"[RewardFishSpawner] Reward Mode: {rewardMode}");

        if (rewardMode)
            DisableTouchPlacement();

        yield return new WaitForSeconds(spawnDelay);

        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("[RewardFishSpawner] Camera.main bulunamadı. " +
                           "AR kamerasının 'MainCamera' tag'ine sahip olduğundan emin olun.");
            yield break;
        }
        Debug.Log("[RewardFishSpawner] Camera.main bulundu.");
        Debug.Log($"[RewardFishSpawner] Camera.main: {camera.name}");
        LogAllCameras();

        if (rewardMode)
            DisableNonMainCameras(camera);

        // ── Gösterilecek objeyi oluştur (debug cube ya da balık) ──
        GameObject obj;
        float objScale;

        if (spawnDebugCubeInsteadOfFish)
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var rend = obj.GetComponent<Renderer>();
            if (rend != null) rend.material.color = Color.red;
            objScale = 0.2f;
            Debug.Log("[RewardFishSpawner] Debug cube spawn edildi.");
        }
        else
        {
            GameObject selectedPrefab = rewardMode
                ? SelectedFishSession.SelectedFishPrefab
                : defaultFishPrefab;

            Debug.Log($"[RewardFishSpawner] Selected prefab: " +
                      $"{(selectedPrefab != null ? selectedPrefab.name : "YOK")}");

            if (selectedPrefab == null)
            {
                Debug.LogError("[RewardFishSpawner] Spawn edilecek prefab yok.");
                yield break;
            }

            obj = Instantiate(selectedPrefab);
            objScale = ResolveRewardScale();
        }

        // ── Yerleştirme: overlay camera > camera child > world ──
        if (rewardMode && useOverlayCameraInRewardMode)
        {
            PlaceWithOverlayCamera(obj, camera, objScale);
        }
        else if (rewardMode && parentToCameraInRewardMode)
        {
            obj.transform.SetParent(camera.transform, false);
            obj.transform.localPosition = cameraLocalPosition;
            obj.transform.localRotation = Quaternion.Euler(cameraLocalEulerAngles);
            obj.transform.localScale = Vector3.one * objScale;
            Debug.Log("[RewardFishSpawner] Camera child mode aktif.");
        }
        else
        {
            Vector3 spawnPos = camera.transform.position + camera.transform.forward * spawnDistance;
            spawnPos.y += verticalOffset;
            obj.transform.position = spawnPos;

            Vector3 lookDir = obj.transform.position - camera.transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
                obj.transform.rotation = Quaternion.LookRotation(lookDir);

            obj.transform.localScale = Vector3.one * objScale;
            Debug.Log($"[RewardFishSpawner] Balık spawn edildi (world): pozisyon {spawnPos}.");
        }

        _spawnedFish = obj;
    }

    // ── Overlay camera ile yerleştirme (URP camera stacking) ──────
    private void PlaceWithOverlayCamera(GameObject obj, Camera mainCam, float objScale)
    {
        int rewardLayer = LayerMask.NameToLayer(rewardLayerName);
        if (rewardLayer == -1)
        {
            Debug.LogError("[RewardFishSpawner] RewardFish layer bulunamadı. " +
                           "Lütfen Unity Tags & Layers içinde RewardFish layer oluştur.");
            Destroy(obj);
            return;
        }

        Debug.Log("[RewardFishSpawner] Overlay camera mode aktif.");
        Debug.Log($"[RewardFishSpawner] Reward layer index: {rewardLayer}");

        Camera overlay = EnsureOverlayCamera(mainCam, rewardLayer);

        obj.transform.SetParent(overlay.transform, false);
        obj.transform.localPosition = cameraLocalPosition;
        obj.transform.localRotation = Quaternion.Euler(cameraLocalEulerAngles);
        obj.transform.localScale = Vector3.one * objScale;

        SetLayerRecursively(obj, rewardLayer);
        Debug.Log("[RewardFishSpawner] Debug cube RewardFish layer'a alındı.");
    }

    private Camera EnsureOverlayCamera(Camera mainCam, int rewardLayer)
    {
        if (_overlayCamera != null) return _overlayCamera;

        var go = new GameObject("RewardOverlayCamera");
        var cam = go.AddComponent<Camera>();

        cam.fieldOfView = mainCam.fieldOfView;
        cam.nearClipPlane = mainCam.nearClipPlane;
        cam.farClipPlane = mainCam.farClipPlane;
        cam.clearFlags = CameraClearFlags.Depth;
        cam.depth = overlayCameraDepth;
        cam.cullingMask = 1 << rewardLayer;

        go.transform.SetParent(mainCam.transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        // AR kamerasının culling mask'inden RewardFish layer'ını çıkar
        mainCam.cullingMask &= ~(1 << rewardLayer);
        Debug.Log("[RewardFishSpawner] Main camera culling mask güncellendi.");

        // URP camera stacking: overlay'i base kameranın stack'ine ekle
        var baseData = mainCam.GetUniversalAdditionalCameraData();
        var overlayData = cam.GetUniversalAdditionalCameraData();
        overlayData.renderType = CameraRenderType.Overlay;
        if (baseData != null && !baseData.cameraStack.Contains(cam))
            baseData.cameraStack.Add(cam);

        _overlayCamera = cam;
        Debug.Log("[RewardFishSpawner] Overlay camera oluşturuldu.");
        return cam;
    }

    private float ResolveRewardScale()
    {
        string id = SelectedFishSession.SelectedFishId;

        float scale;
        if (id == "reward_clownfish")
            scale = clownfishScale;
        else if (id == "reward_shark")
            scale = sharkScale;
        else
            scale = defaultRewardScale;

        Debug.Log($"[RewardFishSpawner] Applied reward scale: {scale} (id: {id ?? "-"})");
        return scale;
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void DisableTouchPlacement()
    {
        if (fishPlacementController != null)
        {
            fishPlacementController.enabled = false;
            Debug.Log("[RewardFishSpawner] FishPlacementController devre dışı bırakıldı.");
        }
        else
        {
            Debug.LogWarning("[RewardFishSpawner] fishPlacementController atanmamış.");
        }

        if (extraPlacementToDisable != null)
        {
            foreach (var b in extraPlacementToDisable)
            {
                if (b != null)
                {
                    b.enabled = false;
                    Debug.Log($"[RewardFishSpawner] Devre dışı bırakıldı: {b.GetType().Name}");
                }
            }
        }
    }

    private void LogAllCameras()
    {
        Camera[] cams = Camera.allCameras;
        Debug.Log($"[RewardFishSpawner] Aktif kamera sayısı: {cams.Length}");
        foreach (var cam in cams)
        {
            if (cam == null) continue;
            Debug.Log($"[RewardFishSpawner] Kamera: {cam.name} | enabled: {cam.enabled} | " +
                      $"depth: {cam.depth} | clearFlags: {cam.clearFlags} | " +
                      $"cullingMask: {cam.cullingMask} | tag: {cam.tag}");
        }
    }

    private void DisableNonMainCameras(Camera mainCam)
    {
        foreach (var cam in Camera.allCameras)
        {
            if (cam == null || cam == mainCam) continue;
            if (_overlayCamera != null && cam == _overlayCamera) continue;

            cam.enabled = false;
            Debug.Log($"[RewardFishSpawner] Kamera devre dışı bırakıldı: {cam.name} (tag: {cam.tag})");
        }
    }
}
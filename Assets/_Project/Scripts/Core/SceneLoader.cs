using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // ── Sahne adları — tek noktadan yönetilir ─────────────────────
    public const string LoginScene       = "LoginScene";
    public const string BootstrapScene   = "00_Bootstrap";
    public const string RealARScene      = "01_AR_Museum";
    public const string WebcamTestScene  = "02_Webcam_Test";
    public const string FishGalleryScene = "03_Fish_Gallery";

    private static string _previousSceneName;

    // ── Sahne geçiş geçmişi ───────────────────────────────────────
    private static void LoadSceneWithHistory(string sceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (!string.IsNullOrEmpty(currentScene) && currentScene != sceneName)
        {
            _previousSceneName = currentScene;
            Debug.Log($"[SceneLoader] Previous scene kaydedildi: {_previousSceneName}");
        }

        Debug.Log($"[SceneLoader] Sahne yükleniyor: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    public static void LoadPreviousSceneOrFallback(string fallbackSceneName = LoginScene)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        string targetScene = !string.IsNullOrEmpty(_previousSceneName)
            ? _previousSceneName
            : fallbackSceneName;

        if (targetScene == currentScene)
            targetScene = fallbackSceneName;

        Debug.Log($"[SceneLoader] Geri dönülüyor: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }

    // ── Mevcut metotlar ────────────────────────────────────────────
    public void LoadRealARScene()
    {
        LoadSceneWithHistory(RealARScene);
    }

    public void LoadWebcamTestScene()
    {
        LoadSceneWithHistory(WebcamTestScene);
    }

    public void LoadLoginScene()
    {
        LoadSceneWithHistory(LoginScene);
    }

    public void LoadBootstrapScene()
    {
        LoadSceneWithHistory(BootstrapScene);
    }

    public void LoadFishGalleryScene()
    {
        LoadSceneWithHistory(FishGalleryScene);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
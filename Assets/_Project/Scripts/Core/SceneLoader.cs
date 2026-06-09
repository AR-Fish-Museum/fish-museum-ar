using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // ── Sahne adları — tek noktadan yönetilir ─────────────────────
    // Bu değerler Build Settings'teki sahne dosya adlarıyla birebir eşleşmelidir.
    public const string LoginScene      = "LoginScene";
    public const string BootstrapScene  = "00_Bootstrap";
    public const string RealARScene     = "01_AR_Museum";
    public const string WebcamTestScene = "02_Webcam_Test";
    public const string FishGalleryScene = "03_Fish_Gallery";

    // ── Mevcut metotlar (isim ve imza korunur) ────────────────────
    public void LoadRealARScene()
    {
        SceneManager.LoadScene(RealARScene);
    }

    public void LoadWebcamTestScene()
    {
        SceneManager.LoadScene(WebcamTestScene);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    // ── Yeni eklenen metotlar ─────────────────────────────────────
    public void LoadLoginScene()
    {
        SceneManager.LoadScene(LoginScene);
    }

    public void LoadBootstrapScene()
    {
        SceneManager.LoadScene(BootstrapScene);
    }

    public void LoadFishGalleryScene()
    {
        SceneManager.LoadScene(FishGalleryScene);
    }
}
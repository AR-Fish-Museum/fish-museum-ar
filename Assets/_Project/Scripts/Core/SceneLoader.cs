using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadRealARScene()
    {
        SceneManager.LoadScene("01_AR_Museum");
    }

    public void LoadWebcamTestScene()
    {
        SceneManager.LoadScene("02_Webcam_Test");
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
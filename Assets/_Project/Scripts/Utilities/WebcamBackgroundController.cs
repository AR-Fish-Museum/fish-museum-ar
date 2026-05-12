using UnityEngine;
using UnityEngine.UI;

public class WebcamBackgroundController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RawImage targetImage;

    [Header("Webcam Settings")]
    [SerializeField] private int requestedWidth = 640;
    [SerializeField] private int requestedHeight = 480;
    [SerializeField] private int requestedFPS = 15;

    private WebCamTexture webcamTexture;

    private void Start()
    {
        StartWebcam();
    }

    private void StartWebcam()
    {
        if (targetImage == null)
        {
            Debug.LogError("Target Image atanmadı.");
            return;
        }

        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogWarning("Kamera bulunamadı.");
            return;
        }

        WebCamDevice selectedDevice = WebCamTexture.devices[0];

        webcamTexture = new WebCamTexture(
            selectedDevice.name,
            requestedWidth,
            requestedHeight,
            requestedFPS
        );

        targetImage.texture = webcamTexture;
        webcamTexture.Play();

        Debug.Log($"Webcam started: {selectedDevice.name} | {requestedWidth}x{requestedHeight} @{requestedFPS}fps");
    }

    private void OnDestroy()
    {
        StopWebcam();
    }

    private void OnApplicationQuit()
    {
        StopWebcam();
    }

    private void StopWebcam()
    {
        if (webcamTexture != null)
        {
            if (webcamTexture.isPlaying)
                webcamTexture.Stop();

            webcamTexture = null;
        }
    }
}
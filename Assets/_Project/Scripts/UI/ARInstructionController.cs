using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARInstructionController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text instructionText;

    [Header("AR")]
    [SerializeField] private ARPlaneManager planeManager;

    private bool hasPlacedFish;

    private void Start()
    {
        SetMessage("Yüzeyi tarayın...");
    }

    private void Update()
    {
        if (hasPlacedFish)
            return;

        if (HasDetectedPlane())
        {
            SetMessage("Yüzey bulundu, ekrana dokunun.");
        }
        else
        {
            SetMessage("Yüzeyi tarayın...");
        }
    }

    public void OnFishPlaced()
    {
        hasPlacedFish = true;
        SetMessage("Balık yerleştirildi.");
    }

    private bool HasDetectedPlane()
    {
        if (planeManager == null)
            return false;

        foreach (var plane in planeManager.trackables)
        {
            if (plane.gameObject.activeInHierarchy)
                return true;
        }

        return false;
    }

    private void SetMessage(string message)
    {
        if (instructionText != null)
        {
            instructionText.text = message;
        }
    }
}
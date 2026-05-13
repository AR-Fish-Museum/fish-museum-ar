using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FishPlacementController : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private GameObject fishPrefab;
    [SerializeField] private ARInstructionController instructionController;

    private static readonly List<ARRaycastHit> hits = new();
    private GameObject spawnedFish;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        Vector2? inputPosition = GetInputPosition();

        if (inputPosition == null)
            return;

        if (raycastManager.Raycast(inputPosition.Value, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (spawnedFish == null)
            {
                spawnedFish = Instantiate(fishPrefab, hitPose.position, hitPose.rotation);
            }
            else
            {
                spawnedFish.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }

            instructionController?.OnFishPlaced();
        }
    }

    private Vector2? GetInputPosition()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return Mouse.current.position.ReadValue();
        }

        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            var touch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                return touch.screenPosition;
            }
        }

        return null;
    }
}
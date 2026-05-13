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

    [Header("Placement Settings")]
    [SerializeField] private float verticalOffset = 0.03f;
    [SerializeField] private float modelScale = 1f;

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
            Vector3 spawnPosition = hitPose.position + Vector3.up * verticalOffset;

            PlaceFish(spawnPosition);
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

    private void PlaceFish(Vector3 position)
    {
        if (spawnedFish == null)
        {
            spawnedFish = Instantiate(fishPrefab, position, Quaternion.identity);
            spawnedFish.transform.localScale = Vector3.one * modelScale;
        }
        else
        {
            spawnedFish.transform.position = position;
        }

        instructionController?.OnFishPlaced();

        Debug.Log("Fish placed: " + spawnedFish.name + " at " + position);
    }
}
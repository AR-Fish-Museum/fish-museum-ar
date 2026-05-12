using UnityEngine;
using UnityEngine.InputSystem;

public class WebcamPlacementController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject fishPrefab;

    [Header("Placement Settings")]
    [SerializeField] private float placementDistance = 3f;
    [SerializeField] private float fishScale = 0.3f;

    private GameObject spawnedFish;

    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        Vector3 placementPosition = ray.GetPoint(placementDistance);

        PlaceFish(placementPosition);
    }

    private void PlaceFish(Vector3 position)
    {
        if (spawnedFish == null)
        {
            spawnedFish = Instantiate(fishPrefab, position, Quaternion.identity);
            spawnedFish.transform.localScale = Vector3.one * fishScale;
        }
        else
        {
            spawnedFish.transform.position = position;
        }

        spawnedFish.transform.LookAt(mainCamera.transform);
        spawnedFish.transform.Rotate(0f, 180f, 0f);
    }
}
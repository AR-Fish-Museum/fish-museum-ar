using UnityEngine;

public class RewardFishSwimmer : MonoBehaviour
{
    private Transform _volumeRoot;
    private Vector3 _volumeSize;
    private float _minSpeed;
    private float _maxSpeed;
    private float _turnSpeed;
    private Vector3 _modelRotationOffsetEuler;

    private Vector3 _targetLocalPosition;
    private float _currentSpeed;
    private bool _configured;

    private const float TargetReachDistance = 0.08f;

    public void Configure(
        Transform volumeRoot,
        Vector3 volumeSize,
        float minSpeed,
        float maxSpeed,
        float turnSpeed,
        Vector3 modelRotationOffsetEuler)
    {
        _volumeRoot = volumeRoot;
        _volumeSize = volumeSize;
        _minSpeed = Mathf.Max(0.01f, minSpeed);
        _maxSpeed = Mathf.Max(_minSpeed, maxSpeed);
        _turnSpeed = Mathf.Max(0.01f, turnSpeed);
        _modelRotationOffsetEuler = modelRotationOffsetEuler;

        PickNewTarget();

        _configured = true;
    }

    private void Update()
    {
        if (!_configured || _volumeRoot == null)
            return;

        Vector3 currentLocal = transform.localPosition;
        Vector3 toTargetLocal = _targetLocalPosition - currentLocal;

        if (toTargetLocal.magnitude <= TargetReachDistance)
        {
            PickNewTarget();
            toTargetLocal = _targetLocalPosition - currentLocal;
        }

        transform.localPosition = Vector3.MoveTowards(
            currentLocal,
            _targetLocalPosition,
            _currentSpeed * Time.deltaTime
        );

        if (toTargetLocal.sqrMagnitude > 0.0001f)
        {
            Vector3 worldDirection = _volumeRoot.TransformDirection(toTargetLocal.normalized);

            Quaternion targetRotation =
                Quaternion.LookRotation(worldDirection, Vector3.up) *
                Quaternion.Euler(_modelRotationOffsetEuler);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                _turnSpeed * Time.deltaTime
            );
        }
    }

    private void PickNewTarget()
    {
        float halfX = _volumeSize.x * 0.5f;
        float halfY = _volumeSize.y * 0.5f;
        float halfZ = _volumeSize.z * 0.5f;

        _targetLocalPosition = new Vector3(
            Random.Range(-halfX, halfX),
            Random.Range(-halfY, halfY),
            Random.Range(-halfZ, halfZ)
        );

        _currentSpeed = Random.Range(_minSpeed, _maxSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        if (_volumeRoot == null)
            return;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = _volumeRoot.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, _volumeSize);
        Gizmos.matrix = oldMatrix;
    }
}
using UnityEngine;

namespace GameDevBuddies
{
    public class TerrainScanOriginPositioner : MonoBehaviour
    {
        [Header("References: ")]
        [SerializeField] private Transform _mainCameraTransform = null;
        [SerializeField] private Transform _playerTransform = null;

        private void LateUpdate()
        {
            transform.position = _playerTransform.position;

            Vector3 cameraLookDirection = _mainCameraTransform.forward;
            cameraLookDirection.y = 0f;
            cameraLookDirection = cameraLookDirection.normalized;

            transform.rotation = Quaternion.LookRotation(cameraLookDirection, Vector3.up);
        }
    }
}
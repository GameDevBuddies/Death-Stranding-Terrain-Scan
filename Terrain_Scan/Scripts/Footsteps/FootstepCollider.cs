using UnityEditor.PackageManager;
using UnityEngine;

namespace GameDevBuddies
{
    /// <summary>
    /// Enumeration specifying the type of the footstep.
    /// </summary>
    public enum FootstepType: byte
    {
        Left = 0,
        Right = 1
    }

    /// <summary>
    /// Class that detects collision with the ground and propagates that to the footstep placement controller.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FootstepCollider : MonoBehaviour
    {
        [SerializeField] private FootstepsPlacementController _footstepPlacementController = null;
        [SerializeField] private FootstepType _footstepType = FootstepType.Left;

        [Header("Options: ")]
        [SerializeField] private LayerMask _groundLayerMask = 6;
        [SerializeField] private float _raycastHeightOffset = 0.1f;
        [SerializeField] private float _footstepPositionOffsetToAvoidClipping = 0.01f;

        private bool _currentlyCollidingWithGround = false;
        private Collider _groundCollider = null;

        private void OnDrawGizmos()
        {
            if (_footstepPlacementController == null)
            {
                return;
            }

            if (_currentlyCollidingWithGround)
            {
                Gizmos.color = _footstepType == FootstepType.Right ? Color.red : Color.green;
                Gizmos.DrawLine(transform.position, transform.position + _footstepPlacementController.ForwardDirection);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_currentlyCollidingWithGround)
            {
                return;
            }

            if ((_groundLayerMask & (1 << other.gameObject.layer)) != 0)
            {
                _currentlyCollidingWithGround = true;
                _groundCollider = other;

                if (_footstepPlacementController == null)
                {
                    Debug.LogError("Couldn't report footstep since footstep placement controller reference hasn't been assigned.");
                    return;
                }

                // Cast a ray downwards to get the exact point of impact with the underlying surface.
                Ray physicsRay = new Ray(transform.position + Vector3.up * _raycastHeightOffset, Vector3.down);
                if (Physics.Raycast(physicsRay, out RaycastHit hitInfo, _raycastHeightOffset * 2f, _groundLayerMask))
                {
                    Vector3 forwardDirection = _footstepPlacementController.ForwardDirection;
                    _footstepPlacementController.RecordFootstep(new FootstepInfo
                    {
                        Rotation = Quaternion.LookRotation(hitInfo.normal, forwardDirection),
                        Position = hitInfo.point + hitInfo.normal.normalized * _footstepPositionOffsetToAvoidClipping,
                        HighlightPercentage = 0f,
                        IsRightFoot = (int)_footstepType
                    });
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_currentlyCollidingWithGround && _groundCollider == other)
            {
                _currentlyCollidingWithGround = false;
                _groundCollider = null;
            }
        }
    }
}
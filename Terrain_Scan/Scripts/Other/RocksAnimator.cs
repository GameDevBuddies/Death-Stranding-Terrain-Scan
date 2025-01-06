using UnityEngine;

namespace GameDevBuddies
{
    public class RocksAnimator : MonoBehaviour
    {
        [Header("References: ")]
        [SerializeField] private Transform _rotatorTransform = null;
        [SerializeField] private Transform[] _smallRocksTransforms = null;

        [Header("Options: ")]
        [SerializeField] private float _rotatorRotationSpeed = 90f;
        [SerializeField] private float _rocksMovementAmplitude = 0.1f;
        [SerializeField] private float _rocksMovementFrequency = 0.5f;
        [SerializeField] private float _rockMovementOffset = 0.5f;

        private Vector3 _rotatorLocalEulers = Vector3.zero;

        private void Update()
        {
            // Rotator animation.
            _rotatorLocalEulers.y += Time.deltaTime * _rotatorRotationSpeed;
            _rotatorTransform.localEulerAngles = _rotatorLocalEulers;

            // Animating movement of individual small rocks.
            float offset = 0f;
            foreach (Transform rockTransform in _smallRocksTransforms)
            {
                Vector3 localPosition = rockTransform.localPosition;
                localPosition.y = Mathf.Sin(Time.time * _rocksMovementFrequency + offset) * _rocksMovementAmplitude;
                rockTransform.localPosition = localPosition;

                offset += _rockMovementOffset;
            }
        }
    }
}
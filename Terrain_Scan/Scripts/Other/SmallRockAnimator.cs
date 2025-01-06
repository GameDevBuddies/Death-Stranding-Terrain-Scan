using UnityEngine;

namespace GameDevBuddies
{
    public class SmallRockAnimator : MonoBehaviour
    {
        [Header("Options: ")]
        [SerializeField] private Vector3 _rotationVector = new Vector3(30f, -45f, 15f);

        private Vector3 _rotatorLocalEulers = Vector3.zero;

        private void Update()
        {
            // Rotator animation.
            _rotatorLocalEulers += Time.deltaTime * _rotationVector;
            transform.localEulerAngles = _rotatorLocalEulers;
        }
    }
}
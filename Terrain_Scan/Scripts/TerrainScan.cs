using GameDevBuddies.CustomAttributes;
using System;
using UnityEngine;

namespace GameDevBuddies
{
    /// <summary>
    /// Structure containing basic information of a specific terrain scan.
    /// </summary>
    [Serializable]
    public struct TerrainScanInfo
    {
        /// <summary>
        /// World position of the origin of the effect.
        /// </summary>
        public Vector3 Origin;
        /// <summary>
        /// Spread direction of the scan effect, on the X-Z plane.
        /// (The Y-component of the vector is 0).
        /// </summary>
        public Vector3 Direction;
        /// <summary>
        /// Maximum angle of the terrain scan effect. Everything outside this
        /// angle threshold from the spread direction shouldn't be affected by the effect.
        /// </summary>
        public float MaxAngle;
        /// <summary>
        /// Range in meters from the origin of the terrain scan inside which special objects
        /// should activate to provide additional information to the player.
        /// </summary>
        public float ActivationRange;
        /// <summary>
        /// Duration of the terrain scan effect. Used for showing additional information about the terrain.
        /// </summary>
        public float ActiveDuration;
    }

    /// <summary>
    /// Class that controls the activity of the terrain scan. It also contains the most important 
    /// properties of the effect, like its origin, spread direction, activity range, and angle threshold.
    /// </summary>
    public class TerrainScan : Singleton<TerrainScan>
    {
        [Header("References: ")]
        [SerializeField] private Transform _terrainScanOriginTransform = null;

        [Header("Settings: ")]
        [SerializeField] private float _maxAngle = 145f;
        [SerializeField] private float _activationRange = 45f;
        [SerializeField] private float _activeDuration = 15f;

        private TerrainScanInfo _lastTerrainScanInfo;
        private bool _terrainScanActive = false;
        private float _terrainScanActivationTime = 0f;

        /// <summary>
        /// Action specifying the start of a terrain scan effect, containing properties
        /// describing it completely (origin, spread direction, angle, range, ...).
        /// </summary>
        public Action<TerrainScanInfo> OnTerrainScanStart = null;
        /// <summary>
        /// Action signaling that the currently active terrain scan effect has been completed.
        /// </summary>
        public Action OnTerrainScanEnd = null;

        private Vector3 TerrainScanDirection
        {
            get
            {
                // Direction vector is parallel with the X-Z plane.
                Vector3 terrainScanOriginDirection = _terrainScanOriginTransform.forward;
                terrainScanOriginDirection.y = 0;
                return terrainScanOriginDirection.normalized;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartTerrainScan();
                return;
            }

            if (!_terrainScanActive)
            {
                return;
            }

            if(Time.time - _terrainScanActivationTime > _activeDuration)
            {
                _terrainScanActive = false;
                OnTerrainScanEnd?.Invoke();
            }
        }


        [Button(nameof(StartTerrainScan))]
        public void StartTerrainScan()
        {
            _lastTerrainScanInfo = new TerrainScanInfo
            {
                Origin = _terrainScanOriginTransform.position,
                Direction = TerrainScanDirection,
                MaxAngle = _maxAngle,
                ActivationRange = _activationRange,
                ActiveDuration = _activeDuration
            };

            _terrainScanActive = true;
            _terrainScanActivationTime = Time.time;

            OnTerrainScanStart?.Invoke(_lastTerrainScanInfo);
        }

        /// <summary>
        /// Function checks if the provided position is inside of the terrain 
        /// scan activation angle range.
        /// </summary>
        /// <param name="position">World position to check for.</param>
        /// <returns>True if the angle of the terrain spread direction and the vector 
        /// from the origin of the scan effect to the provided position is less than the 
        /// maximum allowed angle of the terrain scan.</returns>
        public bool IsInsideTerrainScanAngle(Vector3 position)
        {
            // Get the vector from the terrain scan origin towards the position.
            Vector3 directionFromScanOrigin = (position - _lastTerrainScanInfo.Origin);
            directionFromScanOrigin.y = 0f;
            Vector3 normalizedDirectionFromScanOrigin = directionFromScanOrigin.normalized;

            // Check the correlation between the terrain scan spreading direction and the vector 
            // going from the terrain scan origin towards the position.
            float directionCorrelation = Vector3.Dot(_lastTerrainScanInfo.Direction, normalizedDirectionFromScanOrigin);
            float halfMaxAngle = Mathf.Deg2Rad * 0.5f * _maxAngle;

            // If the correlation is larger than the correlation of the maximum angle of the terrain 
            // scan effect, the position is angle-wise inside the terrain scan effect.
            return directionCorrelation >= Mathf.Cos(halfMaxAngle);
        }

        /// <summary>
        /// Function checks if the provided <paramref name="position"/> is close enough 
        /// to the terrain scan origin to be under activation range for special objects.
        /// </summary>
        /// <param name="position">World position to check for.</param>
        /// <returns>True if the position is closer than <see cref="_activationRange"/>, false otherwise.</returns>
        public bool IsInsideTerrainScanActivationRange(Vector3 position)
        {
            float squareLengthFromOrigin = (_lastTerrainScanInfo.Origin - position).sqrMagnitude;
            return squareLengthFromOrigin <= (_activationRange * _activationRange);
        }
    }
}
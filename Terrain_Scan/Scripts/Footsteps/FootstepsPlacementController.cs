using GameDevBuddies.CustomAttributes;
using System;
using UnityEngine;

namespace GameDevBuddies
{
    /// <summary>
    /// Structure containing information about one footstep mark on the ground.
    /// </summary>
    [Serializable]
    public struct FootstepInfo
    {
        public const int Size = sizeof(float) * 8 + sizeof(int);

        // Position and rotation of the footstep, in the world space.
        public Quaternion Rotation;
        public Vector3 Position;

        // [0, 1] range with 0 = Normal and 1 = Highlighted
        public float HighlightPercentage;

        // 0 = Left Foot, 1 = Right Foot
        public int IsRightFoot;
    }

    /// <summary>
    /// Class that controls placing and removal of footstep marks on the ground.
    /// </summary>
    public class FootstepsPlacementController : MonoBehaviour
    {
        [Serializable]
        private enum FootstepHighlightState: byte
        {
            Normal = 0,
            Highlighted = 1,
            Expiring = 2
        }

        [Header("References: ")]
        [SerializeField] private FootstepsRenderer _footstepRenderer = null;

        [Header("Settings: ")]
        [SerializeField] private int _maxFootstepsCount = 1000;
        [SerializeField] private float _footstepsHighlightExpireDuration = 3f;

        [Header("Debug Visualization: ")]
        [SerializeField] private FootstepHighlightState _currentFootstepsState = FootstepHighlightState.Normal;

        private FootstepInfo[] _recordedFootsteps = null;
        private int _freeFootstepSlotIndex = 0;
        private bool _recordedAtLeastOneFootstep = false;
        private bool _recordedFootstepsCompletelyFilled = false;

        // Terrain scan helper variables.
        private TerrainScan _terrainScan = null;
        private float _lastTerrainScanActivationTime = float.MinValue;
        private TerrainScanInfo _terrainScanInfo;

        public Vector3 ForwardDirection
        {
            get
            {
                Vector3 forward = transform.forward;
                forward.y = 0f;
                return forward.normalized;
            }
        }
        private FootstepInfo[] RecordedFootsteps
        {
            get
            {
                if (_recordedFootsteps == null)
                {
                    _recordedFootsteps = new FootstepInfo[_maxFootstepsCount];
                }
                return _recordedFootsteps;
            }
        }
        private bool IsTerrainScanActive
        {
            get
            {
                return Time.time < _lastTerrainScanActivationTime + _terrainScanInfo.ActiveDuration;
            }
        }
        
        private void OnDrawGizmos()
        {
            for(int i = 0; i < _freeFootstepSlotIndex; i++)
            {
                FootstepInfo footstepInfo = RecordedFootsteps[i];
                Gizmos.color = footstepInfo.IsRightFoot == 1 ? Color.red : Color.green;
                Gizmos.DrawSphere(footstepInfo.Position, 0.02f);
            }       
        }

        private void Start()
        {
            _terrainScan = TerrainScan.Instance;
            _terrainScan.OnTerrainScanStart += ActivateFootstepsHighlight;
        }

        private void OnDestroy()
        {
            if(_terrainScan == null)
            {
                return;
            }

            _terrainScan.OnTerrainScanStart -= ActivateFootstepsHighlight;
        }

        private void Update()
        {
            // Check if we should transition from the current highlight state into another one.
            UpdateCurrentHighlightState();

            // Updating current highlight percentage of all active footsteps.
            UpdateFootstepsHighlightPercentage();

            // Don't issue render command if we haven't recorded any footsteps yet.
            if (!_recordedAtLeastOneFootstep)
            {
                return;
            }

            // Render recorded footsteps.
            _footstepRenderer.RenderFootsteps(footsteps: RecordedFootsteps, visibleFootstepsCount: _freeFootstepSlotIndex);
        }

        /// <summary>
        /// Function adds the provided <paramref name="footstepInfo"/> to the collection of recorded footsteps that
        /// will be sent 
        /// </summary>
        /// <param name="footstepInfo"></param>
        public void RecordFootstep(FootstepInfo footstepInfo)
        {
            // Helper variable that prevents issuing render commands before recording at least one footstep.
            _recordedAtLeastOneFootstep = true;

            // All new footsteps should be normal except if the terrain scan effect is currently active and the footstep is 
            // located inside its active area and distance range.
            if (_currentFootstepsState == FootstepHighlightState.Highlighted)
            {
                footstepInfo.HighlightPercentage = ShouldFootstepBeHighlighted(footstepInfo.Position) ? 1.0f : 0.0f;
            }

            // Cache the footstep information so that it would get rendered.
            RecordedFootsteps[_freeFootstepSlotIndex] = footstepInfo;

            // Update the index to the next free slot so that the next footstep can be placed.
            _freeFootstepSlotIndex++;
            if(_freeFootstepSlotIndex >= _maxFootstepsCount)
            {
                // Track that we made an overflow and that we're going over from the first index again.
                _recordedFootstepsCompletelyFilled = true;
                _freeFootstepSlotIndex = 0;
            }
        }

        [Button(nameof(SimulateTerrainScanActivation))]
        private void SimulateTerrainScanActivation()
        {
            Vector3 forwardXZ = transform.forward;
            forwardXZ.y = 0;
            forwardXZ = forwardXZ.normalized;

            TerrainScanInfo terrainScanInfo = new TerrainScanInfo
            {
                Origin = transform.position,
                Direction = forwardXZ,
                MaxAngle = 145f,
                ActivationRange = 22.5f
            };

            ActivateFootstepsHighlight(terrainScanInfo);
        }

        private void UpdateCurrentHighlightState()
        {
            switch (_currentFootstepsState)
            {
                case FootstepHighlightState.Highlighted:
                    if (!IsTerrainScanActive)
                    {
                        _currentFootstepsState = FootstepHighlightState.Expiring;
                    }
                    break;
                case FootstepHighlightState.Expiring:
                    float currentExpirationTime = Time.time - (_lastTerrainScanActivationTime + _terrainScanInfo.ActiveDuration);
                    if (currentExpirationTime >= _footstepsHighlightExpireDuration)
                    {
                        UpdateFootstepsHighlightPercentage();
                        _currentFootstepsState = FootstepHighlightState.Normal;
                    }
                    break;
                case FootstepHighlightState.Normal:
                default:
                    break;
            }
        }

        private void UpdateFootstepsHighlightPercentage()
        {
            float highlightPercentage = GetHighlightPercentage();
            int lastFootstepIndex = _recordedFootstepsCompletelyFilled ? _maxFootstepsCount : _freeFootstepSlotIndex;
            for(int i = 0; i < lastFootstepIndex; i++)
            {
                // Reducing the footstep highlight percentage. This doesn't affect new footsteps that haven't been created during the active period of the terrain scan.
                FootstepInfo footstepInfo = RecordedFootsteps[i];                
                footstepInfo.HighlightPercentage = ShouldFootstepBeHighlighted(footstepInfo.Position) ? Mathf.Min(highlightPercentage, footstepInfo.HighlightPercentage) : 0.0f;
                RecordedFootsteps[i] = footstepInfo;
            }
        }

        private void ActivateFootstepsHighlight(TerrainScanInfo terrainScanInfo)
        {
            // Caching all required properties for determining if the footstep is inside the scan effect or not.
            _currentFootstepsState = FootstepHighlightState.Highlighted;
            _lastTerrainScanActivationTime = Time.time;
            _terrainScanInfo = terrainScanInfo;

            for (int i = 0; i < _freeFootstepSlotIndex; i++)
            {
                FootstepInfo footstepInfo = RecordedFootsteps[i];
                footstepInfo.HighlightPercentage = ShouldFootstepBeHighlighted(footstepInfo.Position) ? 1.0f : 0.0f;
                RecordedFootsteps[i] = footstepInfo;                
            }
        }

        private float GetHighlightPercentage()
        {
            switch (_currentFootstepsState)
            {
                case FootstepHighlightState.Highlighted:
                    // During the active phase of the effect, the highlight should have maximum visibility.
                    return 1f;
                case FootstepHighlightState.Expiring:
                    // During the expiration phase, interpolate from the maximum visibility of the highlight
                    // towards the minimum one.
                    float currentExpirationTime = Time.time - (_lastTerrainScanActivationTime + _terrainScanInfo.ActiveDuration);
                    float expirationPercentage = Mathf.Clamp01(currentExpirationTime / _footstepsHighlightExpireDuration);
                    return Mathf.Lerp(1f, 0f, expirationPercentage);
                case FootstepHighlightState.Normal:
                default:
                    // During normal game play, while the terrain scan effect isn't visible, no highlight should be visible.
                    return 0f;
            }
        }

        private bool ShouldFootstepBeHighlighted(Vector3 position)
        {
            if(_terrainScan == null)
            {
                return false;
            }

            // If the footstep is outside the circle arc of the terrain scan effect, we can early exit and
            // conclude that the footstep shouldn't be highlighted.
            bool footstepInsideCircleArc = _terrainScan.IsInsideTerrainScanAngle(position);
            if (!footstepInsideCircleArc)
            {
                return false;
            }

            // Finally, if the footstep is close enough to the terrain scan origin, mark it as highlighted.
            return _terrainScan.IsInsideTerrainScanActivationRange(position);
        }
    }
}
using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using UnityEngine;

namespace GameDevBuddies
{
    [ExecuteAlways]
    public class TerrainScanController : MonoBehaviour
    {
        [Serializable]
        public enum TerrainScanState: byte
        {
            Inactive = 0,
            Spawning = 1,
            Prewarm = 2,
            Expanding = 3
        }

        [Header("References: ")]
        [SerializeField] private Material _terrainScanMaterial = null;

        [Header("Global Animation Options: ")]
        [SerializeField] private float _spawnAnimationDuration = 0.2f;
        [SerializeField] private float _prewarmAnimationDuration = 0.3f;
        [SerializeField] private float _expansionAnimationDuration = 2.5f;

        [Header("Pre Warm Animation Options: ")]
        [SerializeField] private float _spawnInitialVelocity = 15f;
        [SerializeField] private float _preWarmVelocity = 2f;

        [Header("Spawn Options: ")]
        [SerializeField] private Ease _spawnVelocityAnimationType = Ease.InCubic;
        [SerializeField] private float _spawnInitialRange = 0.5f;        
        [SerializeField] private Ease _spawnOpacityAnimationType = Ease.InQuart;
        [SerializeField] private float _spawnFinalOpacity = 0.55f;

        [Header("Expansion Options: ")]
        [SerializeField] private Ease _expansionAccelerationAnimationType = Ease.InOutQuad;        
        [SerializeField] private float _expansionMaxVelocity = 300f;
        [SerializeField] private float _expansionDurationForMaxVelocity = 0.3f;
        [SerializeField] private Ease _expansionDeccelerationAnimationType = Ease.OutCubic;
        [SerializeField] private float _expansionFinalVelocity = 5f;
        [SerializeField] private Ease _fadeOutAnimationType = Ease.OutQuad;
        [SerializeField] private float _fadeOutDuration = 1.7f;

        [Header("Dark Circle Options: ")]
        [SerializeField] private Ease _darkCircleFadeOutAnimationType = Ease.OutCubic;
        [SerializeField] private float _darkCircleFadeOutDuration = 0.2f;

        // Animation properties.
        private TerrainScanState _currentAnimationState = TerrainScanState.Inactive;
        private float _currentAnimationStateTime = 0f;

        // Expansion properties.
        private float _currentVelocity = 0f;
        private float _currentRange = 0f;

        // Opacity properties.
        private float _currentOpacity = 0f;

        public float TotalScanDuration { get { return _spawnAnimationDuration + _prewarmAnimationDuration + _expansionAnimationDuration; } }

        private void Start()
        {
            _terrainScanMaterial.SetFloat("_Terrain_Scan_Range", _currentRange);
            _terrainScanMaterial.SetFloat("_Global_Visibility", _currentOpacity);
        }

        private void Update()
        {
            if (!AreAllReferencesAssigned())
            {
                return;
            }

            UpdateAnimation();
            UpdateOrigin();
        }        

        public void StartAnimation()
        {
            _currentAnimationStateTime = 0f;
            _currentAnimationState = TerrainScanState.Spawning;

            _currentRange = _spawnInitialRange;
            _currentVelocity = _spawnInitialVelocity;

            UpdateSpreadSpeed();
            UpdateOpacity();
        }

        private void CompleteAnimation()
        {
            _currentAnimationStateTime = 0f;
            _currentOpacity = 0f;
            _currentAnimationState = TerrainScanState.Inactive;

            UpdateSpreadSpeed();
            UpdateOpacity();
        }

        private bool AreAllReferencesAssigned()
        {
            return _terrainScanMaterial != null;
        }

        private void UpdateAnimation()
        {
            if (_currentAnimationState == TerrainScanState.Inactive)
            {
                return;
            }

            _currentAnimationStateTime += Time.deltaTime;

            // Checking if the animation should transition into another state.
            switch (_currentAnimationState)
            {
                case TerrainScanState.Spawning:
                    if(_currentAnimationStateTime >= _spawnAnimationDuration)
                    {
                        _currentAnimationStateTime -= _spawnAnimationDuration;
                        _currentAnimationState = TerrainScanState.Prewarm;
                    }
                    break;
                case TerrainScanState.Prewarm:
                    if (_currentAnimationStateTime >= _prewarmAnimationDuration)
                    {
                        _currentAnimationStateTime -= _prewarmAnimationDuration;
                        _currentAnimationState = TerrainScanState.Expanding;
                    }
                    break;
                case TerrainScanState.Expanding:
                    if (_currentAnimationStateTime >= _expansionAnimationDuration)
                    {
                        CompleteAnimation();
                        return;
                    }
                    break;
                case TerrainScanState.Inactive:
                default:
                    return;
            }

            UpdateSpreadSpeed();
            UpdateOpacity();
        }

        private void UpdateSpreadSpeed()
        {
            switch (_currentAnimationState)
            {
                case TerrainScanState.Spawning:
                    float animationPercentage = EaseManager.Evaluate(_spawnVelocityAnimationType, null, _currentAnimationStateTime, _spawnAnimationDuration, 0f, 0f);
                    _currentVelocity = Mathf.Lerp(_spawnInitialVelocity, _preWarmVelocity, Mathf.Clamp01(animationPercentage));
                    break;
                case TerrainScanState.Prewarm:
                    // Keep velocity as is during the pre warm state.
                    break;
                case TerrainScanState.Expanding:
                    // First, accelerate towards the max velocity.
                    if (_currentAnimationStateTime <= _expansionDurationForMaxVelocity)
                    {
                        float expandingAccelerationPercentage = EaseManager.Evaluate(_expansionAccelerationAnimationType, null, _currentAnimationStateTime, _expansionDurationForMaxVelocity, 0f, 0f);
                        _currentVelocity = Mathf.Lerp(_preWarmVelocity, _expansionMaxVelocity, Mathf.Clamp01(expandingAccelerationPercentage));
                    }
                    else
                    {
                        // Then, slowly slow down towards the min velocity.
                        float expandingDeccelerationPercentage = EaseManager.Evaluate(_expansionDeccelerationAnimationType, null, _currentAnimationStateTime - _expansionDurationForMaxVelocity, _expansionAnimationDuration - _expansionDurationForMaxVelocity, 0f, 0f);
                        _currentVelocity = Mathf.Lerp(_expansionMaxVelocity, _expansionFinalVelocity, Mathf.Clamp01(expandingDeccelerationPercentage));
                    }
                    break;
                case TerrainScanState.Inactive:
                default:
                    _currentVelocity = 0f;
                    break;
            }

            _currentVelocity = Mathf.Min(_expansionMaxVelocity, _currentVelocity);
            _currentVelocity = Mathf.Max(0f, _currentVelocity);

            _currentRange += Time.deltaTime * _currentVelocity;
            _terrainScanMaterial.SetFloat("_Terrain_Scan_Range", _currentRange);
        }

        private void UpdateOpacity()
        {
            float groundCircleOpacity = 0f;
            switch (_currentAnimationState)
            {
                case TerrainScanState.Spawning:
                    float animationPercentage = EaseManager.Evaluate(_spawnOpacityAnimationType, null, _currentAnimationStateTime, _spawnAnimationDuration, 0f, 0f);
                    _currentOpacity = Mathf.Lerp(0f, _spawnFinalOpacity, Mathf.Clamp01(animationPercentage));
                    groundCircleOpacity = 1f;
                    break;
                case TerrainScanState.Prewarm:
                    _currentOpacity = _spawnFinalOpacity;
                    groundCircleOpacity = 1f;
                    break;
                case TerrainScanState.Expanding:
                    // Updating opacity.
                    float hideStartTime = _expansionAnimationDuration - _fadeOutDuration;
                    if (_currentAnimationStateTime >= hideStartTime)
                    {
                        float easedHidePercentage = EaseManager.Evaluate(_fadeOutAnimationType, null, _currentAnimationStateTime - hideStartTime, _fadeOutDuration, 0f, 0f);
                        _currentOpacity = Mathf.Lerp(_spawnFinalOpacity, 0f, Mathf.Clamp01(easedHidePercentage));
                    }

                    // Updating ground circle opacity.
                    if (_currentAnimationStateTime <= _darkCircleFadeOutDuration)
                    {
                        float darkCircleVisibilityPercentage = EaseManager.Evaluate(_darkCircleFadeOutAnimationType, null, _currentAnimationStateTime, _darkCircleFadeOutDuration, 0f, 0f);
                        groundCircleOpacity = 1.0f - Mathf.Clamp01(darkCircleVisibilityPercentage);
                    }

                    break;
                case TerrainScanState.Inactive:
                default:
                    _currentOpacity = 0f;
                    break;
            }

            _terrainScanMaterial.SetFloat("_Global_Visibility", _currentOpacity);
            _terrainScanMaterial.SetFloat("_Dark_Circle_Visibility", groundCircleOpacity);
        }

        private void UpdateOrigin()
        {
            _terrainScanMaterial.SetVector("_Terrain_Scan_Origin", transform.position);

            Vector3 terrainScanOriginDirection = transform.forward;
            terrainScanOriginDirection.y = 0;
            _terrainScanMaterial.SetVector("_Terrain_Scan_Direction_XZ", terrainScanOriginDirection.normalized);
        }
    }
}
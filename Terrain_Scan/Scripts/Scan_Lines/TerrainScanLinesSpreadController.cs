using GameDevBuddies.CustomAttributes;
using UnityEngine;

namespace GameDevBuddies
{
    /// <summary>
    /// Class responsible for controlling the spread of the scan lines material effect.
    /// It basically calculates the current visible distance and spreading speed of the scan lines
    /// and updates it on the material.
    /// </summary>
    public class TerrainScanLinesSpreadController : MonoBehaviour
    {
        [Header("References: ")]
        [SerializeField] private TerrainScanSpreadRecorder _terrainScanSpreadRecorder = null;
        [SerializeField] private Material _terrainScanMaterial = null;

        [Header("Settings: ")]
        [SerializeField, Range(0.05f, 1f)] private float _timeSpeedMultiplier = 1.0f;

        [Header("Recorded Spread Data: ")]
        [SerializeField] private TerrainSpreadRecordedFrame[] _recordedSpreadingFrames = null;

        // Cached terrain scan properties.
        private TerrainScan _terrainScan = null;
        private TerrainScanInfo _terrainScanInfo;
        private bool _terrainScanActive = false;

        // Animation properties.
        private float _currentAnimationStateTime = 0f;
        private int _currentRecordedFrameIndex = 0;
        private TerrainSpreadRecordedFrame _currentSpreadFrame;

        public float TerrainScanMaxAngle { get => _terrainScanMaterial.GetFloat("_Side_Edge_Soften_Arc_Angle"); }
        public float TotalScanDuration { get { return _recordedSpreadingFrames[^1].Time; } }
        public float CurrentSpreadRange { get => _currentSpreadFrame.SpreadRange; }

        private float CurrentGlobalVisibility { get => _currentSpreadFrame.GlobalEffectOpacity; }
        private float CurrentDarkCircleVisibility { get => _currentSpreadFrame.DarkCircleOpacity; }

        private void Start()
        {
            SetInitialValues();
            UpdateMaterialProperties();

            _terrainScan = TerrainScan.Instance;
            _terrainScan.OnTerrainScanStart += StartScanLinesSpread;
        }

        private void OnDestroy()
        {
            if (_terrainScan != null)
            {
                _terrainScan.OnTerrainScanStart -= StartScanLinesSpread;
            }
        }

        private void Update()
        {
            if (!AreAllReferencesAssigned() || !_terrainScanActive)
            {
                return;
            }

            UpdateAnimation();
            UpdateMaterialProperties();
        }

        [Button(nameof(RecordTerrainSpread))]
        private void RecordTerrainSpread()
        {
            _recordedSpreadingFrames = _terrainScanSpreadRecorder.RecordTerrainSpread();
        }

        [Button(nameof(SimulateScanLinesSpread))]
        private void SimulateScanLinesSpread()
        {
            TerrainScanInfo terrainScanInfo = new TerrainScanInfo
            {
                Origin = transform.position,
                Direction = transform.forward,
                MaxAngle = 145f,
                ActivationRange = 22.5f,
                ActiveDuration = 20f
            };

            StartScanLinesSpread(terrainScanInfo);
        }

        private void StartScanLinesSpread(TerrainScanInfo terrainScanInfo)
        {
            // Caching terrain scan details.
            _terrainScanInfo = terrainScanInfo;

            // Resetting animation control properties to initial values.
            SetInitialValues();
            UpdateMaterialProperties();

            // Setting the material values that don't change over time and should only be set once.
            _terrainScanMaterial.SetFloat("_Side_Edge_Soften_Arc_Angle", _terrainScanInfo.MaxAngle);
            _terrainScanMaterial.SetVector("_Terrain_Scan_Origin", _terrainScanInfo.Origin);
            _terrainScanMaterial.SetVector("_Terrain_Scan_Direction_XZ", _terrainScanInfo.Direction);

            // Activating the update logic for spreading the terrain scan effect.
            _terrainScanActive = true;
        }

        private void SetInitialValues()
        {
            _currentAnimationStateTime = 0f;
            _currentRecordedFrameIndex = 0;
            _currentSpreadFrame = _recordedSpreadingFrames[0];
        }

        private void CompleteAnimation()
        {
            _currentSpreadFrame = _recordedSpreadingFrames[^1];
            _currentAnimationStateTime = _currentSpreadFrame.Time;
            _terrainScanActive = false;
        }

        private void UpdateMaterialProperties()
        {
            _terrainScanMaterial.SetFloat("_Terrain_Scan_Range", CurrentSpreadRange);
            _terrainScanMaterial.SetFloat("_Global_Visibility", CurrentGlobalVisibility);
            _terrainScanMaterial.SetFloat("_Dark_Circle_Visibility", CurrentDarkCircleVisibility);
        }

        private bool AreAllReferencesAssigned()
        {
            return _terrainScanMaterial != null;
        }

        private void UpdateAnimation()
        {
            _currentAnimationStateTime += Time.deltaTime * _timeSpeedMultiplier;
            _currentRecordedFrameIndex = GetRecordedFrameIndex(_currentAnimationStateTime, _currentRecordedFrameIndex);
            _currentSpreadFrame = GetInterpolatedSpreadFrame(_currentAnimationStateTime, _currentRecordedFrameIndex);
        }

        private TerrainSpreadRecordedFrame GetInterpolatedSpreadFrame(float animationTime, int recordedSpreadFrameIndex)
        {
            // Making sure the index is valid.
            int totalFrameIndices = _recordedSpreadingFrames.Length;
            int lastFrameIndex = totalFrameIndices - 1;
            recordedSpreadFrameIndex = Mathf.Clamp(recordedSpreadFrameIndex, 0, lastFrameIndex);

            TerrainSpreadRecordedFrame previousFrame = _recordedSpreadingFrames[recordedSpreadFrameIndex];
            TerrainSpreadRecordedFrame nextFrame = _recordedSpreadingFrames[Mathf.Min(recordedSpreadFrameIndex + 1, lastFrameIndex)];

            float nextFrameTime = nextFrame.Time;
            float previousFrameTime = previousFrame.Time;
            float interpolationPercentage = Mathf.Clamp01(Mathf.InverseLerp(previousFrameTime, nextFrameTime, animationTime));

            return new TerrainSpreadRecordedFrame
            {
                Time = animationTime,
                SpreadRange = Mathf.Lerp(previousFrame.SpreadRange, nextFrame.SpreadRange, interpolationPercentage),
                GlobalEffectOpacity = Mathf.Lerp(previousFrame.GlobalEffectOpacity, nextFrame.GlobalEffectOpacity, interpolationPercentage),
                DarkCircleOpacity = Mathf.Lerp(previousFrame.DarkCircleOpacity, nextFrame.DarkCircleOpacity, interpolationPercentage),
            };
        }

        private int GetRecordedFrameIndex(float animationTime, int previouslyCachedFrameIndex = -1)
        {
            // Initial setup for the algorithm, optionally starting from a previously cached index for faster execution.
            int totalFrameIndices = _recordedSpreadingFrames.Length;
            int lastFrameIndex = totalFrameIndices - 1;
            int initialFrameIndex = previouslyCachedFrameIndex == -1 ? 0 : Mathf.Min(previouslyCachedFrameIndex, lastFrameIndex);

            // Checking if the initial frame index is valid or not.
            TerrainSpreadRecordedFrame potentialInitialFrame = _recordedSpreadingFrames[initialFrameIndex];
            if (potentialInitialFrame.Time > animationTime)
            {
                initialFrameIndex = 0;
            }

            // Go over all frames and find the one encapsulating the provided animation time.            
            TerrainSpreadRecordedFrame previousFrame = _recordedSpreadingFrames[initialFrameIndex];
            TerrainSpreadRecordedFrame nextFrame = _recordedSpreadingFrames[Mathf.Min(initialFrameIndex + 1, lastFrameIndex)];

            for (int i = initialFrameIndex; i < totalFrameIndices; i++)
            {
                if (previousFrame.Time <= animationTime && nextFrame.Time >= animationTime)
                {
                    return i;
                }

                int potentialNextFrameIndex = i + 1;
                if (potentialNextFrameIndex > lastFrameIndex)
                {
                    break;
                }

                previousFrame = nextFrame;
                nextFrame = _recordedSpreadingFrames[potentialNextFrameIndex];
            }

            // In case we haven't found the index of the frame for the specified animation time, return the last one.
            return lastFrameIndex;
        }
    }
}
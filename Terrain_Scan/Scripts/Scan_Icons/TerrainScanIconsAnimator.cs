using UnityEngine;

namespace GameDevBuddies
{
    /// <summary>
    /// Class responsible for updating animation properties of the terrain icons
    /// material and stopping the icons rendering after animation completes.
    /// </summary>
    public class TerrainScanIconsAnimator : MonoBehaviour
    {
        [Header("References: ")]
        [SerializeField] private TerrainScanLinesSpreadController _terrainScanLinesController = null;

        [Header("Opacity Animation Options: ")]
        [SerializeField] private int _cyclesCount = 6;
        [SerializeField] private float _fadeInDuration = 0.15f;
        [SerializeField] private float _fullyVisibleDuration = 1.5f;
        [SerializeField] private float _fadeOutDuration = 2f;
        [SerializeField] private float _fullyInvisibleDuration = 0.5f;
        [SerializeField] private float _spreadSpeed = 4f;
        [SerializeField] private float _unwalkableTerrainIconSpreadRangeOffset = 2f;
        [SerializeField] private float _appearingFadeInDistanceFromEdge = 1.25f;

        [Header("Hover Animation Options: ")]
        [SerializeField] private float _movementAmplitude = 0.025f;
        [SerializeField] private float _movementFrequency = 1f;
        [SerializeField] private float _movementOffsetMultiplier = 8f;

        private TerrainScanIconsRenderer _iconsRenderer = null;
        private ComputeShader _iconsComputeShader = null;
        private Material _iconsRenderMaterial = null;
        private float _iconsAnimationTime = 0f;

        /// <summary>
        /// Total duration of active icons.
        /// </summary>
        public float IconsActiveDuration { get { return (_cyclesCount + 1) * AnimationCycleDuration; } }

        /// <summary>
        /// Total duration of one animation cycle for each icon.
        /// </summary>
        private float AnimationCycleDuration { get { return _fadeInDuration + _fullyVisibleDuration + _fadeOutDuration + _fullyInvisibleDuration; } }
        private float CurrentSpreadDistance { get { return _terrainScanLinesController.CurrentSpreadRange; } }

        private void Update()
        {
            if (_iconsRenderer == null || !_iconsRenderer.IconsRenderingActive)
            {
                return;
            }

            // Update current animation time on the material.
            _iconsAnimationTime += Time.deltaTime;
            _iconsComputeShader.SetFloat("_CurrentAnimationTime", _iconsAnimationTime);
            _iconsComputeShader.SetFloat("_CurrentScanSpreadRange", CurrentSpreadDistance);
            _iconsComputeShader.SetFloat("_Time", Time.time);
            _iconsRenderMaterial.SetFloat("_CurrentTime", Time.time);

            // Stop the animation after all visible cycles have been completed (adding another invisible safety
            // cycle so that no visible popping occurs, and all icons fade out completely).
            if (_iconsAnimationTime >= (_cyclesCount + 1) * AnimationCycleDuration)
            {
                CompleteAnimation();
            }
        }

        public void StartIconsAnimation(TerrainScanIconsRenderer iconsRenderer, ComputeShader iconsComputeShader, Material iconsRenderMaterial)
        {
            // Cache required references.
            _iconsRenderer = iconsRenderer;
            _iconsComputeShader = iconsComputeShader;
            _iconsRenderMaterial = iconsRenderMaterial;
            _iconsAnimationTime = 0f;

            // Opacity animation properties that need to be set only once.             
            _iconsComputeShader.SetFloat("_TotalAnimationCycles", _cyclesCount);
            _iconsComputeShader.SetFloat("_FadeInDuration", _fadeInDuration);
            _iconsComputeShader.SetFloat("_FullyVisibleDuration", _fullyVisibleDuration);
            _iconsComputeShader.SetFloat("_FadeOutDuration", _fadeOutDuration);
            _iconsComputeShader.SetFloat("_FullyInvisibleDuration", _fullyInvisibleDuration);
            _iconsComputeShader.SetFloat("_SpreadSpeedMultiplier", 1.0f / _spreadSpeed);
            _iconsComputeShader.SetFloat("_AnimationCycleDuration", AnimationCycleDuration);
            _iconsComputeShader.SetFloat("_UnwalkableTerrainIconSpreadRangeOffset", _unwalkableTerrainIconSpreadRangeOffset);
            _iconsComputeShader.SetFloat("_AppearingFadeInDistanceFromEdge", _appearingFadeInDistanceFromEdge);

            // Movement animation properties that need to be set only once.            
            _iconsComputeShader.SetFloat("_MovementAmplitude", _movementAmplitude);
            _iconsComputeShader.SetFloat("_MovementFrequency", _movementFrequency);
            _iconsComputeShader.SetFloat("_MovementOffsetMultiplier", 1.0f / _movementOffsetMultiplier);

            // Refreshing the properties that will be updated every frame.
            _iconsComputeShader.SetFloat("_CurrentAnimationTime", _iconsAnimationTime);
            _iconsComputeShader.SetFloat("_CurrentScanSpreadRange", CurrentSpreadDistance);
            _iconsComputeShader.SetFloat("_Time", Time.time);
            _iconsRenderMaterial.SetFloat("_CurrentTime", Time.time);
        }

        private void CompleteAnimation()
        {
            _iconsRenderer.StopIconsRendering();
            _iconsRenderer = null;
            _iconsComputeShader = null;
        }
    }
}
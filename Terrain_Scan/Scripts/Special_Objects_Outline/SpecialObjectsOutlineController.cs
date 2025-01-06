using System;
using System.Collections;
using UnityEngine;

namespace GameDevBuddies
{
    /// <summary>
    /// Class that controls the outline of all special objects by animating the outline material 
    /// opacity over time. Animation consists of flickering X times on/off.
    /// </summary>
    public class SpecialObjectsOutlineController : MonoBehaviour
    {
        [Header("References: ")]
        [SerializeField] private Material _outlineMaterial = null;

        [Header("Settings: ")]
        [SerializeField] private int _flickerCount = 9;
        [SerializeField] private float _fadeInDuration = 0.1f;
        [SerializeField] private float _fullyVisibleDuration = 0.3f;
        [SerializeField] private float _fadeOutDuration = 0.2f;
        [SerializeField] private float _fullyInvisibleDuration = 0.1f;

        private TerrainScan _terrainScan = null;
        private Coroutine _outlineUpdateCoroutine = null;
        private float _outlineAlpha = 0f;

        private float OutlineAlpha
        {
            set
            {
                _outlineAlpha = value;
                UpdateMaterialAlpha();
            }
        }

        private void Start()
        {
            OutlineAlpha = 0f;

            _terrainScan = TerrainScan.Instance;
            _terrainScan.OnTerrainScanStart += StartOutlineOpacityAnimation;
        }

        private void OnDestroy()
        {
            if(_terrainScan != null)
            {
                _terrainScan.OnTerrainScanStart -= StartOutlineOpacityAnimation;
            }
        }

        private void StartOutlineOpacityAnimation(TerrainScanInfo _)
        {
            if(_outlineUpdateCoroutine != null)
            {
                StopCoroutine(_outlineUpdateCoroutine);
                _outlineUpdateCoroutine = null;
            }

            _outlineUpdateCoroutine = StartCoroutine(AnimateOutlineOpacityCoroutine());
        }

        private void UpdateMaterialAlpha()
        {
            _outlineMaterial.SetFloat("_OutlineVisibility", _outlineAlpha);
        }

        private IEnumerator AnimateOutlineOpacityCoroutine()
        {
            OutlineAlpha = 0f;

            float timer;
            for(int i = 0; i < _flickerCount; i++)
            {
                // Fade In.
                timer = 0f;
                while (timer <= _fadeInDuration)
                {
                    OutlineAlpha = Mathf.Clamp01(timer / _fadeInDuration);
                    timer += Time.deltaTime;
                    yield return null;
                }

                // Staying fully visible.
                timer = 0f;
                while (timer <= _fullyVisibleDuration)
                {
                    OutlineAlpha = 1.0f;
                    timer += Time.deltaTime;
                    yield return null;
                }

                // Fade Out.
                timer = 0f;
                while (timer <= _fadeOutDuration)
                {
                    OutlineAlpha = 1.0f - Mathf.Clamp01(timer / _fadeOutDuration);
                    timer += Time.deltaTime;
                    yield return null;
                }

                // Staying fully invisible.
                timer = 0f;
                while (timer <= _fullyInvisibleDuration)
                {
                    OutlineAlpha = 0.0f;
                    timer += Time.deltaTime;
                    yield return null;
                }

                yield return null;
            }

            _outlineUpdateCoroutine = null;
        }
    }
}
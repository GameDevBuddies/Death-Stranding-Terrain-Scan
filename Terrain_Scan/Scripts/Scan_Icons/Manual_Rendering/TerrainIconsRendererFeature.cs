using GameDevBuddies.CustomAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameDevBuddies
{
    [Serializable]
    public class TerrainIconsRenderPassSettings
    {
        [Header("Profiling: ")]
        public string ProfilingPassName;

        [Header("Render Target: ")]
        public RenderTexture OutputColorTexture;
        public RenderTexture OutputDepthTexture;
        public bool ShouldClearOutputTexture;

        [Header("Filtering Options: ")]
        public LayerMask SupportedLayersMask;
        public List<string> SupportedShaderPassNames;

        [Header("Overriding Render Material: ")]
        public Material OverrideMaterial;
        public int OverrideMaterialPassIndex;
    }

    public class TerrainIconsRendererFeature : ScriptableRendererFeature
    {
        [Header("Settings: ")]
        [SerializeField, TagAsString] private string _terrainIconsCameraTag = "Terrain_Icons_Camera";
        [SerializeField] private List<TerrainIconsRenderPassSettings> _renderPassesSettings = new List<TerrainIconsRenderPassSettings>();

        private TerrainIconsRenderPass _terrainIconsRenderPass = null;

        /// <inheritdoc/>
        public override void Create()
        {
            _terrainIconsRenderPass = new TerrainIconsRenderPass(_terrainIconsCameraTag, _renderPassesSettings);
            _terrainIconsRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        /// <inheritdoc/>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Only rendering terrain icons with the special orthographic terrain icons camera.
            if (!renderingData.cameraData.camera.CompareTag(_terrainIconsCameraTag))
            {
                return;
            }

            if (_terrainIconsRenderPass != null)
            {
                renderer.EnqueuePass(_terrainIconsRenderPass);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(_terrainIconsRenderPass != null)
            {
                _terrainIconsRenderPass.Dispose();
                _terrainIconsRenderPass = null;
            }
        }
    }
}
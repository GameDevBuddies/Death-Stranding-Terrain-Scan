using GameDevBuddies.CustomAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameDevBuddies
{
    [Serializable]
    public class SpecialObjectsOutlineNormalsPassSettings
    {
        [Header("Render Target: ")]
        public RenderTexture NormalsRT;

        [Header("Filtering Options: ")]
        public LayerMask SupportedLayersMask;
        public List<string> SupportedShaderPassNames;

        [Header("Override Materials: ")]
        public Material NormalsRenderMaterial;
        public int NormalsRenderMaterialPassIndex;
        public Material OutlineRenderMaterial;
        public int OutlineRenderMaterialPassIndex;
    }

    public class SpecialObjectsOutlineRenderer : ScriptableRendererFeature
    {
        [Header("General Settings: ")]
        [SerializeField, TagAsString] private string _mainCameraTag = "MainCamera";

        [Header("Normals Rendering Settings: ")]
        [SerializeField] private SpecialObjectsOutlineNormalsPassSettings _specialObjectsOutlinePassSettings = null;

        private SpecialObjectsOutlineRenderPass _specialObjectsOutlineRenderPass = null;

        /// <inheritdoc/>
        public override void Create()
        {
            _specialObjectsOutlineRenderPass = new SpecialObjectsOutlineRenderPass(_mainCameraTag, _specialObjectsOutlinePassSettings);
            _specialObjectsOutlineRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        /// <inheritdoc/>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Only rendering terrain icons with the special orthographic terrain icons camera.
            if (!renderingData.cameraData.camera.CompareTag(_mainCameraTag))
            {
                return;
            }

            if(_specialObjectsOutlinePassSettings == null || _specialObjectsOutlinePassSettings.NormalsRT == null)
            {
                return;
            }

            if (_specialObjectsOutlineRenderPass != null)
            {
                renderer.EnqueuePass(_specialObjectsOutlineRenderPass);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_specialObjectsOutlineRenderPass != null)
            {
                _specialObjectsOutlineRenderPass.Dispose();
                _specialObjectsOutlineRenderPass = null;
            }
        }
    }
}
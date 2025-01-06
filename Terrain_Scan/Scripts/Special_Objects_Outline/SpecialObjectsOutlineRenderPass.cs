using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GameDevBuddies
{
    /// <summary>
    /// Render pass that first renders normals of the special objects into another texture. Then it uses
    /// that texture to create outlines over the camera color texture for these special objects.
    /// </summary>
    public class SpecialObjectsOutlineRenderPass : ScriptableRenderPass
    {
        private RTHandle _normalsRTHandle = null;
        private RTHandle _temporaryRTHandle;
        private LayerMask _supportedLayersMask;
        private List<ShaderTagId> _supportedShaderTagIds;
        private FilteringSettings _filteringSettings;
        private Material _normalsRenderMaterial;
        private int _normalsRenderMaterialPassIndex;
        private Material _outlineRenderMaterial;
        private int _outlineRenderMaterialPassIndex;
        private string _cameraTagId = string.Empty;

        public SpecialObjectsOutlineRenderPass(string cameraTagId, SpecialObjectsOutlineNormalsPassSettings renderPassSettings)
        {
            if (renderPassSettings.NormalsRT == null)
            {
                return;
            }

            if (_normalsRTHandle == null)
            {
                _normalsRTHandle = RTHandles.Alloc(renderPassSettings.NormalsRT);
            }

            _supportedLayersMask = renderPassSettings.SupportedLayersMask;
            _supportedShaderTagIds = new List<ShaderTagId>();
            foreach (string supportedShaderPassName in renderPassSettings.SupportedShaderPassNames)
            {
                _supportedShaderTagIds.Add(new ShaderTagId(supportedShaderPassName));
            }
            _filteringSettings = new FilteringSettings(new RenderQueueRange(0, 5000), renderPassSettings.SupportedLayersMask);
            _normalsRenderMaterial = renderPassSettings.NormalsRenderMaterial;
            _normalsRenderMaterialPassIndex = renderPassSettings.NormalsRenderMaterialPassIndex;
            _outlineRenderMaterial = renderPassSettings.OutlineRenderMaterial;
            _outlineRenderMaterialPassIndex = renderPassSettings.OutlineRenderMaterialPassIndex;
            _cameraTagId = cameraTagId;
        }

        /// <inheritdoc/>
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //// Normals
            RenderTextureDescriptor textureDescriptor = renderingData.cameraData.renderer.cameraColorTargetHandle.rt.descriptor;
            textureDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            RenderingUtils.ReAllocateIfNeeded(ref _normalsRTHandle, textureDescriptor, FilterMode.Bilinear);

            ConfigureTarget(_normalsRTHandle, renderingData.cameraData.renderer.cameraDepthTargetHandle);
            ConfigureClear(ClearFlag.Color, Color.clear);
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Only rendering terrain icons with the special orthographic terrain icons camera.
            if (!renderingData.cameraData.camera.CompareTag(_cameraTagId))
            {
                return;
            }

            // Get a command buffer instance from the pool.
            CommandBuffer cmd = CommandBufferPool.Get();

            // Create drawing settings for this render pass based on the supported shader tags.
            DrawingSettings drawingSettings = CreateDrawingSettings(_supportedShaderTagIds, ref renderingData, SortingCriteria.CommonTransparent);
            if (_normalsRenderMaterial != null)
            {
                drawingSettings.overrideMaterial = _normalsRenderMaterial;
                drawingSettings.overrideMaterialPassIndex = _normalsRenderMaterialPassIndex;
            }

            // Update the camera culling mask to the desired rendering layers for this render pass.
            renderingData.cameraData.camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters);
            cullingParameters.cullingMask = (uint)(int)_supportedLayersMask;

            // Rendering normals of special objects into normals texture.
            using (new ProfilingScope(cmd, new ProfilingSampler("Special Objects Outline Normals")))
            {
                // Always clear the command buffer first.
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                // Clearing the current render target.
                cmd.ClearRenderTarget(false, true, Color.clear);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                RenderStateBlock renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
                renderStateBlock.mask |= RenderStateMask.Depth;
                renderStateBlock.depthState = new DepthState(writeEnabled: false, compareFunction: CompareFunction.LessEqual);
                context.DrawRenderers(context.Cull(ref cullingParameters), ref drawingSettings, ref _filteringSettings, ref renderStateBlock);
            }

            RenderTextureDescriptor textureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            textureDescriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref _temporaryRTHandle, textureDescriptor, FilterMode.Bilinear);

            // Using normals texture to create outlines over camera color texture.
            using (new ProfilingScope(cmd, new ProfilingSampler("Special Object Outlines")))
            {
                cmd.SetGlobalTexture("_SpecialObjectsNormals", _normalsRTHandle.rt);
                Blitter.BlitCameraTexture(cmd, renderingData.cameraData.renderer.cameraColorTargetHandle, _temporaryRTHandle, _outlineRenderMaterial, _outlineRenderMaterialPassIndex);
                Blitter.BlitCameraTexture(cmd, _temporaryRTHandle, renderingData.cameraData.renderer.cameraColorTargetHandle);
            }

            // Execute all commands of the command buffer.
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // Release command buffer back to the pool so that it can be reused.
            CommandBufferPool.Release(cmd);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_normalsRTHandle != null)
            {
                _normalsRTHandle = null;
            }

            if (_temporaryRTHandle != null)
            {
                _temporaryRTHandle = null;
            }

            if (_normalsRenderMaterial != null)
            {
                _normalsRenderMaterial = null;
            }
        }
    }
}
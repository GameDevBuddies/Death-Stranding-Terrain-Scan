using UnityEngine;

namespace GameDevBuddies 
{ 
    public class FootstepsRenderer : MonoBehaviour
    {
        [Header("References: ")]
        [SerializeField] private Mesh _footstepMesh = null;
        [SerializeField] private Material _footstepMaterial = null;

        // Indirect rendering helper variables.
        private GraphicsBuffer _drawArgumentsBuffer = null;
        private GraphicsBuffer.IndirectDrawIndexedArgs[] _drawArguments;
        private ComputeBuffer _footstepsBuffer = null;

        private void OnDestroy()
        {
            if(_drawArgumentsBuffer != null)
            {
                _drawArgumentsBuffer.Dispose();
                _drawArgumentsBuffer.Release();
                _drawArgumentsBuffer = null;
            }

            if (_footstepsBuffer != null)
            {
                _footstepsBuffer.Dispose();
                _footstepsBuffer.Release();
                _footstepsBuffer = null;
            }

            _drawArguments = null;
        }

        public void RenderFootsteps(FootstepInfo[] footsteps, int visibleFootstepsCount)
        {
            // Setting render parameters for rendering the footsteps.
            RenderParams renderParams = new RenderParams(_footstepMaterial);

            // Setting up correct world bounds for frustum culling of the footsteps.
            // Using "infinite" size hire since I don't care about culling for this example.
            // NOTE: Change size for your project to correctly cull the footsteps.
            renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 5000f);

            // Initializing the draw arguments buffer and the data.
            if(_drawArgumentsBuffer == null)
            {
                _drawArgumentsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
                _drawArguments = new GraphicsBuffer.IndirectDrawIndexedArgs[1]
                {
                    new GraphicsBuffer.IndirectDrawIndexedArgs
                    {
                        baseVertexIndex = 0,
                        startIndex = 0,
                        startInstance = 0,
                        indexCountPerInstance = _footstepMesh.GetIndexCount(0),
                        instanceCount = (uint)visibleFootstepsCount
                    }
                };
            }
            else
            {
                // Buffer and array already initialized, just update the instances count.
                _drawArguments[0].instanceCount = (uint)visibleFootstepsCount;
            }

            // Set the indirect draw data to the arguments buffer.
            _drawArgumentsBuffer.SetData(_drawArguments);

            // Set the compute buffer that will hold information about each footstep.
            if(_footstepsBuffer == null)
            {
                _footstepsBuffer = new ComputeBuffer(footsteps.Length, FootstepInfo.Size);                
            }

            // Updating the footsteps data on the GPU.
            _footstepsBuffer.SetData(footsteps);
            _footstepMaterial.SetBuffer("_Footsteps", _footstepsBuffer);

            Graphics.RenderMeshIndirect(renderParams, _footstepMesh, _drawArgumentsBuffer);
        }
    }
}
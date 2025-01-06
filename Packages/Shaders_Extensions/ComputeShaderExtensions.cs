using UnityEngine;

namespace GameDevBuddies
{
    /// <summary>
    /// Static class containing functionality that eases the use of <see cref="ComputeShader"/>s.
    /// </summary>
    public static class ComputeShaderExtensions
    {
        /// <summary>
        /// Function dispatches the <paramref name="computeShader"/> by first finding out the ID of the kernel that will be dispatched, specified 
        /// via the <paramref name="kernelFunctionName"/>, and then finding out how much kernel threads are required 
        /// for the desired amount of computations, specified by the <paramref name="kernelFunctionName"/>.
        /// </summary>
        /// <param name="computeShader">Reference to the <see cref="ComputeShader"/> instance that will be dispatched.</param>
        /// <param name="kernelFunctionName">Name of the kernel function that will be dispatched on the shader.</param>
        /// <param name="numberOfComputations">Number of computations required for dispatching.</param>
        public static void DispatchKernel(this ComputeShader computeShader, string kernelFunctionName, int numberOfComputations)
        {
            int kernelID = computeShader.FindKernel(kernelFunctionName);
            computeShader.DispatchThreads(kernelID, numberOfComputations);
        }

        /// <summary>
        /// Function dispatches the <paramref name="computeShader"/> by first finding out the ID of the kernel that will be dispatched, specified 
        /// via the <paramref name="kernelFunctionName"/>, and then finding out how much kernel threads are required 
        /// for the desired amount of computations, specified by the <paramref name="kernelFunctionName"/>.
        /// </summary>
        /// <param name="computeShader">Reference to the <see cref="ComputeShader"/> instance that will be dispatched.</param>
        /// <param name="kernelFunctionName">Name of the kernel function that will be dispatched on the shader.</param>
        /// <param name="numberOfComputationsX">Number of computations required for dispatching.</param>
        /// <param name="numberOfComputationsY">Number of computations required for dispatching.</param>
        public static void DispatchKernel(this ComputeShader computeShader, string kernelFunctionName, int numberOfComputationsX, int numberOfComputationsY)
        {
            int kernelID = computeShader.FindKernel(kernelFunctionName);
            computeShader.DispatchThreads(kernelID, numberOfComputationsX, numberOfComputationsY);
        }

        /// <summary>
        /// Function dispatches the <paramref name="computeShader"/> by finding out how much kernel threads are required
        /// for the desired amount of computations, specified by the <paramref name="numberOfComputations"/> parameter.
        /// <br>NOTE: This dispatch prepares a one dimensional thread dispatch group.</br>
        /// </summary>
        /// <param name="computeShader">Reference to the compute shader that will be dispatched.</param>
        /// <param name="kernelID">ID of the kernel function that will be dispatched on the shader.</param>
        /// <param name="numberOfComputations">Number of computations required for dispatching.</param>
        public static void DispatchThreads(this ComputeShader computeShader, int kernelID, int numberOfComputations)
        {
            computeShader.GetKernelThreadGroupSizes(kernelID, out uint x, out _, out _);

            int numberOfThreadGroups = (numberOfComputations + (int)x - 1) / (int)x;
            computeShader.Dispatch(kernelID, numberOfThreadGroups, 1, 1);
        }

        /// <summary>
        /// Function dispatches the <paramref name="computeShader"/> by finding out how much kernel threads are required
        /// for the desired amount of computations, specified by the <paramref name="numberOfComputationsX"/> and 
        /// <paramref name="numberOfComputationsY"/> parameters.
        /// <br>NOTE: This dispatch prepares a two dimensional thread dispatch group.</br>
        /// </summary>
        /// <param name="computeShader">Reference to the compute shader that will be dispatched.</param>
        /// <param name="kernelID">ID of the kernel function that will be dispatched on the shader.</param>
        /// <param name="numberOfComputationsX">Number of computations on X-axis, required for dispatching.</param>
        /// <param name="numberOfComputationsY">Number of computations on Y-axis, required for dispatching.</param>
        public static void DispatchThreads(this ComputeShader computeShader, int kernelID, int numberOfComputationsX, int numberOfComputationsY)
        {
            computeShader.GetKernelThreadGroupSizes(kernelID, out uint x, out uint y, out _);

            int numberOfThreadGroupsX = (numberOfComputationsX + (int)x - 1) / (int)x;
            int numberOfThreadGroupsY = (numberOfComputationsY + (int)y - 1) / (int)y;
            computeShader.Dispatch(kernelID, numberOfThreadGroupsX, numberOfThreadGroupsY, 1);
        }
    }
}

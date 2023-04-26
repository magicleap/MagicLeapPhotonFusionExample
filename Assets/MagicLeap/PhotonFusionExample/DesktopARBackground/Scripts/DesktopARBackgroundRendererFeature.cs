using UnityEngine;
using UnityEngine.Rendering;

using System;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


namespace MagicLeapNetworkingDemo.Desktop
{
    /// <summary>
    /// A render feature for rendering the camera background for standalone. Edited version of <see cref="DesktopARBackgroundRendererFeature"/>
    /// </summary>
    public class DesktopARBackgroundRendererFeature : ScriptableRendererFeature
    {

        /// <summary>
        /// The scriptable render pass to be added to the renderer when the camera background is to be rendered.
        /// </summary>
        ARCameraBeforeOpaquesRenderPass beforeOpaquesScriptablePass => m_BeforeOpaquesScriptablePass ??= new ARCameraBeforeOpaquesRenderPass();
        ARCameraBeforeOpaquesRenderPass m_BeforeOpaquesScriptablePass;

        /// <summary>
        /// The scriptable render pass to be added to the renderer when the camera background is to be rendered.
        /// </summary>
        ARCameraAfterOpaquesRenderPass afterOpaquesScriptablePass => m_AfterOpaquesScriptablePass ??= new ARCameraAfterOpaquesRenderPass();
        ARCameraAfterOpaquesRenderPass m_AfterOpaquesScriptablePass;

        /// <summary>
        /// Create the scriptable render pass.
        /// </summary>
        public override void Create() {}

        /// <summary>
        /// Add the background rendering pass when rendering a game camera with an enabled AR camera background component.
        /// </summary>
        /// <param name="renderer">The scriptable renderer in which to enqueue the render pass.</param>
        /// <param name="renderingData">Additional rendering data about the current state of rendering.</param>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var currentCamera = renderingData.cameraData.camera;
            if ((currentCamera != null) && (currentCamera.cameraType == CameraType.Game))
            {
                DesktopARCameraBackground cameraBackground = currentCamera.gameObject.GetComponent<DesktopARCameraBackground>();
                if ((cameraBackground != null) && cameraBackground.BackgroundRenderingEnabled
                    && (cameraBackground.Material != null)
                    && TrySelectRenderPassForBackgroundRenderMode(cameraBackground.CurrentRenderingMode, out var renderPass))
                {
               
                    renderPass.Setup(cameraBackground);
                    renderer.EnqueuePass(renderPass);
                }
            }
        }

        /// <summary>
        /// Selects the render pass for a given <see cref="UnityEngine.XR.ARSubsystems.XRCameraBackgroundRenderingMode"/>
        /// </summary>
        /// <param name="renderingMode">The <see cref="UnityEngine.XR.ARSubsystems.XRCameraBackgroundRenderingMode"/>
        /// that indicates which render pass to use.
        /// </param>
        /// <param name="renderPass">The <see cref="ARCameraBackgroundRenderPass"/> that corresponds
        /// to the given <paramref name="renderingMode">.
        /// </param>
        /// <returns>
        /// <c>true</c> if <paramref name="renderPass"/> was populated. Otherwise, <c>false</c>.
        /// </returns>
        bool TrySelectRenderPassForBackgroundRenderMode(XRCameraBackgroundRenderingMode renderingMode, out DesktopARCameraBackgroundRenderPass renderPass)
        {
            switch (renderingMode)
            {
                case XRCameraBackgroundRenderingMode.AfterOpaques:
                    renderPass = afterOpaquesScriptablePass;
                    return true;

                case XRCameraBackgroundRenderingMode.BeforeOpaques:
                    renderPass = beforeOpaquesScriptablePass;
                    return true;

                case XRCameraBackgroundRenderingMode.None:
                default:
                    renderPass = null;
                    return false;
            }
        }

        /// <summary>
        /// An abstract (xref: UnityEngine.Rendering.Universal.ScriptableRenderPass) that provides common
        /// utilities for rendering an AR Camera Background.
        /// </summary>
        abstract class DesktopARCameraBackgroundRenderPass : ScriptableRenderPass
        {
            /// <summary>
            /// The name for the custom render pass which will display in graphics debugging tools.
            /// </summary>
            const string k_CustomRenderPassName = "Desktop AR Background Pass (URP)";

        

            /// <summary>
            /// The material used for rendering the device background using the camera video texture and potentially
            /// other device-specific properties and textures.
            /// </summary>
            Material m_BackgroundMaterial;

            /// <summary>
            /// The projection matrix used to render the <see cref="mesh"/>.
            /// </summary>
            protected abstract Matrix4x4 projectionMatrix { get; }

            /// <summary>
            /// The (xref: UnityEngine.Mesh) used in this custom render pass.
            /// </summary>
            protected abstract Mesh mesh { get; }

            /// <summary>
            /// Set up the background render pass.
            /// </summary>
            /// <param name="cameraBackground">The <see cref="ARCameraBackground"/>
            ///  component that provides the (xref: UnityEngine.Material)
            /// and any additional rendering information required by the render pass.
            /// </param>
            /// <param name="invertCulling">Whether the culling mode should be inverted.</param>
            public void Setup(DesktopARCameraBackground cameraBackground)
            {
                SetupInternal(cameraBackground);
                m_BackgroundMaterial = cameraBackground.Material;
            }

            /// <summary>
            /// Provides inheritors an opportunity to perform any specialized setup during
            /// (xref: UnityEngine.Rendering.Universal.ScriptableRenderPass.Setup).
            /// </summary>
            /// <param name="cameraBackground">The <see cref="ARCameraBackground"/> component
            /// that provides the (xref: UnityEngine.Material)
            /// and any additional rendering information required by the render pass.
            /// </param>
            protected virtual void SetupInternal(DesktopARCameraBackground cameraBackground) {}

            /// <summary>
            /// Execute the commands to render the camera background.
            /// </summary>
            /// <param name="context">The render context for executing the render commands.</param>
            /// <param name="renderingData">Additional rendering data about the current state of rendering.</param>
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get(k_CustomRenderPassName);
                cmd.BeginSample(k_CustomRenderPassName);

                cmd.SetInvertCulling(false);

                cmd.SetViewProjectionMatrices(Matrix4x4.identity, projectionMatrix);

                cmd.DrawMesh(mesh, Matrix4x4.identity, m_BackgroundMaterial);

                cmd.SetViewProjectionMatrices(renderingData.cameraData.camera.worldToCameraMatrix,
                                              renderingData.cameraData.camera.projectionMatrix);

                cmd.EndSample(k_CustomRenderPassName);
                context.ExecuteCommandBuffer(cmd);

                CommandBufferPool.Release(cmd);
            }

            /// <summary>
            /// Clean up any resources for the render pass.
            /// </summary>
            /// <param name="commandBuffer">The command buffer for frame cleanup.</param>
            public override void FrameCleanup(CommandBuffer commandBuffer)
            {
            }
        }

        /// <summary>
        /// The custom render pass to render the camera background before rendering opaques.
        /// </summary>
        class ARCameraBeforeOpaquesRenderPass : DesktopARCameraBackgroundRenderPass
        {
            /// <summary>
            /// Constructs the background render pass.
            /// </summary>
            public ARCameraBeforeOpaquesRenderPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            }

            /// <summary>
            /// Configure the render pass by setting the render target and clear values.
            /// </summary>
            /// <param name="commandBuffer">The command buffer for configuration.</param>
            /// <param name="renderTextureDescriptor">The descriptor of the target render texture.</param>
            public override void Configure(CommandBuffer commandBuffer, RenderTextureDescriptor renderTextureDescriptor)
            {
                ConfigureClear(ClearFlag.Depth, Color.clear);
            }

            /// <inheritdoc />
            protected override Matrix4x4 projectionMatrix => DesktopARCameraBackground.BeforeOpaquesOrthoProjection;

            /// <inheritdoc />
            protected override Mesh mesh => DesktopARCameraBackground.FullScreenNearClipMesh;
        }

        /// <summary>
        /// The custom render pass to render the camera background after rendering opaques.
        /// </summary>
        class ARCameraAfterOpaquesRenderPass : DesktopARCameraBackgroundRenderPass
        {
            /// <summary>
            /// Constructs the background render pass.
            /// </summary>
            public ARCameraAfterOpaquesRenderPass()
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            }

            /// <summary>
            /// Configure the render pass by setting the render target and clear values.
            /// </summary>
            /// <param name="commandBuffer">The command buffer for configuration.</param>
            /// <param name="renderTextureDescriptor">The descriptor of the target render texture.</param>
            public override void Configure(CommandBuffer commandBuffer, RenderTextureDescriptor renderTextureDescriptor)
            {
                ConfigureClear(ClearFlag.None, Color.clear);
            }

            /// <inheritdoc />
            protected override void SetupInternal(DesktopARCameraBackground cameraBackground)
            {
                if (cameraBackground.GetComponent<AROcclusionManager>()?.enabled ?? false)
                {
                    // If an occlusion texture is being provided, rendering will need
                    // to compare it against the depth texture created by the camera.
                    ConfigureInput(ScriptableRenderPassInput.Depth);
                }
            }

            /// <inheritdoc />
            protected override Matrix4x4 projectionMatrix => DesktopARCameraBackground.AfterOpaquesOrthoProjection;

            /// <inheritdoc />
            protected override Mesh mesh => DesktopARCameraBackground.FullScreenFarClipMesh;
        }

    }
}

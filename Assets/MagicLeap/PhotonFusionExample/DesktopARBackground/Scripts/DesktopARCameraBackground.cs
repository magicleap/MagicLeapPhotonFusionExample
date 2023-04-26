using Klak.TestTools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace MagicLeapNetworkingDemo.Desktop
{
    /// <summary>
    /// Edited version of <see cref="ARCameraBackground"/> that allows the script to run on standalone. (Only works in URP)
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     This requires <see cref="DesktopARBackgroundRendererFeature"/> to be add to your render pipeline.
    ///     </para>
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class DesktopARCameraBackground : MonoBehaviour
    {
        /// <summary>
        /// The camera to which the projection matrix is set on each frame event.
        /// </summary>
        private Camera _camera;

        /// <summary>
        /// Command buffer for any custom rendering commands.
        /// </summary>
        private CommandBuffer _commandBuffer;

        /// <summary>
        /// The previous clear flags for the camera, if any.
        /// </summary>
        private CameraClearFlags? _previousCameraClearFlags;

        /// <summary>
        /// The original field of view of the camera, before enabling background rendering.
        /// </summary>
        private float? _previousCameraFieldOfView;

        /// <summary>
        /// The original depth mode for the camera.
        /// </summary>
        private DepthTextureMode _previousCameraDepthMode;


        /// <summary>
        /// True if background rendering is enabled, false otherwise.
        /// </summary>
        private bool _backgroundRenderingEnabled;


        /// <summary>
        /// The current <c> Material </c> used for background rendering.
        /// </summary>
        public Material Material => _defaultMaterial;

        /// <summary>
        /// Whether background rendering is enabled.
        /// </summary>
        /// <value>
        /// <c> true </c> if background rendering is enabled and if at least one camera frame has been received.
        /// Otherwise, <c> false </c>.
        /// </value>
        public bool BackgroundRenderingEnabled => _backgroundRenderingEnabled;

        /// <summary>
        /// The default material for rendering the background.
        /// </summary>
        /// <value>
        /// The default material for rendering the background.
        /// </value>
        private Material _defaultMaterial;

        [SerializeField] private bool _enable = true;
        #if APRIL_TAGS
        public ImageSource WebcamImageSource;
        #endif
        public XRCameraBackgroundRenderingMode CurrentRenderingMode = XRCameraBackgroundRenderingMode.None;
        
        private XRCameraBackgroundRenderingMode _commandBufferRenderOrderState = XRCameraBackgroundRenderingMode.None;



        private void Awake()
        {
#if !APRIL_TAGS
            _enable = false;
#endif
            if (_enable == false)
            {
                enabled = false;
                return;
            }

            _camera = GetComponent<Camera>();
            _defaultMaterial = new Material(Shader.Find("Custom/DesktopARBackground"));
        }

        private void OnDestroy()
        {
            if (_enable == false)
            {
                return;
            }
            Destroy(_defaultMaterial);
        }

        private void OnEnable()
        {
            if (_enable == false)
            {
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            // Ensure that background rendering is disabled until the first camera frame is received.
            _backgroundRenderingEnabled = false;
#if APRIL_TAGS
            WebcamImageSource.CameraTexture += OnCameraFrameReceived;
#endif
            _previousCameraDepthMode = _camera.depthTextureMode;
#endif
        }

        private void OnDisable()
        {
            if (_enable == false)
            {
                return;
            }
#if UNITY_EDITOR || UNITY_STANDALONE
#if APRIL_TAGS
            WebcamImageSource.CameraTexture -= OnCameraFrameReceived;
#endif
            DisableBackgroundRendering();

            // We are no longer setting the projection matrix so tell the camera to resume its normal projection matrix
            // calculations.
            _camera.ResetProjectionMatrix();
#endif
        }

        /// <summary>
        /// Enable background rendering by disabling the camera's clear flags, and enabling the legacy RP background
        /// rendering if your application is in legacy RP mode.
        /// </summary>
        private void EnableBackgroundRendering()
        {
            _backgroundRenderingEnabled = true;


            DisableBackgroundClearFlags();

            _previousCameraFieldOfView = _camera.fieldOfView;
        }

        /// <summary>
        /// Disable background rendering by disabling the legacy RP background rendering if your application is  in legacy RP mode
        /// and restoring the camera's clear flags.
        /// </summary>
        private void DisableBackgroundRendering()
        {
            _backgroundRenderingEnabled = false;


            RestoreBackgroundClearFlags();

            if (_previousCameraFieldOfView.HasValue)
            {
                _camera.fieldOfView = _previousCameraFieldOfView.Value;
                _previousCameraFieldOfView = null;
            }
        }

        /// <summary>
        /// Set the camera's clear flags to do nothing while preserving the previous camera clear flags.
        /// </summary>
        private void DisableBackgroundClearFlags()
        {
            _previousCameraClearFlags = _camera.clearFlags;
            _camera.clearFlags = CurrentRenderingMode == XRCameraBackgroundRenderingMode.AfterOpaques ? CameraClearFlags.Depth : CameraClearFlags.Nothing;
        }

        /// <summary>
        /// Restore the previous camera's clear flags, if any.
        /// </summary>
        private void RestoreBackgroundClearFlags()
        {
            if (_previousCameraClearFlags != null)
            {
                _camera.clearFlags = _previousCameraClearFlags.Value;
            }
        }


        private static bool _initializedFarClipMesh;
        private static Mesh _farClipMesh;

        public static Mesh FullScreenFarClipMesh
        {
            get
            {
                if (!_initializedFarClipMesh)
                {
                    _farClipMesh = BuildFullscreenMesh(-1f);
                    _initializedFarClipMesh = _farClipMesh != null;
                }

                return _farClipMesh;
            }
        }

        private static bool _initializedNearClipMesh;
        private static Mesh _nearClipMesh;

        public static Mesh FullScreenNearClipMesh
        {
            get
            {
                if (!_initializedNearClipMesh)
                {
                    _nearClipMesh = BuildFullscreenMesh(0.1f);
                    _initializedNearClipMesh = _nearClipMesh != null;
                }

                return _nearClipMesh;
            }
        }

        private static Mesh BuildFullscreenMesh(float zVal)
        {
            const float bottomV = 0f;
            const float topV = 1f;
            var mesh = new Mesh { vertices = new[] { new(0f, 0f, zVal), new Vector3(0f, 1f, zVal), new Vector3(1f, 1f, zVal), new Vector3(1f, 0f, zVal) }, uv = new[] { new Vector2(0f, bottomV), new Vector2(0f, topV), new Vector2(1f, topV), new Vector2(1f, bottomV) }, triangles = new[] { 0, 1, 2, 0, 2, 3 } };

            mesh.UploadMeshData(false);
            return mesh;
        }


        public static Matrix4x4 BeforeOpaquesOrthoProjection { get; } = Matrix4x4.Ortho(0f, 1f, 0f, 1f, -0.1f, 9.9f);


        /// <summary>
        /// The orthogonal projection matrix for the after opaque background rendering. For use when drawing the
        /// <see cref="FullScreenFarClipMesh"/>.
        /// </summary>
        public static Matrix4x4 AfterOpaquesOrthoProjection { get; } = Matrix4x4.Ortho(0, 1, 0, 1, 0, 1);


        /// <summary>
        /// Callback for the camera frame event.
        /// </summary>
        private void OnCameraFrameReceived(Texture frame)
        {
            var activeRenderingMode = CurrentRenderingMode;
            // Enable background rendering when first frame is received.
            if (_backgroundRenderingEnabled)
            {
                if (frame == null || activeRenderingMode == XRCameraBackgroundRenderingMode.None)
                {
                    DisableBackgroundRendering();
                    return;
                }

                if (_commandBuffer != null)
                {
                    if (_commandBufferRenderOrderState != activeRenderingMode)
                    {
                        RestoreBackgroundClearFlags();
                        SetCameraDepthTextureMode(activeRenderingMode);
                        DisableBackgroundClearFlags();
                        _commandBufferRenderOrderState = CurrentRenderingMode;
                    }
                }
            }
            else if (frame != null && activeRenderingMode != XRCameraBackgroundRenderingMode.None)
            {
                EnableBackgroundRendering();
            }

            if (Material != null)
            {
                var count = 1;
                for (var i = 0; i < count; ++i)
                {
                    Material.SetTexture("_MainTex", frame);
                }

                Material.SetVector("_MainTex_ST", new Vector4(0, 0, frame.width, frame.height));
            }
        }

        /// <summary>
        /// Ensure the camera generates a depth texture after opaques for use when comparing environment depth when rendering
        /// after opaques.
        /// </summary>
        private void SetCameraDepthTextureMode(XRCameraBackgroundRenderingMode mode)
        {
            switch (mode)
            {
                case XRCameraBackgroundRenderingMode.AfterOpaques:
                {
                    break;
                }

                case XRCameraBackgroundRenderingMode.None:
                case XRCameraBackgroundRenderingMode.BeforeOpaques:
                default:
                    _camera.depthTextureMode = _previousCameraDepthMode;
                    break;
            }
        }
    }
}

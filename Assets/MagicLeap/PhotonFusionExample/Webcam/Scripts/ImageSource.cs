using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;

namespace Klak.TestTools {
/// <summary>
/// Modified version of Keijiro Takahashi's Image Source script: <a href="https://github.com/keijiro/TestTools/blob/main/Packages/jp.keijiro.klak.testtools/Script/ImageSource.cs"> Link to Github Repository </a>.
/// </summary>
/// <remarks>Unlicensed: <a href="https://github.com/keijiro/TestTools/blob/main/Packages/jp.keijiro.klak.testtools/LICENSE">Original License</a>.</remarks>
public sealed class ImageSource : MonoBehaviour
{
    #region Public property

    public Texture Texture => OutputBuffer;
    public Vector2Int OutputResolution => _outputResolution;
    public Action<Texture> CameraTexture;
    #endregion

    #region Editable attributes



    // Webcam options
    [SerializeField] string _webcamName = "";
    [SerializeField] Vector2Int _webcamResolution = new Vector2Int(1920, 1080);
    [SerializeField] int _webcamFrameRate = 30;

    [SerializeField] bool _fullScreenResolution;
    // Output options
    [SerializeField] RenderTexture _outputTexture = null;
    [SerializeField] Vector2Int _outputResolution = new Vector2Int(1920, 1080);

    #endregion


    #region Private members


    private WebCamTexture _webcam;
    private RenderTexture _buffer;
    private Material _rotateMaterial;
    private RenderTexture _webFlipped;
    private bool _running;
    private RenderTexture OutputBuffer => _outputTexture != null ? _outputTexture : _buffer;

    // Blit a texture into the output buffer with aspect ratio compensation.
    void Blit(Texture source, bool vflip = false)
    {
        if (source == null) return;
   
        var aspect1 = (float)source.width / source.height;
        var aspect2 = (float)OutputBuffer.width / OutputBuffer.height;

        var scale = new Vector2(aspect2 / aspect1, aspect1 / aspect2);
        scale = Vector2.Min(Vector2.one, scale);
        if (vflip) scale.y *= -1;

        var offset = (Vector2.one - scale) / 2;
       Graphics.Blit(source, OutputBuffer, scale, offset);
      
    }
    #endregion

    #region MonoBehaviour implementation

    public int ScreenWidth
    {
        get
        {
            if (UnityEngine.XR.XRSettings.enabled)
            {
                return XRSettings.eyeTextureWidth;
            }
            else
            {
                return Screen.width;
            }
        }
    }

    public int ScreenHeight
    {
        get
        {
            if (UnityEngine.XR.XRSettings.enabled)
            {
                return XRSettings.eyeTextureHeight;
            }
            else
            {
                return Screen.height;
            }
        }
    }
    void Start()
    {
        if (_fullScreenResolution)
        {
            _outputResolution.y = ScreenHeight;
            _outputResolution.x = ScreenWidth;
        }

        _rotateMaterial = new Material(Shader.Find("Custom/Web Camera Shader"));
        // Allocate a render texture if no output texture has been given.
        if (_outputTexture == null)
        {
            _buffer = new RenderTexture(_outputResolution.x, _outputResolution.y, 0);
        }


            // Webcam source type:
            // Create a WebCamTexture and start capturing.
            _webcam = new WebCamTexture(_webcamName, _webcamResolution.x, _webcamResolution.y, _webcamFrameRate);
            if (_webcam == null)
            {
                Debug.LogError("Failed to connect to camera.");
            }

        
    }

    public void StartCamera()
    {
        if (_webcam == null)
        {
            Debug.LogError("Camera is not connected so it cannot be started.");
            return;
        }
        _webcam.Play();
        _rotateMaterial.SetFloat("_RotationDegrees", -_webcam.videoRotationAngle);

        if (_webFlipped == null)
        {
            if (Mathf.Abs(_webcam.videoRotationAngle) == 90)
            {
                _webFlipped = new RenderTexture(_webcam.height, _webcam.width, 0);
            }
            else
            {
                _webFlipped = new RenderTexture(_webcam.width, _webcam.height, 0);
            }
        }

        _running = true;
    }

    public void StopCamera()
    {
        if (!_running)
        {
            Debug.LogError("Camera is not running so it cannot be stopped.");
            return;
        }
        _webcam.Stop();
        _running = false;
    

    }

    void OnDestroy()
    {
        if (_webcam != null) Destroy(_webcam);
        if (_buffer != null) Destroy(_buffer);
        if (_rotateMaterial != null) Destroy(_rotateMaterial);
        if (_webFlipped != null) Destroy(_webFlipped);
    }

    void Update()
    {
        if (_running)
        {
            RotateBlit(_webcam);
        }
    }
    void RotateBlit(Texture source)
    {
     
        Graphics.Blit(source, _webFlipped, _rotateMaterial);
    
        Blit(_webFlipped, _webcam.videoVerticallyMirrored);
        CameraTexture?.Invoke(source);
        
    }

    #endregion
}

} // namespace Klak.TestTools

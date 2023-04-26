using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if APRIL_TAGS
using AprilTag.Interop;
#endif
using UI = UnityEngine.UI;
using Klak.TestTools;
using MagicLeap;
using MagicLeap.Utils;
using MagicLeapNetworkingDemo.MarkerTracking;
using UnityEngine.XR.MagicLeap;

/// <summary>
/// Tracks April Tags on multiple platforms.
/// </summary>
/// <remarks>
///     <para>
///     Credit: Keijiro Takahashi's April Tags for Unity: <a href="https://github.com/keijiro/jp.keijiro.apriltag"> Github Repository Link</a>.
///     </para>
/// <para>
/// The script extends the marker detection by tracking the marker and managing the calling <see cref = "MarkerTrackerActions"/> for each marker.
/// </para>
/// </remarks>
sealed class GenericAprilTagTracker : BaseMarkerTrackerBehaviour
{
    /// <inheritdoc />
    public override bool SupportedOnPlatform
    {
        get
        {
#if !APRIL_TAGS
            return false;
#endif
#if UNITY_ANDROID && ANDROID_X86_64
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!Application.isEditor || UnityEngine.XR.MagicLeap.MagicLeapXrProvider.IsZIRunning)
            {
                return false;
            }
#endif
            return true;

        }
    }
    
    [SerializeField] private List<int> _trackedMarkerIds = new List<int>();
#if APRIL_TAGS
    [SerializeField] ImageSource _source = null;
    [SerializeField] private Family.TagType _markerType = Family.TagType.Tag36h11;
#endif

    [SerializeField] int _decimation = 4;
    [SerializeField] private float _maxLostTime = 3;
    [SerializeField] bool _trackOnStart = false;
    [Header("Debug Settings")]
    [SerializeField] float _tagSize = 0.05f;

    [SerializeField] Canvas _webcamCanvas = null;
    [SerializeField] UI.RawImage _webcamPreview = null;
 




#region Tracking Properties

    private List<int> _lostMarkerIds;
    private List<int> _updatedMarkerIds;
    private Dictionary<int, MarkerTrackerActions> _arucoActionsByMarkerId;
    private Dictionary<int, float> _lastTrackedTimeByMarkerId;
    private float _countDownTime;
#if APRIL_TAGS
    private AprilTag.TagDetector _detector;
#endif
    private bool _running;

#endregion
 
    private void Awake()
    {
        if (!SupportedOnPlatform)
        {
            return;
        }

        if (_webcamCanvas)
        {
            _webcamCanvas.enabled = false;
        }
        _arucoActionsByMarkerId = new Dictionary<int, MarkerTrackerActions>();
        foreach (var id in _trackedMarkerIds)
        {
            _arucoActionsByMarkerId.Add(id, new MarkerTrackerActions());
        }
    }

    IEnumerator Start()
    {
        yield return null;
        if(_trackOnStart)
        {
            StartTracking();
        }
    }

    public override void StartTracking()
    {
        if (!SupportedOnPlatform)
        {
            return;
        }
#if APRIL_TAGS
    
        if (_detector != null)
        {
            Debug.unityLogger.LogWarning("MARKER-TRACKING", "Cannot start marker tracking because it is already started.");
            return;
        }


        _source.StartCamera();
        _countDownTime = _maxLostTime;
        _updatedMarkerIds = new List<int>();
        _lostMarkerIds = new List<int>();
        _lastTrackedTimeByMarkerId = new Dictionary<int, float>();

        if (_source == null)
        {
            _source = FindObjectOfType<ImageSource>();
        }

        var dims = _source.OutputResolution;
        _detector = new AprilTag.TagDetector(dims.x, dims.y, _decimation, _markerType);
        _running = true;
        if (_webcamCanvas)
        {
            _webcamCanvas.enabled = true;
        }

     
#endif
    }

    public override void StopTracking()
    {
        if (!SupportedOnPlatform)
        {
            return;
        }

#if APRIL_TAGS
    
        if (_detector == null)
        {
            return;
        }

        _source.StopCamera();
        _running = false;
        _detector?.Dispose();
        _detector = null;
        //Delayed to insure that the method is called after the last marker update. Otherwise the marker could be re-added directly after removal
        ThreadDispatcher.RunOnMainThread(() =>
                                         {
                                             RemoveAllTrackedMarkers();
                                       
                                             if (_webcamCanvas)
                                             {
                                                 _webcamCanvas.enabled = false;
                                             }
                                         });
        
 
#endif
    }
    void OnDestroy()
    {
        StopTracking();
        
    }

    private void Update()
    {
    #if APRIL_TAGS
        if (!_running || !SupportedOnPlatform)
        {
            return;
        }

        if (_webcamPreview)
        {
            _webcamPreview.texture = _source.Texture;
        }
        if (_countDownTime > 0)
        {
            _countDownTime -= Time.deltaTime;
        }
#endif
    }


    void LateUpdate()
    {
#if APRIL_TAGS
        if (!_running||!SupportedOnPlatform || _source.Texture== null)
        {
            return;
        }
        
        // Source image acquisition
        var image = _source.Texture.AsSpan();
        if (image.IsEmpty)
        {
            return;
        }

        // AprilTag detection
        var fov = Camera.main.fieldOfView * Mathf.Deg2Rad;
        _detector?.ProcessImage(image, fov, _tagSize);
        
        // Detected tag tracking
        foreach (var aprilTag in _detector.DetectedTags)
        {
     
            OnTrackerResultsFound(aprilTag.ID, aprilTag.Position, aprilTag.Rotation * Quaternion.Euler(new Vector3(0, 180, 0)));

        }

#endif
    }


    private void RemoveAllTrackedMarkers()
    {
        //Check to see if something is being tracked
        if (_lastTrackedTimeByMarkerId != null)
        {
            var currentlyTrackedMarkers = _lastTrackedTimeByMarkerId.Keys;
            foreach (var markerId in currentlyTrackedMarkers)
            {
                var trackerActions = _arucoActionsByMarkerId[markerId];
                trackerActions?.Removed();
            }

            _updatedMarkerIds.Clear();
            _lastTrackedTimeByMarkerId.Clear();
            _lostMarkerIds.Clear();
        }
    }

    private void OnTrackerResultsFound(int id, Vector3 position, Quaternion rotation)
    {
        if (!SupportedOnPlatform || !_running)
        {
            return;
        }


        if (!_trackedMarkerIds.Contains(id))
        {
        
            return;
        }

        var trackerActions = _arucoActionsByMarkerId[id];
        if (!_updatedMarkerIds.Contains(id))
        {
            _updatedMarkerIds.Add(id);
        }

        trackerActions.Updated?.Invoke(new MarkerPose((uint)id, position, rotation));


        if (_lastTrackedTimeByMarkerId.ContainsKey(id))
        {
            _lastTrackedTimeByMarkerId[id] = Time.time;
        }
        else
        {
            _lastTrackedTimeByMarkerId.Add(id, Time.time);
            trackerActions?.Added?.Invoke();
        }

        if (_countDownTime <= 0)
        {
            TrackLostMarkers();

            _countDownTime = _maxLostTime;
        }

     
    }

    private void TrackLostMarkers()
    {
        if (!SupportedOnPlatform)
        {
            return;
        }

        _lostMarkerIds.Clear();
        for (int i = 0; i < _trackedMarkerIds.Count; i++)
        {
            var id = _trackedMarkerIds[i];
            if (!_updatedMarkerIds.Contains(id))
            {
                if (_lastTrackedTimeByMarkerId.TryGetValue(id, out var lastUpdateTime))
                {
                    if (Time.time - lastUpdateTime > _maxLostTime)
                    {
                        if (_arucoActionsByMarkerId.TryGetValue(id, out var trackerActions))
                        {
                            trackerActions.Removed?.Invoke();
                            _lostMarkerIds.Add(id);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < _lostMarkerIds.Count; i++)
        {
            _lastTrackedTimeByMarkerId.Remove(_lostMarkerIds[i]);
        }

        _updatedMarkerIds.Clear();
    }
    /// <inheritdoc />
    public override MarkerTrackerActions GetActionsForId(int id)
    {
        if (_arucoActionsByMarkerId == null)
        {
            return new MarkerTrackerActions();
        }
        if (_arucoActionsByMarkerId.TryGetValue(id, out var actions))
        {
            return actions;
        }

        Debug.LogError($"Cannot get marker With ID [{id}] because it is not tracked");
        return null;
    }
}

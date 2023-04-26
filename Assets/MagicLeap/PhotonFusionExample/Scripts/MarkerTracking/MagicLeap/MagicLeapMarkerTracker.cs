using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MagicLeap;
using MagicLeap.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeapNetworkingDemo.MarkerTracking
{
    /// <summary>
    /// Marker tracker for Magic Leap 2. 
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     To learn more about Magic Leap 2 target tracking <a href="https://developer-docs.magicleap.cloud/docs/guides/unity/marker-tracking/marker-tracker-overview"> see the documentation</a>.
    ///     </para>
    ///     <para>
    ///     The script extends the marker detection by tracking the marker and managing the calling <see cref="MarkerTrackerActions"/> for each marker.
    ///     </para>
    /// </remarks>
    public class MagicLeapMarkerTracker : BaseMarkerTrackerBehaviour
    {
        /// <inheritdoc/>
        public override bool SupportedOnPlatform
        {
            get
            {
#if UNITY_ANDROID && ANDROID_X86_64
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (!Application.isEditor || MagicLeapXrProvider.IsZIRunning)
                {
                    return true;
                }

#endif
                return false;
            }
        }

        /// <summary>
        /// Settings to be used for the tracker
        /// </summary>
        [SerializeField] private MarkerTrackerSettings _markerTrackerSettings;

        [SerializeField] private List<int> _trackedMarkerIds;
        [SerializeField] private float _maxLostTime = 3;
        [SerializeField] private bool _trackOnStart;


    #region Magic Leap Properties

        private readonly MLPermissions.Callbacks _permissionCallbacks = new();
        private bool _scanning;
        private Task _startScanningTask;
        private Task _stopScanningTask;
        private bool _permissionGranted;

    #endregion

    #region Tracking Properties

        private List<int> _lostMarkerIds;
        private List<int> _updatedMarkerIds;
        private Dictionary<int, MarkerTrackerActions> _arucoActionsByMarkerId;
        private Dictionary<int, float> _lastTrackedTimeByMarkerId;
        private float _countDownTime;

    #endregion

        private void Awake()
        {
            if (!SupportedOnPlatform)
            {
                return;
            }

            _countDownTime = _maxLostTime;
            _updatedMarkerIds = new List<int>();
            _lostMarkerIds = new List<int>();
            _lastTrackedTimeByMarkerId = new Dictionary<int, float>();

            _arucoActionsByMarkerId = new Dictionary<int, MarkerTrackerActions>();
            foreach (var id in _trackedMarkerIds)
            {
                _arucoActionsByMarkerId.Add(id, new MarkerTrackerActions());
            }
        }

        private void Start()
        {
            if (!SupportedOnPlatform)
            {
                return;
            }

            RequestPermissions();
        }

        // private void OnApplicationPause(bool pauseStatus)
        // {
        //     if (pauseStatus && _scanning && SupportedOnPlatform)
        //     {
        //         _scanning = false;
        //         _stopScanningTask = MLMarkerTracker.StopScanningAsync();
        //         _stopScanningTask.GetAwaiter().GetResult();
        //
        //     }
        //     else
        //     {
        //         StartTracking();
        //     }
        // }

        private void OnDisable()
        {
            StopTracking();
        }


        /// <summary>
        /// Starts marker tracking on the MagicLeap
        /// </summary>
        public override void StartTracking()
        {
#if UNITY_ANDROID && ANDROID_X86_64
            if (!SupportedOnPlatform)
            {
                return;
            }

            if (!_permissionGranted)
            {
                Debug.unityLogger.LogError("MARKER-TRACKING", $"Marker tracking failed to start. Error: Permission not granted.");
            }

            if (_scanning == false && (_startScanningTask == null || _startScanningTask.IsCompleted))
            {
                try
                {
                   ThreadDispatcher.RunOnWorkerThread(() =>
                                                      {
                                                           //Update the settings and start tracking.
                                                           var customProfile = _markerTrackerSettings.TrackerProfile == MLMarkerTracker.Profile.Custom ? MLMarkerTracker.TrackerSettings.CustomProfile.Create(_markerTrackerSettings.FPSHint, _markerTrackerSettings.ResolutionHint, _markerTrackerSettings.CameraHint, _markerTrackerSettings.FullAnalysisIntervalHint, _markerTrackerSettings.CornerRefineMethod, _markerTrackerSettings.UseEdgeRefinement) : default;
                                                          var trackerSettings = MLMarkerTracker.TrackerSettings.Create(true, _markerTrackerSettings.MarkerTypes, _markerTrackerSettings.QRCodeSize, _markerTrackerSettings.ArucoDicitonary, _markerTrackerSettings.ArucoMarkerSize, _markerTrackerSettings.TrackerProfile, customProfile);
                                                           _startScanningTask = MLMarkerTracker.SetSettingsAsync(trackerSettings);
                                                       
                                                           ThreadDispatcher.RunOnMainThread(() =>
                                                                                            {
                                                                                                _scanning = true;
                                                                                                MLMarkerTracker.OnMLMarkerTrackerResultsFound += OnTrackerResultsFound;
                                                                                            });
                                                           
                                                    });
                }
                catch (Exception e)
                {
                    Debug.unityLogger.LogError("MARKER-TRACKING", $"Marker tracking failed to start. Error: {e.Message}");
                }
            }
            else
            {
                if (_scanning)
                {
                    Debug.unityLogger.LogWarning("MARKER-TRACKING", "Cannot start marker tracking. issue: [tracking already started].");
                }

                if (_startScanningTask != null && !_startScanningTask.IsCompleted)
                {
                    Debug.unityLogger.LogWarning("MARKER-TRACKING", $"Cannot start marker tracking. issue: [tracking is starting]. previous start task not complete.");
                }
            }
#endif
        }

        /// <summary>
        /// Stops marker tracking on the MagicLeap
        /// </summary>
        public override void StopTracking()
        {

            if (!SupportedOnPlatform)
            {
                return;
            }
            if (_scanning && (_stopScanningTask == null || _stopScanningTask.IsCompleted))
            {
                try
                {
                    _scanning = false;
                    MLMarkerTracker.OnMLMarkerTrackerResultsFound -= OnTrackerResultsFound;
                    ThreadDispatcher.RunOnWorkerThread(() =>
                                                     {

                                                         _stopScanningTask = MLMarkerTracker.StopScanningAsync();
                                                         _stopScanningTask.GetAwaiter().GetResult();
                                                         ThreadDispatcher.RunOnMainThread(RemoveAllTrackedMarkers);
                                                     });
  
                  
                }
                catch (Exception e)
                {
                    Debug.unityLogger.LogError("MARKER-TRACKING", $"Failed to stop marker tracking. Error: {e.Message}");
                }

                //Delayed to insure that the method is called after the last marker update. Otherwise the marker could be re-added directly after removal
              //  Invoke(nameof(RemoveAllTrackedMarkers), .1f);
            }

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

                _countDownTime = _maxLostTime;
                _updatedMarkerIds.Clear();
                _lastTrackedTimeByMarkerId.Clear();
                _lostMarkerIds.Clear();
            }
        }

        private void Update()
        {
            if (!SupportedOnPlatform)
            {
                return;
            }

            if (_countDownTime > 0)
            {
                _countDownTime -= Time.deltaTime;
            }
        }

        private void TrackLostMarkers()
        {
            if (!SupportedOnPlatform)
            {
                return;
            }

            _lostMarkerIds.Clear();
            for (var i = 0; i < _trackedMarkerIds.Count; i++)
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

            for (var i = 0; i < _lostMarkerIds.Count; i++)
            {
                _lastTrackedTimeByMarkerId.Remove(_lostMarkerIds[i]);
            }

            _updatedMarkerIds.Clear();
        }

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
            return null;
        }


        private void OnTrackerResultsFound(MLMarkerTracker.MarkerData data)
        {
            if (_scanning==false || !SupportedOnPlatform || !_trackedMarkerIds.Contains((int)data.ArucoData.Id))
            {
                return;
            }

            var id = (int)data.ArucoData.Id;


            var trackerActions = _arucoActionsByMarkerId[id];
            if (!_updatedMarkerIds.Contains(id))
            {
                _updatedMarkerIds.Add(id);
            }

          

            //Bug in SDK <=1.5 that reports April Tag pose with a 180 x axis rotation (will be fixed in future release)
            if (data.ArucoData.Dictionary.ToString().Contains("APRILTAG"))
            {
                trackerActions.Updated?.Invoke(new MarkerPose(data.ArucoData.Id, data.Pose.position, data.Pose.rotation * Quaternion.AngleAxis(180, Vector3.forward)));
            }
            else
            {
                trackerActions.Updated?.Invoke(new MarkerPose(data.ArucoData.Id, data.Pose.position, data.Pose.rotation));
            }
         
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

    #region Permission Callbacks

        private void RequestPermissions()
        {
            _permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
            _permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
            _permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
            MLPermissions.RequestPermission(MLPermission.MarkerTracking, _permissionCallbacks);
        }

        private void UnregisterPermissionEvents()
        {
            _permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
            _permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
            _permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;
        }
        
        private void OnPermissionGranted(string permission)
        {
            if (permission == MLPermission.MarkerTracking)
            {
                if (_trackOnStart)
                {
                    StartTracking();
                }
                _permissionGranted = true;
            }

            UnregisterPermissionEvents();
        }

        private void OnPermissionDenied(string permission)
        {
            if (permission == MLPermission.MarkerTracking)
            {
                Debug.unityLogger.LogError("MARKER-TRACKING", "Error: {permission} Permission Denied.");
            }

            UnregisterPermissionEvents();
        }

    #endregion
    }
}

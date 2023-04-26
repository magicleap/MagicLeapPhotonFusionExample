using System;
using System.Collections;
using System.Collections.Generic;
using MagicLeap.Utils;
using UnityEngine;

namespace MagicLeapNetworkingDemo.MarkerTracking{

/// <summary>
/// GameObject that responds to a marker events to represent the parker in the virtual space.
/// </summary>
/// <remarks>
/// The events of the marker are controlled by the current marker tracker. <see cref="MagicLeapMarkerTracker"/> and <see cref="GenericAprilTagTracker"/>
/// </remarks>
public class VirtualFiducialMarker : MonoBehaviour
{
    /// <summary>
    /// The marker ID to follow. The ID has to be added to the Marker Tracker <see cref="MagicLeapMarkerTracker"/> and <see cref="GenericAprilTagTracker"/>.
    /// </summary>
    [Tooltip("The marker ID to follow. The ID has to be added to the Marker Tracker.")]
    public int ID;

    /// <summary>
    /// Called everytime the marker updates its position.
    /// This is used by <see cref="MarkerTrackingControls"/> to track calibration progress.
    /// </summary>
    [Tooltip("Called everytime the marker updates its position.")]
    public Action MarkerUpdated;
    /// <summary>
    /// The amount of points that are used to optimized the pose. (averages index 0 -> x).
    /// </summary>
    [Tooltip("The amount of points that are used to optimized the pose. (averages index 0 -> x). ")]
    [SerializeField, Range(1,50)] private int _averagingSize = 1;

    /// <summary>
    /// The position threshold for low pass filter.
    /// </summary>
    [Tooltip("The position threshold for low pass filter.")]
    [SerializeField] private float _positionLowPass = 0.005f;
  
    /// <summary>
    /// The rotation threshold for low pass filter.
    /// </summary>
    [Tooltip("The rotation threshold for low pass filter.")]
    [SerializeField] private float _rotationLowPass = 2f;
    
    [SerializeField] private GameObject _optionalGraphic;
    [SerializeField] private Vector3 _rotationOffset = new Vector3(0,180,0);

    [Header("Debug Settings")]
    [Tooltip("render cube even when tracking is lost.")]
    [SerializeField] private bool _keepVisualOn;

    [Tooltip("Draw debug cube.")]
    [SerializeField] private bool _debugDraw;

    [Tooltip("Size of debug graphic.")]
    [SerializeField] float _debugWireCubeSize = 0.05f;

    [Tooltip("The position low pass.")]
    [SerializeField] private Material _tagMaterial = null;
    private TagDrawer _drawer;
    private BaseMarkerTrackerBehaviour _markerTracker;
    private MarkerTrackerActions _markerActions;
    private List<Vector3> _averagePosition = new List<Vector3>();
    private List<Quaternion> _averageRotation = new List<Quaternion>();
    private Vector3 _previousPosition;
    private Quaternion _previousRotation;
    private bool _isActive;

    private void Awake()
    {
   
    }

    private IEnumerator Start()
    {
        if (_tagMaterial == null)
        {
            _tagMaterial = new Material(Shader.Find("Unlit/Color"));
            _tagMaterial.color = Color.yellow;
        }

        yield return null;
        if (_optionalGraphic)
        {
            _optionalGraphic.SetActive(false);
        }
        if (_markerActions == null)
        {
            if (BaseMarkerTrackerBehaviour.Instance != null)
            {
                _markerTracker = BaseMarkerTrackerBehaviour.Instance;
                _markerActions = _markerTracker.GetActionsForId(ID);
                _markerActions.Updated += OnUpdated;
                _markerActions.Added += OnAdded;
                _markerActions.Removed += OnRemoved;
            }
        }
    }
    
    

    private void OnRemoved()
    {
        _isActive = false;
        if(_optionalGraphic)
        {
            _optionalGraphic.SetActive(false);
        }
    }

    private void OnAdded()
    {
        _isActive = true;
        if (_optionalGraphic)
        {
            _optionalGraphic.SetActive(true);
        }
    }

    private void OnEnable()
    {
        if (_debugDraw)
        {
            _drawer = new TagDrawer(_tagMaterial);
        }
        if (_markerActions != null)
        {
            _markerActions.Updated += OnUpdated;
            _markerActions.Added += OnAdded;
            _markerActions.Removed += OnRemoved;
        }
    }

    private void OnDisable()
    {
        _drawer?.Dispose();
        _averagePosition.Clear();
        _averageRotation.Clear();
        if(_markerActions!= null)
        {
            _markerActions.Updated -= OnUpdated;
            _markerActions.Added -= OnAdded;
            _markerActions.Removed -= OnRemoved;
        }
    }

    private void OnUpdated(MarkerPose data)
    {
        transform.position = data.Position;
        transform.rotation = data.Rotation;
        UpdateTracking(data);
    }

    private void Update()
    {
        if(_isActive==false && !_keepVisualOn)
        {
            return;
        }

        if (_debugDraw)
        {
            _optionalGraphic.SetActive(true);
            _drawer?.Draw(ID, transform.position, transform.rotation, _debugWireCubeSize);
        }
    }


    private void UpdateTracking(MarkerPose data)
    {
        if (_optionalGraphic)
        {
            _optionalGraphic.SetActive(true);
        }

        var newPosition = data.Position;
        var newRotation = data.Rotation * Quaternion.Euler(_rotationOffset);
        if (_averagePosition.Count > 0)
        {
            MathUtils.LowpassFilter(_previousPosition, _previousRotation,ref newPosition, ref newRotation, _positionLowPass, _rotationLowPass);
        }

      
    
        AddTrackingResult(newPosition, newRotation);
        _previousPosition = newPosition;
        _previousRotation = newRotation;
    }
    void AddTrackingResult(Vector3 pos, Quaternion rot)
    {
        _averagePosition.Add(pos);
        _averageRotation.Add(rot);

        if(_averagePosition.Count > _averagingSize)
        {
            _averagePosition.RemoveAt(0);
            _averageRotation.RemoveAt(0);
        }
      

        var newPos = new Vector3(0,0,0);
        var newRot = Quaternion.identity;
    

        newRot = MathUtils.Average(_averageRotation[0], _averageRotation.ToArray());
        newPos = MathUtils.Average(_averagePosition.ToArray());

    
        transform.position = newPos;
        transform.rotation = newRot;
        MarkerUpdated?.Invoke();
 
    }

}
}
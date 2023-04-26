using System.Collections.Generic;
using Fusion;
using MagicLeapNetworkingDemo;
using MagicLeapNetworkingDemo.Input;
using UnityEngine;
using UnityEngine.Serialization;

namespace MagicLeap.Networking
{

    /// <summary>
    /// Modified version of script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Represents the player across the network. Consumes data from <see cref="HardwareRig"/> and replays relative to the shared reference point.
    /// </para>
    /// </remarks>
    public class NetworkRig : NetworkBehaviour
    {
     
        public HardwareRig HardwareRig;
        public NetworkGameController Controller;
        public NetworkHeadset Headset;


        [Networked]
        private Vector3 _controllerPosition { get; set; }
        [Networked]
        private Quaternion _controllerRotation { get; set; }
        [Networked]
        private Vector3 _headsetPosition { get; set; }
        [Networked]
        private Quaternion _headsetRotation { get; set; }
        [Networked]
        private Vector3 _rigPosition { get; set; }
        [Networked]
        private Quaternion _rigRotation { get; set; }

        private Interpolator<Vector3> _controllerPositionInterpolator;
        private Interpolator<Quaternion> _controllerRotationInterpolator;
        private Interpolator<Vector3> _headsetPositionInterpolator;
        private Interpolator<Quaternion> _headsetRotationInterpolator;
        private Interpolator<Quaternion> _rigRotationInterpolator;
        private Interpolator<Vector3> _rigPositionInterpolator;
        private SharedReferencePoint _referencePoint;
    

        private void Start()
        {
            _referencePoint = SharedReferencePoint.Instance;
            
        }
        //StateAuthority means we can control the object.
        public virtual bool HasControl => Object.HasStateAuthority;

        public override void Spawned()
        {
            base.Spawned();
            _referencePoint = SharedReferencePoint.Instance;
            if (HasControl)
            {
                HardwareRig = FindObjectOfType<HardwareRig>();
                if (HardwareRig == null)
                {
                    Debug.LogError("Missing HardwareRig in the scene");
                }
            }

            transform.SetParent(_referencePoint.transform);
            _controllerPositionInterpolator = GetInterpolator<Vector3>(nameof(_controllerPosition));
            _controllerRotationInterpolator = GetInterpolator<Quaternion>(nameof(_controllerRotation));
            _headsetPositionInterpolator = GetInterpolator<Vector3>(nameof(_headsetPosition));
            _headsetRotationInterpolator = GetInterpolator<Quaternion>(nameof(_headsetRotation));
            _rigPositionInterpolator = GetInterpolator<Vector3>(nameof(_rigPosition));
            _rigRotationInterpolator = GetInterpolator<Quaternion>(nameof(_rigRotation));
        }

        public override void FixedUpdateNetwork()
        {


            // update the rig at each network tick (only true on local user)
            if (GetInput<RigInput>(out var input))
            {
                ApplyInputToRigParts(input);
                Controller.controllerInputData = input.ControllerInput;

            }
        }

        private void ApplyRelativeWorldPosition(Vector3 rigPosition, Quaternion rigRotation, 
                                                Vector3 controllerPosition, Quaternion controllerRotation, 
                                                Vector3 headsetPosition, Quaternion headsetRotation)
        {
            transform.position = _referencePoint.transform.TransformPoint(rigPosition);
            transform.rotation = _referencePoint.transform.rotation * rigRotation;
            Controller.transform.position = _referencePoint.transform.TransformPoint(controllerPosition);
            Controller.transform.rotation = _referencePoint.transform.rotation * controllerRotation;
            Headset.transform.position = _referencePoint.transform.TransformPoint(headsetPosition);
            Headset.transform.rotation = _referencePoint.transform.rotation * headsetRotation;
        }
        protected virtual void ApplyInputToRigParts(RigInput input)
        {
          
                
            _rigPosition = input.CenterPosition;
            _rigRotation = input.CenterRotation;
            _controllerPosition = input.ControllerPosition;
           _controllerRotation = input.ControllerRotation;
            _headsetPosition = input.headsetPosition;
            _headsetRotation = input.headsetRotation;
            ApplyRelativeWorldPosition(_rigPosition,_rigRotation,
                                _controllerPosition,_controllerRotation,
                                _headsetPosition,_headsetRotation);



        }
   

        public override void Render()
        {
           //If this is a remote user, use the interpolated values.
            if (!HasControl)
            {
          
                ApplyRelativeWorldPosition(_rigPositionInterpolator.Value, _rigRotationInterpolator.Value,
                                           _controllerPositionInterpolator.Value, _controllerRotationInterpolator.Value,
                                           _headsetPositionInterpolator.Value, _headsetRotationInterpolator.Value);
            }
        }
    }
}

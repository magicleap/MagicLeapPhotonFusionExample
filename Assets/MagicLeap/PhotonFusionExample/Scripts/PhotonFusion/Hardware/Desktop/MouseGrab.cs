using MagicLeap.Networking.Interactions;
using MagicLeapNetworkingDemo.Grabbing;
using UnityEngine.InputSystem;
using UnityEngine;
namespace MagicLeapNetworkingDemo.Desktop
{    
    public interface IMouseTeleportHover
    {
        void OnHoverHit(RaycastHit hit);
        void OnNoHover();
    }

    /// <summary>
    /// Modified version of script "MouseTeleport" from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Allow to have partial interaction capability by controlling the XR controller with the mouse.
    /// </para>
    /// <para>
    ///  Changes:
    /// <list type="bullet">
    ///     <item>
    ///         <description> Remove teleport.</description>
    ///     </item>
    ///     <item>
    ///         <description> Renamed variables to fit only grabbing functionality.</description>
    ///     </item>
    ///     </list>
    ///     </para>
    /// </remarks>
    public class MouseGrab : MonoBehaviour
    {
        [SerializeField] private HardwareRig _rig;
         [SerializeField] private Camera _mouseCamera;

        private HardwareGameController _grabberHand;
        private GameObject _grabbed = null;
        private IMouseTeleportHover _hoverListener;
        private float _grabHandDistance = 0;
        private Transform _head => _rig == null ? null : _rig.headset.transform;
        private Vector3 _defaultGameControllerPosition;
        private Quaternion _defaultGameControllerRotation;
        protected virtual void Awake()
        {


            _defaultGameControllerPosition = _head.InverseTransformPoint(_rig.gameController.transform.position);
            _defaultGameControllerRotation = Quaternion.Inverse(_head.rotation) * _rig.gameController.transform.rotation;

        }

        void Start()
        {
            // grab and teleport are done with left hand
            _grabberHand = _rig.gameController;
       
        }



        bool CheckGrab(Ray mouseRay)
        {
            bool didTouch = false;

            // No teleport/click when right click is pressed. Only rotation
            if (!Mouse.current.rightButton.isPressed && Mouse.current.leftButton.isPressed)
            {
                if (Physics.Raycast(mouseRay, out RaycastHit hit, 40f))
                {
                    // check if there is already a grabbed object
                    if (_grabbed == null)
                    {
                        // check if the hit object can be grab
                        GameObject grabbableObject = null;
                        var grabbable = hit.collider.GetComponentInParent<Grabbable>();
                        if (grabbable)
                        {
                            grabbableObject = grabbable.gameObject;
                        }
                        else
                        {
                            var networkGrabbable = hit.collider.GetComponentInParent<NetworkColliderGrabbable>();
                            if (networkGrabbable)
                            {
                                grabbableObject = networkGrabbable.gameObject;
                            }
                        }
                        if (grabbableObject != null)
                        {
                            // the ray hit a grabbable object
                            didTouch = true;
                            _grabbed = grabbableObject;

                           

                            // We move the local hand to the hit position, and active isGrabbing
                            _grabberHand.transform.position = hit.point;
                            _grabberHand.IsGrabbing = true;
                            _grabHandDistance = (hit.point - mouseRay.origin).magnitude;


                            // TO DO Update the position of the grabbed object
                        }
                    }
                }
            }
            return didTouch;
        }

        void CheckUngrab()
        {
            // Check if we should ungrab
            if (!Mouse.current.leftButton.wasReleasedThisFrame && !Mouse.current.leftButton.isPressed)
            {
                if (_grabbed != null)
                {
                    _grabbed = null;
                    _grabberHand.IsGrabbing = false;
                }
            }
        }

        public bool BeamCast(out RaycastHit hitInfo, Vector3 origin, Vector3 direction)
        {
            Ray handRay = new Ray(origin, direction);
            return Physics.Raycast(handRay, out hitInfo);
        }
        Vector3 SearchTarget(Ray mouseRay)
        {
            var target = mouseRay.origin + mouseRay.direction * 20;
            if (BeamCast(out RaycastHit hit, mouseRay.origin, mouseRay.direction))
            {
                target = hit.point;
                if (_hoverListener != null) _hoverListener.OnHoverHit(hit);
            }
            else
            {
                if (_hoverListener != null) _hoverListener.OnNoHover();
            }
            return target;
        }

        protected virtual void Update()
        {
        
            if (_grabbed == null) _grabHandDistance = 0;
            var mouseRay = _mouseCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // Check if the mouse hit a grabbable object
            bool didTouch = CheckGrab(mouseRay);
            bool beamerRotationHandled = false;
            bool grabberHandPositionHandled = didTouch;
            CheckUngrab();

            Vector3 target = Vector3.zero;
            bool targetSearched = false;
            if (_hoverListener != null)
            {
                // If a hover listener expect to know if a hover occured, we have to check whatever buttons are clicked
                target = SearchTarget(mouseRay);
                targetSearched = true;
            }

            if(!didTouch && _grabbed == null && Mouse.current.rightButton.isPressed == false)
            {
                if (Mouse.current.leftButton.isPressed || Mouse.current.leftButton.wasReleasedThisFrame)
                {
                   
                    if(targetSearched == false)
                    {
                        target = SearchTarget(mouseRay);
                    }

                    beamerRotationHandled = true;
                    var currentBeamLocalRotation = _rig.gameController.transform.rotation;
                    var beamRotation = Quaternion.LookRotation(target - _rig.gameController.transform.position);
                    // Explanation:
                    // We have: beamerHand.transform.rotation * currentBeamLocalRotation = rayBeamer.origin.rotation
                    // We want: rayBeamer.origin.rotation = beamRotation;
                    // So beamerHand.transform.rotation * currentBeamLocalRotation = beamRotation
                    // So beamerHand.transform.rotation * currentBeamLocalRotation * Quaternion.Inverse(currentBeamLocalRotation) = beamRotation * Quaternion.Inverse(currentBeamLocalRotation)
                    // So beamerHand.transform.rotation * Quaternion.Identity = beamRotation * Quaternion.Inverse(currentBeamLocalRotation). Simplified to:
                    _rig.gameController.transform.rotation = beamRotation * Quaternion.Inverse(currentBeamLocalRotation);
                }
            }

        
            _rig.gameController.transform.rotation = _head.rotation * _defaultGameControllerRotation;

            if (_grabbed != null)
            {
                grabberHandPositionHandled = true;
                _rig.gameController.transform.position = mouseRay.origin + mouseRay.direction * _grabHandDistance;
            }

            if (!grabberHandPositionHandled)
            {
                var leftHandPosition = _head.TransformPoint(_defaultGameControllerPosition) + mouseRay.direction * handRange;
                if ((leftHandPosition - _rig.gameController.transform.position).sqrMagnitude < 1f)
                {
                    leftHandPosition = Vector3.Lerp(_rig.gameController.transform.position, leftHandPosition, 30f * Time.deltaTime);
                }
                _rig.gameController.transform.position = leftHandPosition;
            }
            if (!beamerRotationHandled)
            {
                _rig.gameController.transform.LookAt(mouseRay.origin + mouseRay.direction * 100f);
                _rig.gameController.transform.Rotate(-40, 0, 0);
            }
        }
        float handRange = 0.7f;
    }

}
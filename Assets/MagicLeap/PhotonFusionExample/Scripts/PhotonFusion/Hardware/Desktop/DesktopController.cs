
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MagicLeapNetworkingDemo.Desktop
{


    /// <summary>
    /// Modified version of script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Desktop hardware movements controller, using keyboard to move. Moves <see cref="HardwareRig"/>
    /// </para>
    /// </remarks>
    public class DesktopController : MonoBehaviour
    {
        [SerializeField] private InputActionProperty _forwardAction;
        [SerializeField] private InputActionProperty _rotationAction;
        [SerializeField] private HardwareRig _rig;

        [SerializeField] private float _strafeSpeed = 3;
        [SerializeField] private float _forwardSpeed = 3;
        [SerializeField] private float _rotationSpeed = 180;

        void Start()
        {
            if (_forwardAction != null && _forwardAction.action != null)
            {
          
                if (_forwardAction.reference == null && _forwardAction.action.bindings.Count == 0)
                {

                    _forwardAction.action.AddCompositeBinding("2DVector")
                               .With("Up", "<Keyboard>/w")
                               .With("Down", "<Keyboard>/s")
                               .With("Left", "<Keyboard>/a")
                               .With("Right", "<Keyboard>/d");
                }

                _forwardAction.action.Enable();
            }
            if (_rotationAction != null && _rotationAction.action != null)
            {
                if (_rotationAction.reference == null && _rotationAction.action.bindings.Count == 0)
                {
                  
                    _rotationAction.action.AddCompositeBinding("Axis")
                        .With("Positive", "<Keyboard>/e")
                        .With("Negative", "<Keyboard>/q");
                }
                _rotationAction.action.Enable();
            }
         
        }

        void Update()
        {
        
            if (_rotationAction != null && _rotationAction.action != null)
            {
                _rig.Rotate(_rotationAction.action.ReadValue<float>() * Time.deltaTime * _rotationSpeed);
            }

            if (_forwardAction != null && _forwardAction.action != null)
            {
                var command = _forwardAction.action.ReadValue<Vector2>();
                if (command.magnitude == 0) return;
                var headsetMove = command.y * _forwardSpeed * Time.deltaTime * _rig.headset.transform.forward + command.x * _strafeSpeed * Time.deltaTime * _rig.headset.transform.right;
                var move = new Vector3(headsetMove.x, 0, headsetMove.z);
                var newPosition = _rig.transform.position + move;

                Move(newPosition);
            }
        }

        public virtual void Move(Vector3 newPosition)
        {
            _rig.Teleport(newPosition);
        }
    }
}

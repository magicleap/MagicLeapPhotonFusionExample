
using System.Collections;
using System.Collections.Generic;
using MagicLeapNetworkingDemo;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MagicLeapNetworkingDemo.Desktop
{
    /// <summary>
    /// Modified version of script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Allow movement of the camera using mouse input.
    /// </para>
    /// </remarks>
    public class MouseCamera : MonoBehaviour
    {
        public InputActionProperty mouseXAction;
        public InputActionProperty mouseYAction;

        [SerializeField] private HardwareRig _rig;
        [Header("Mouse point of view")]
        [SerializeField] private Vector2 _maxMouseInput = new Vector2(10, 10);

        [SerializeField] private float _maxHeadRotationSpeed = 30;
        [SerializeField] private Vector2 _sensitivity = new Vector2(10, 10);
        [SerializeField] private float _maxHeadAngle = 65;
        [SerializeField] private float _minHeadAngle = 65;
        
        private Vector3 rotation = Vector3.zero;
        private Vector2 mouseInput;

        private Transform _head => _rig == null ? null : _rig.headset.transform;

        
        private void Awake()
        {
            if (mouseXAction.action.bindings.Count == 0) mouseXAction.action.AddBinding("<Mouse>/delta/x");
            if (mouseYAction.action.bindings.Count == 0) mouseYAction.action.AddBinding("<Mouse>/delta/y");

            mouseXAction.action.Enable();
            mouseYAction.action.Enable();

            if (_rig == null) _rig = GetComponentInParent<HardwareRig>();
        }

        private void Start()
        {
           var mouseCamera = GetComponentInChildren<Camera>();
            mouseCamera.transform.position = _head.position;
            mouseCamera.transform.rotation = _head.rotation;
        }

        private void Update()
        {
            if (Mouse.current.rightButton.isPressed)
            {
                mouseInput.x = mouseXAction.action.ReadValue<float>() * Time.deltaTime * _sensitivity.x;
                mouseInput.y = mouseYAction.action.ReadValue<float>() * Time.deltaTime * _sensitivity.y;

                mouseInput.y = Mathf.Clamp(mouseInput.y, -_maxMouseInput.y, _maxMouseInput.y);
                mouseInput.x = Mathf.Clamp(mouseInput.x, -_maxMouseInput.x, _maxMouseInput.x);

                rotation.x = _head.eulerAngles.x - mouseInput.y;
                rotation.y = _head.eulerAngles.y + mouseInput.x;

                if (rotation.x > _maxHeadAngle && rotation.x < (360 - _minHeadAngle))
                {
                    if (Mathf.Abs(_maxHeadAngle - rotation.x) < Mathf.Abs(rotation.x - (360 - _minHeadAngle)))
                    {
                        rotation.x = _maxHeadAngle;
                    }
                    else
                    {
                        rotation.x = -_minHeadAngle;
                    }
                }
                else if (rotation.x < -_minHeadAngle)
                {
                    rotation.x = -_minHeadAngle;
                }

                var newRot = Quaternion.Lerp(_head.rotation, Quaternion.Euler(rotation), _maxHeadRotationSpeed * Time.deltaTime);

                _head.rotation = newRot;
            }
        }
    }
}

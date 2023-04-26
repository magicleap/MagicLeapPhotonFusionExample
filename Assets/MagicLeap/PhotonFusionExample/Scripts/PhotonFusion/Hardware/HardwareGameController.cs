using System.Collections.Generic;
using MagicLeapNetworkingDemo.Extensions;
using MagicLeapNetworkingDemo.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace MagicLeapNetworkingDemo
{
    /// <summary>
    /// Modified version of script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Script takes input from the Magic Leap controller and detects if the user is pressing the bumper
    /// </para>
    /// </remarks>
	public class HardwareGameController: MonoBehaviour
    {
      
        public ControllerInputData ControllerInput;
        public bool IsGrabbing = false;

        [Header("Hand pose input")]

        public InputActionProperty gripAction;
        public InputActionProperty triggerAction;


        public float grabThreshold = 0.5f;
        
        /// <summary>
        /// Should grab be toggled by the input action? This is set to false when using <see cref="MagicLeapNetworkingDemo.Desktop.MouseGrab"/> since it controls the grab value
        /// </summary>
        public bool updateGrabWithAction = true;

        private void Awake()
        {
           
            gripAction.EnableWithDefaultXRBindings( new List<string> { "grip" });
            triggerAction.EnableWithDefaultXRBindings(new List<string> { "trigger" });
        }

        protected virtual void Update()
        {
            // update hand pose
   
            ControllerInput.gripCommand = gripAction.action.ReadValue<float>();
            ControllerInput.triggerCommand = triggerAction.action.ReadValue<float>();
        

            // update hand interaction
            if(updateGrabWithAction) IsGrabbing = ControllerInput.gripCommand > grabThreshold;
        }

        #region Haptic feedback (vibrations)
        private UnityEngine.XR.InputDevice? _device = null;
        private bool supportImpulse = false;

        // Find the device associated to a VR controller, to be able to send it haptic feedback (vibrations)
        public UnityEngine.XR.InputDevice? Device
        {
            get
            {
                if (_device == null)
                {
              
                    InputDeviceCharacteristics trackedControllerFilter = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.TrackedDevice ;

                    List<UnityEngine.XR.InputDevice> foundControllers = new List<UnityEngine.XR.InputDevice>();
                    InputDevices.GetDevicesWithCharacteristics(trackedControllerFilter, foundControllers);

                    if (foundControllers.Count > 0)
                    {
                        var inputDevice = foundControllers[0];
                        _device = inputDevice;
                        if (inputDevice.TryGetHapticCapabilities(out var hapticCapabilities))
                        {
                            // We memorize if this device can support vibrations
                            supportImpulse = hapticCapabilities.supportsImpulse;
                        }
                    }
                }
                return _device;
            }
        }

        // If a device supporting haptic feedback has been detected, send a vibration to it (here in the form of an impulse)
        public void SendHapticImpulse(float amplitude = 0.3f, float duration = 0.3f, uint channel = 0)
        {
            if (Device != null)
            {
                var inputDevice = Device.GetValueOrDefault();
                if (supportImpulse)
                {
                    inputDevice.SendHapticImpulse(channel, amplitude, duration);
                }
            }
        }

        // If a device supporting haptic feedback has been detected, send a vibration to it (here in the form of a buffer describing the vibration data)
        public void SendHapticBuffer(byte[] buffer, uint channel = 0)
        {
            if (Device != null)
            {
                var inputDevice = Device.GetValueOrDefault();
                if (supportImpulse)
                {
                    inputDevice.SendHapticBuffer(channel, buffer);
                }
            }
        }
        #endregion

    }
}

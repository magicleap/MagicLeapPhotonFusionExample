using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using MagicLeap.Networking;
using MagicLeapNetworkingDemo.Input;
using UnityEngine;
using UnityEngine.Events;

namespace MagicLeapNetworkingDemo
{


    /// <summary>
    /// Modified version of script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Hardware rig gives access to the various rig parts: head, controller, and xr origin, represented by the hardware rig itself
    /// </para>
    /// </remarks>
    public class HardwareRig : MonoBehaviour, INetworkRunnerCallbacks
    {
        public HardwareGameController gameController;
        public HardwareHeadset headset;

        public NetworkRunner runner;
        public SharedReferencePoint SharedReferencePoint;


        protected virtual void Start()
        {
            SharedReferencePoint = SharedReferencePoint.Instance;
            if(runner == null)
            {
                Debug.LogWarning("Runner has to be set in the inspector to forward the input");
            }
            if(runner) runner.AddCallbacks(this);
        }

        #region Locomotion
        // Update the hardware rig rotation. This will trigger a Riginput network update
        public virtual void Rotate(float angle)
        {
            transform.RotateAround(headset.transform.position, transform.up, angle);
        }

        // Update the hardware rig position. This will trigger a Riginput network update
        public virtual void Teleport(Vector3 position)
        {
            Vector3 headsetOffet = headset.transform.position - transform.position;
            headsetOffet.y = 0;
            transform.position = position - headsetOffet;
          
        }



        #endregion

        #region INetworkRunnerCallbacks

        // Prepare the input, that will be read by NetworkRig in the FixedUpdateNetwork
        public virtual void OnInput(NetworkRunner runner, NetworkInput input) {
            RigInput rigInput = PrepareRigInput();
            input.Set(rigInput);
        }

        protected virtual RigInput PrepareRigInput()
        {
 
            RigInput rigInput = new RigInput();
            if (SharedReferencePoint == null)
            {
                SharedReferencePoint = SharedReferencePoint.Instance;
                return rigInput;
            }

            rigInput.CenterPosition = SharedReferencePoint.transform.InverseTransformPoint(transform.position);
            rigInput.CenterRotation = Quaternion.Inverse(SharedReferencePoint.transform.rotation) * transform.rotation;
            rigInput.ControllerPosition = SharedReferencePoint.transform.InverseTransformPoint(gameController.transform.position);
            rigInput.ControllerRotation = Quaternion.Inverse(SharedReferencePoint.transform.rotation) * gameController.transform.rotation;
            rigInput.headsetPosition = SharedReferencePoint.transform.InverseTransformPoint(headset.transform.position);
            rigInput.headsetRotation = Quaternion.Inverse(SharedReferencePoint.transform.rotation) * headset.transform.rotation;
    
            return rigInput;
        }

        #endregion

        #region INetworkRunnerCallbacks (unused)
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }


        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnConnectedToServer(NetworkRunner runner) { }

        public void OnDisconnectedFromServer(NetworkRunner runner) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }
        #endregion
    }
}

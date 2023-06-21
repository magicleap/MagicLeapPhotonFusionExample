using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MagicLeap.Networking
{
    /// <summary>
    /// Modified version of script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Networking script that handles the following:
    /// <list type="bullet">
    ///     <item>
    ///         <description> Connecting to the network. </description>
    ///     </item>
    ///     <item>
    ///         <description> Spawning a networked prefab when the local user is connected.</description>
    ///     </item>
    ///     </list>
    ///     </para>
    /// </remarks>
    public class ConnectionManager : MonoBehaviour, INetworkRunnerCallbacks
    {

        public string roomName = "SampleFusion-MagicLeap2";
        public bool connectOnStart;
        public Action<string> ConnectionFailed;
        public Action DisconnectedFromServer;
        public Action ConnectedToServer;
        [Header("Fusion settings")]
        [Tooltip("Fusion runner. Automatically created if not set")]
        public NetworkRunner runner;

        public INetworkSceneManager sceneManager;

        [Header("Local user spawner")]
        public NetworkObject userPrefab;


        [Header("Event")]
        public UnityEvent onWillConnect = new();

        private void Awake()
        {
            // Check if a runner exist on the same game object
            if (runner == null)
            {
                runner = GetComponent<NetworkRunner>();
            }

            // Create the Fusion runner and let it know that we will be providing user input
            if (runner == null)
            {
                runner = gameObject.AddComponent<NetworkRunner>();
            }

            runner.ProvideInput = true;
        }

        private async void Start()
        {
            // Launch the connection at start
            if (connectOnStart)
            {
                await Connect();
            }
        }

        public async Task Connect()
        {
            // Create the scene manager if it does not exist
            if (sceneManager == null)
            {
                sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
            }

            if (onWillConnect != null)
            {
                onWillConnect.Invoke();
            }

           
            var args = new StartGameArgs
                       {
                           GameMode = GameMode.Shared, //this demo only handles input for shared game mode
                           SessionName = roomName,
                           Scene = SceneManager.GetActiveScene().buildIndex,
                           SceneManager = sceneManager
                       };
            await runner.StartGame(args);
        }


    #region INetworkRunnerCallbacks

        public void OnPlayerJoined(NetworkRunner playerRunner, PlayerRef player)
        {
            if (player == playerRunner.LocalPlayer)
            {
                Debug.Log($"OnPlayerJoined: {playerRunner.UserId}");
                // Spawn the user prefab for the local user
                var networkPlayerObject = playerRunner.Spawn(userPrefab, transform.position, transform.rotation, player, (runner, obj) => { });
            }
        }

    #endregion


    #region Unused INetworkRunnerCallbacks

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            ConnectedToServer?.Invoke();
        }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            DisconnectedFromServer?.Invoke();
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            ConnectionFailed?.Invoke($"Connection Failed: {reason}");
        }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
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

using MagicLeap.Networking;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeapNetworkingDemo.Shared
{
    /// <summary>
    /// Modified version of script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// Script to display an overlay UI to select desktop or VR mode, and active the associated rig, alongside the connexion component
    /// </remarks>
    public class RigSelection : MonoBehaviour
    {
        [SerializeField] private ConnectionManager _connectionManager;
        [SerializeField] private HardwareRig _magicLeapRig;
        [SerializeField] private HardwareRig _desktopRig;


        private void Awake()
        {
            _connectionManager.gameObject.SetActive(false);
            _magicLeapRig.gameObject.SetActive(false);
            _desktopRig.gameObject.SetActive(false);


            if (MagicLeapXrProvider.IsZIRunning || Application.platform == RuntimePlatform.Android)
            {
                EnableMagicLeap2Rig();
            }
            else
            {
                EnableDesktopRig();
            }
        }

        private void EnableMagicLeap2Rig()
        {
            gameObject.SetActive(false);
            _magicLeapRig.gameObject.SetActive(true);
            OnRigSelected();
        }

        private void EnableDesktopRig()
        {
            gameObject.SetActive(false);
            _desktopRig.gameObject.SetActive(true);
            OnRigSelected();
        }

        private void OnRigSelected()
        {
            _connectionManager.gameObject.SetActive(true);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using MagicLeap;
using MagicLeap.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

public class MapLocalizer : MonoBehaviour
{
    public enum MapMode
    {
        OnDevice=0,
        ARCloud=1,
    }

    [Tooltip("0 for OnDevice | 1 for ARCloud.")]
    public MapMode mapMode = MapMode.OnDevice;
    
    // Activity action: Launch the Mapping Tool activity to select a particular space
    // to be localized to.
    private string selectSpaceId = "com.magicleap.intent.action.SELECT_SPACE";


    // Intent extra: The mapping mode for the space provided by the SpaceID.
    // Pass 0 for OnDevice and 1 for ARCloud.
    private string extraMappingMode = "com.magicleap.intent.extra.MAPPING_MODE";
    // Start is called before the first frame update

    [SerializeField] private Canvas _notLocalizedCanvas;
    [SerializeField] private Canvas _permissionsNotGrantedCanvas;
    [SerializeField] private Button _permissionsExitAppButton;
    [SerializeField] private Button _exitAppButton;
    [SerializeField] private Button _openSpacesButton;
    [SerializeField] private TMP_Text _localizationText;
    private bool _appStarted;
    
    void Start()
    {
        _permissionsNotGrantedCanvas.enabled = false;
        InitializeUI();
        if (!Application.isEditor)
        {
            MLAnchorsAsync.Instance.GetPermissionResponse(PermissionsGranted);
        }
        _appStarted = true;

    }

    private void PermissionsGranted(bool granted)
    {
        if (!granted)
        {
            _notLocalizedCanvas.enabled = true;
            _permissionsNotGrantedCanvas.enabled = true;
        }
        else
        {
            StartCoroutine(UpdateLocalizationStatus());
        }
    }

    void InitializeUI()
    {
        _permissionsExitAppButton.onClick.AddListener(OnExitClicked);
        _exitAppButton.onClick.AddListener(OnExitClicked);
        _openSpacesButton.onClick.AddListener(OnOpenSpacesApp);
    }

    void OnExitClicked()
    {
        Application.Quit();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && _appStarted)
        {
            if (!Application.isEditor)
            {
                MLAnchorsAsync.Instance.GetPermissionResponse(PermissionsGranted);
            }
        }
    }

    IEnumerator UpdateLocalizationStatus()
    {
        MLAnchors.GetLocalizationInfo(out var localizationInfo);
        var localized = localizationInfo.LocalizationStatus == MLAnchors.LocalizationStatus.Localized;
        var validPose = localized && IsPoseValid(localizationInfo.SpaceOrigin);
        
        if (localizationInfo.LocalizationStatus != MLAnchors.LocalizationStatus.Localized)
        {
            _notLocalizedCanvas.enabled = true;
            _localizationText.text = "You are not localized. To co-locate using Spaces, please localize into a map.";
        }
        while (localized == false || validPose == false)
        {
            bool busy = true;

            ThreadDispatcher.RunAsync(() =>
                                      {

                                          MLAnchors.GetLocalizationInfo(out localizationInfo);
                                          busy = false;
                                      });
         

            while (busy)
            {
                yield return null;
            }
            localized = localizationInfo.LocalizationStatus == MLAnchors.LocalizationStatus.Localized;
            validPose = localized && IsPoseValid(localizationInfo.SpaceOrigin);
        }
        
        if (localizationInfo.MappingMode == MLAnchors.MappingMode.ARCloud && mapMode == MapMode.OnDevice)
        {
            _localizationText.text = "You are localized to a map on AR Cloud, but the app is using local maps to co-locate. Please change your Space.";
        }

        else if (localizationInfo.MappingMode != MLAnchors.MappingMode.ARCloud && mapMode == MapMode.ARCloud)
        {
            _localizationText.text = "You are localized to a local ma, but the app is using AR Cloud. Please change your Space.";
        }
        else
        {
           SharedReferencePoint.Instance.transform.SetPositionAndRotation(localizationInfo.SpaceOrigin.position, localizationInfo.SpaceOrigin.rotation);
        }
    }

    bool IsPoseValid(Pose anchorPose)
    {
        return !(anchorPose.rotation.x == 0
             && anchorPose.rotation.y == 0
             && anchorPose.rotation.z == 0
             && anchorPose.rotation.w == 0);
    }


    private void OnOpenSpacesApp()
    {
#if UNITY_MAGICLEAP || UNITY_ANDROID
        try
        {
            AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", selectSpaceId);
            intent.Call<AndroidJavaObject>("putExtra", extraMappingMode, ((int)mapMode).ToString());
            activity.Call("startActivityForResult", intent, 0);
        }
        catch (Exception e)
        {
            Debug.LogError("Error while launching spaces app: " + e);
        }
#endif
    }
}

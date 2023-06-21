using System.Collections;
using System.Collections.Generic;
using MagicLeap.Utilities;
using MagicLeapNetworkingDemo.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MagicLeapNetworkingDemo.MarkerTracking
{
	/// <summary>
	/// Controls toggling tracking on and off to calibrate the shared reference point
	/// </summary>
	/// <remarks>
	/// <para>
	/// The script maps the menu key on Magic Leap and M as the calibration button.
	/// </para>
	/// </remarks>
	public class MarkerTrackingControls : MonoBehaviour
	{
		[SerializeField] private TMP_Text _calibrationText;
		[SerializeField] private int _calibrationSteps=200;
		[SerializeField] private VirtualFiducialMarker virtualFiducialMarker;
		[SerializeField] private Image _instructionsPanel;
		[SerializeField] private Image _calibrationPanel;
		[SerializeField] private PlaceInFront _PlaceInFront;
		[SerializeField] private Button _lockPlacementButton;
		[SerializeField] private Button _followButton;
		[Header("Input Actions")]
		[SerializeField] private InputActionProperty _desktopCalibrationAction;
		[SerializeField] private InputActionProperty _magicLeapCalibrationAction;
		private int _currentCalibrationStep = 0;
		private bool _calibrationInProgress = false;
		private BaseMarkerTrackerBehaviour _markerTracker;

		private void OnValidate()
		{
			if(virtualFiducialMarker== null)
			virtualFiducialMarker = FindObjectOfType<VirtualFiducialMarker>();
		}

		void Start()
		{
			
			OnFollowClicked();
			StartCoroutine(FindMarker());
			_instructionsPanel.gameObject.SetActive(true);
			_calibrationPanel.gameObject.SetActive(false);
			
			_lockPlacementButton.onClick.AddListener(OnLockPlacementClicked);
			_followButton.onClick.AddListener(OnFollowClicked);
			
			SubscribeToInput();
		}

		IEnumerator FindMarker()
		{
			_markerTracker = BaseMarkerTrackerBehaviour.Instance;
			while (_markerTracker== null)
			{
				_markerTracker = BaseMarkerTrackerBehaviour.Instance;
				Debug.LogWarning("Searching for marker tracker...");
				yield return null;
			}

			Debug.LogWarning("<color=green>marker tracker found.</color>");
		}

		private void SubscribeToInput()
		{
			if (_desktopCalibrationAction.action != null)
			{
				if (_desktopCalibrationAction.reference == null && _desktopCalibrationAction.action.bindings.Count == 0)
				{

					_desktopCalibrationAction.action.AddBinding("<Keyboard>/m");
					_desktopCalibrationAction.action.performed += MenuKeyPressed;
					_desktopCalibrationAction.action.Enable();

				}

			}

			if (_magicLeapCalibrationAction.action != null)
			{
				if (_magicLeapCalibrationAction.reference == null && _magicLeapCalibrationAction.action.bindings.Count == 0)
				{
					_magicLeapCalibrationAction.EnableWithDefaultXRBindings(new List<string> { "menuButton" });
					_magicLeapCalibrationAction.action.performed += MenuKeyPressed;
					_magicLeapCalibrationAction.action.Enable();
				}

			}
		}
		

		private void OnDestroy()
		{
			_desktopCalibrationAction.action.Dispose();
			_magicLeapCalibrationAction.action.Dispose();
		}

		private void OnFollowClicked()
		{
			_PlaceInFront.PlaceOnUpdate = true;
			_followButton.gameObject.SetActive(false);
			_lockPlacementButton.gameObject.SetActive(true);
		}

		private void OnLockPlacementClicked()
		{
			_PlaceInFront.PlaceOnUpdate = false;
			_followButton.gameObject.SetActive(true);
			_lockPlacementButton.gameObject.SetActive(false);
		}
		private void MenuKeyPressed(InputAction.CallbackContext obj)
		{
			
			if (!_calibrationInProgress)
			{
				StartCalibration();
			}
			else
			{
				StopCalibration();
			}
		}

		private void StartCalibration()
		{
			if (_calibrationInProgress)
			{
				return;
			}

			_instructionsPanel.gameObject.SetActive(false);
			_calibrationPanel.gameObject.SetActive(true);
			_calibrationText.text = $"- Please look at the marker.";
			_currentCalibrationStep = 0;
			virtualFiducialMarker.MarkerUpdated += MarkerUpdated;
			_markerTracker.StartTracking();
			_calibrationInProgress = true;

		}

		void MarkerUpdated()
		{
			_currentCalibrationStep++;
			_calibrationText.text = $"- Calibration: [{Mathf.RoundToInt((_currentCalibrationStep / ((float)_calibrationSteps)) * 100)}%] Complete.";
			if (_currentCalibrationStep >= _calibrationSteps)
			{
				
				FinishCalibration();
				_calibrationText.text = $"- Calibration Complete.";
			
			
			}

			
		}

		private void StopCalibration()
		{
			if(!_calibrationInProgress)
			{
				return;
			}
			if (_currentCalibrationStep < _calibrationSteps)
			{
				_calibrationText.text = $"- Calibration Stopped.";
			}

		
			FinishCalibration();
			
			
		}

		void FinishCalibration()
		{
			virtualFiducialMarker.MarkerUpdated -= MarkerUpdated;
			_markerTracker.StopTracking();
			_currentCalibrationStep = 0;
			_calibrationInProgress = false;
			StartCoroutine(WaitToDisable(3));
		
		}

		IEnumerator WaitToDisable(float seconds)
		{
			yield return new WaitForSeconds(seconds);
			_instructionsPanel.gameObject.SetActive(true);
			_calibrationPanel.gameObject.SetActive(false);

		}

	
	}
}

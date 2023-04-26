using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

namespace MagicLeap.Networking.Interactions
{

	/// <summary>
	/// Modified version of script "NetworkHandColliderGrabbableCube": from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Debuggable version of <see cref="NetworkColliderGrabbable"/>.
	/// </para>
	/// </remarks>
[RequireComponent(typeof(NetworkColliderGrabbable))]
public class NetworkColliderGrabbableCube : NetworkBehaviour
{
	public TextMeshProUGUI authorityText;
	public TextMeshProUGUI debugText;

	private void Awake()
	{
		debugText.text = "";
		var grabbable = GetComponent<NetworkColliderGrabbable>();
		grabbable.onDidGrab.AddListener(OnDidGrab);
		grabbable.onWillGrab.AddListener(OnWillGrab);
		grabbable.OnDidUngrab.AddListener(OnDidUngrab);
	}

	private void DebugLog(string debug)
	{
		debugText.text = debug;
		Debug.Log(debug);
	}

	private void UpdateStatusCanvas()
	{
		if (Object.HasStateAuthority)
			authorityText.text = "You have the state authority on this object";
		else
			authorityText.text = "You have NOT the state authority on this object";
	}

	public override void FixedUpdateNetwork()
	{
		UpdateStatusCanvas();
	}

	void OnDidUngrab()
	{
		DebugLog($"{gameObject.name} ungrabbed");
	}

	void OnWillGrab(NetworColliderGrabber newGrabber)
	{
		DebugLog($"Grab on {gameObject.name} requested by {newGrabber}. Waiting for state authority ...");
	}

	void OnDidGrab(NetworColliderGrabber newGrabber)
	{
		DebugLog($"{gameObject.name} grabbed by {newGrabber}");
	}
}
}
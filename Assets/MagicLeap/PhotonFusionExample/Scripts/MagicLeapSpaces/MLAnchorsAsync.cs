using System;
using System.Collections;
using System.Collections.Generic;
using MagicLeap.Utilities;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap
{
	public class MLAnchorsAsync : Singleton<MLAnchorsAsync>
	{
		private MLPermissions.Callbacks _permissionCallbacks = new MLPermissions.Callbacks();

	

		private List<MLAnchors.Anchor> _updatedAnchors = new List<MLAnchors.Anchor>();
		private List<string> _removedAnchorsIds = new List<string>();
		private List<MLAnchors.Anchor> _addedAnchors = new List<MLAnchors.Anchor>();
		private List<string> _cachedCurrentAnchors = new List<string>();
		private bool _hasPermissions = true;
		private bool _permissionsRequested = true;
		private Action<bool> _granted;
		void Start()
		{
			_permissionsRequested = false;
			_permissionCallbacks.OnPermissionGranted+= PermissionCallbacksOnPermissionGranted;
			_permissionCallbacks.OnPermissionDenied += PermissionCallbacksOnPermissionDenied;
			MLPermissions.RequestPermission(MLPermission.SpatialAnchors, _permissionCallbacks);
	
		}

		public void GetPermissionResponse(Action<bool> granted)
		{
			if (_permissionsRequested)
			{
				granted?.Invoke(_hasPermissions);
			}
			else
			{
				_granted += granted;
			}
		}


		private void PermissionCallbacksOnPermissionDenied(string permission)
		{
		
			if (permission == MLPermission.SpatialAnchors)
			{
				_permissionsRequested = true;
				Debug.LogError("[LOCALIZATION] :: Cannot start Spatial Anchors. Permission Denied.");
				enabled = false;
				_granted?.Invoke(false);
			}
		}

		private void PermissionCallbacksOnPermissionGranted(string permission) 
		{

			if (permission == MLPermission.SpatialAnchors)
			{
				_permissionsRequested = true;
				_granted?.Invoke(true);
				Debug.Log("[LOCALIZATION] :: Started Spatial Anchors. Permission Granted.");
				_hasPermissions = true;
			}
		}

		private void Log(LogType type, object message)
		{
			ThreadDispatcher.RunOnMainThread(() => { Debug.unityLogger.Log(type, message); });

		}
		private MLResult QueryAnchors(out MLAnchors.Anchor[] anchors)
		{
			anchors = Array.Empty<MLAnchors.Anchor>();

			MLAnchors.Request request = new MLAnchors.Request();
			MLAnchors.Request.Params queryParams = new MLAnchors.Request.Params(Vector3.zero,0,0,false);
			MLResult result = request.Start(queryParams);
			if (!result.IsOk)
			{
				return result;
			}

			result = request.TryGetResult(out var resultData);
			if (result.IsOk)
			{
				anchors = resultData.anchors;
			}

			return result;
		}
		
		public void QueryAnchors(Action<List<MLAnchors.Anchor>> updated, Action<List<MLAnchors.Anchor>> added, Action<List<string>> removed)
		{
			ThreadDispatcher.RunOnWorkerThread(() =>
												{
													QueryAnchorsWorkerThread(updated, added, removed);
												});
		}

		public void GetLocalizationInfo(Action<MLAnchors.LocalizationInfo> infoUpdated)
		{

			ThreadDispatcher.RunOnWorkerThread(() =>
												{
													GetLocalizationStatusOnWorkerThread(infoUpdated);
												});
		}

		public void Create(Vector3 position, Quaternion rotation, long time, Action<MLAnchors.Anchor> created)
		{
		
			ThreadDispatcher.RunOnWorkerThread(() =>
												{
													if (MLPermissions.CheckPermission(MLPermission.SpatialAnchors).IsOk)
													{

														var result = MLAnchors.Anchor.Create(new Pose(position, rotation), time, out MLAnchors.Anchor anchor);
														if (result.IsOk)
														{
															ThreadDispatcher.RunOnMainThread(() => { created?.Invoke(anchor); });
														}
														else
														{
															Log(LogType.Error, $"[LOCALIZATION] :: Failed to create anchor. Reason: {result.Result}");
														}
													}
												});
			
		}

		public void Publish(MLAnchors.Anchor anchor, Action<MLAnchors.Anchor> published)
		{

			ThreadDispatcher.RunOnWorkerThread(() =>
												{
												

													
														var result = anchor.Publish();
														if (result.IsOk)
														{
															ThreadDispatcher.RunOnMainThread(() => { published?.Invoke(anchor); });
														}
														else
														{
															Log(LogType.Error, $"[LOCALIZATION] :: Failed to create anchor. Reason: {result.Result}");
														}

												});

		}
		
		public void Delete(Vector3 center, float radius, uint maxResults, bool sorted, Action<MLResult,string> deleted)
		{


			ThreadDispatcher.RunOnWorkerThread(() =>
												{

														var deleteQuery = new MLAnchors.Request();
														var queryResult = deleteQuery.Start(new MLAnchors.Request.Params(center, radius, maxResults, sorted));
														if (queryResult.IsOk)
														{
															var getResult = deleteQuery.TryGetResult(out MLAnchors.Request.Result result);
															if (getResult.IsOk)
															{
																if (result.anchors.Length > 0)
																{
																	var anchor = result.anchors[0];
																	var anchorId = anchor.Id;
																	var deleteResult = anchor.Delete();
																	if (deleteResult.IsOk)
																	{
																		ThreadDispatcher.RunOnMainThread(() => { deleted?.Invoke(deleteResult, anchorId); });
																	}
																	else
																	{
																		Log(LogType.Error, $"[LOCALIZATION] :: Cannot delete anchor [{anchorId}]: {getResult.Result}");
																	}
														
																}
															}
															else
															{
																Log(LogType.Error, $"[LOCALIZATION] :: Cannot get query results: {getResult.Result}");
															}
														}
														else
														{
															Log(LogType.Error, $"[LOCALIZATION] :: Cannot query anchors: {queryResult.Result}");
														}

												});
			
		}

		private void QueryAnchorsWorkerThread(Action<List<MLAnchors.Anchor>> updated, Action<List<MLAnchors.Anchor>> added, Action<List<string>> removed)
		{
			MLResult result = QueryAnchors(out var anchors);
		
			if (result.IsOk)
			{
			

				ThreadDispatcher.RunOnMainThread(() => UpdateAnchors(anchors, updated, added, removed));
			}
			else
			{
				Log(LogType.Error, $"[LOCALIZATION] :: Cannot query anchors: {result.Result}");
			}
		}
		
		
		void UpdateAnchors(MLAnchors.Anchor[] anchors,Action<List<MLAnchors.Anchor>> updated, Action<List<MLAnchors.Anchor>> added, Action<List<string>> removed)
		{
			_addedAnchors.Clear();
			_removedAnchorsIds.Clear();
			_updatedAnchors.Clear();
			var currentAnchors = new List<string>();
			foreach (MLAnchors.Anchor anchor in anchors)
			{
				if (IsPoseValid(anchor.Pose))
				{
					if (_cachedCurrentAnchors.Contains(anchor.Id))
					{
						_updatedAnchors.Add(anchor);
					}
					else
					{
						_addedAnchors.Add(anchor);
					}

					
				}

				currentAnchors.Add(anchor.Id);
			}

			foreach (string anchorId in _cachedCurrentAnchors)
			{
				if (!currentAnchors.Contains(anchorId))
				{
					_removedAnchorsIds.Add(anchorId);
				}
			}

			if (_addedAnchors.Count > 0)
			{
				added?.Invoke(_addedAnchors);
			}

			if (_updatedAnchors.Count > 0)
			{
				updated?.Invoke(_updatedAnchors);
			}

			if (_removedAnchorsIds.Count > 0)
			{
				removed?.Invoke(_removedAnchorsIds);
			}

			_cachedCurrentAnchors = currentAnchors;
		}
		
		void GetLocalizationStatusOnWorkerThread(Action<MLAnchors.LocalizationInfo> infoUpdated)
		{
				MLAnchors.LocalizationInfo mlInfo;
				var result = MLAnchors.GetLocalizationInfo(out mlInfo);
				if (result.IsOk)
				{
				
					ThreadDispatcher.RunOnMainThread(() => infoUpdated?.Invoke(mlInfo));
				}
				else
				{
					Log(LogType.Error, $"[LOCALIZATION] :: Get Localization Info Failed: {result.Result}");
				}
				
				
		
		}

		bool IsPoseValid(Pose anchorPose)
		{
			return !(anchorPose.rotation.x == 0
				&& anchorPose.rotation.y == 0
				&& anchorPose.rotation.z == 0
				&& anchorPose.rotation.w == 0);
		}
	}
}

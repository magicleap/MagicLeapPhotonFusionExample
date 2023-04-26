using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace MagicLeapNetworkingDemo.Editor
{
	/// <summary>
	/// Checks if jp.keijiro.apriltag is added or removed 
	/// </summary>
	/// <seealso cref="UnityEditor.AssetPostprocessor" />
	[InitializeOnLoad]
	public class ProcessorTypeDefines : AssetPostprocessor
	{
		private const string APRIL_TAGS_DEFINES_SYMBOL = "APRIL_TAGS";
		private const string PACKAGE_ID = "jp.keijiro.apriltag";
		private const string PACKAGE_FILE = "AprilTag.Runtime.asmdef";

		private static bool _packageInstalled
		{
			get
			{
#if APRIL_TAGS
				return true;
#else
				return false;
#endif
			}
		}
		static ProcessorTypeDefines()
		{

			EditorApplication.projectChanged += EditorApplicationOnProjectChanged;
			EditorApplication.delayCall += EditorApplicationOnProjectChanged;
			EditorApplication.delayCall += CheckForAprilTagPackage;
			EditorApplication.quitting += OnQuit;
			Events.registeringPackages += RegisteringPackagesEventHandler;
		}
		private static void OnQuit()
		{
			EditorApplication.quitting -= OnQuit;
			Events.registeringPackages -= RegisteringPackagesEventHandler;
		}

		/// <summary>
		/// Checks for package based on package name and/or file
		/// </summary>
		/// <param name="packageName">Name of package to find.</param>
		/// <param name="fileInPackage">The file in package.</param>
		private static bool HasPackages(string packageName, string fileInPackage)
		{

			if (!string.IsNullOrWhiteSpace(packageName))
			{
				var tgzFileExists = File.Exists(Path.GetFullPath(Application.dataPath + "/../Packages/" + packageName + ".tgz"));
				if (tgzFileExists)
				{
					return true;
				}

				var packageFolderExistsInPackages = Directory.EnumerateDirectories(Path.GetFullPath(Application.dataPath + "/../Packages"), packageName + "*").Any();
				if (packageFolderExistsInPackages)
				{
					return true;
				}
			}
			if (!string.IsNullOrWhiteSpace(fileInPackage))
			{
			
				var hasFileInPackages = Directory.EnumerateFiles(Path.GetFullPath(Application.dataPath + "/../Packages/"), PACKAGE_FILE, SearchOption.AllDirectories).Any();
				if (hasFileInPackages)
				{
					return true;
				}
			}

			return false;
		}
		
		// The method is expected to receive a PackageRegistrationEventArgs event argument to check if th packages is being installed or uninstalled.
		private static void RegisteringPackagesEventHandler(PackageRegistrationEventArgs packageRegistrationEventArgs)
		{
			foreach (var addedPackage in packageRegistrationEventArgs.added)
			{

				if (addedPackage.name == PACKAGE_ID)
				{
					if (!ContainsDefineSymbolInAllBuildTargets(APRIL_TAGS_DEFINES_SYMBOL))
						AddDefineSymbol(APRIL_TAGS_DEFINES_SYMBOL);
				}

			}

			foreach (var removedPackage in packageRegistrationEventArgs.removed)
			{

				if (removedPackage.name == PACKAGE_ID)
				{
					if (ContainsDefineSymbolInAnyBuildTarget(APRIL_TAGS_DEFINES_SYMBOL))
						RemoveDefineSymbol(APRIL_TAGS_DEFINES_SYMBOL);
				}

			}

		}
		
		[MenuItem("Magic Leap/Fusion Example/Install Desktop April Tag Detector")]
		public static void InstallGenericAprilTagTracking()
		{
			CheckForAprilTagPackage();
			var packagePath = "MagicLeap/PhotonFusionExample/ThirdParty";
			var packageName = "jp.keijiro.apriltag.zip";
			var zipAssetFolderRelativePath = Path.Combine(Application.dataPath, packagePath, packageName).Replace("\\", "/");
			var destination = Path.Combine(Application.dataPath, "../", "Packages").Replace("\\","/");
			var fullPathToDestination = Path.GetFullPath(destination);
			var fullPathToZip = Path.GetFullPath(zipAssetFolderRelativePath);
			var packageExists = File.Exists(fullPathToZip);
			
			if (!packageExists)
			{
				EditorUtility.DisplayDialog("Package Not Found", $"Please insure that the file jp.keijiro.apriltag.zip is located at [Assets/{packagePath}].", "Okay");
				return;
			}

			if (_packageInstalled)
			{
				EditorUtility.DisplayDialog("Package already installed", $"AprilTag package for Unity by jp.keijiro is already installed.", "Okay");
			}
			else
			{
				System.IO.Compression.ZipFile.ExtractToDirectory(fullPathToZip, fullPathToDestination);
				AssetDatabase.Refresh();
			}

			
			
		}

		private static void CheckForAprilTagPackage()
		{

			if (!HasPackages(PACKAGE_ID, PACKAGE_FILE) && ContainsDefineSymbolInAnyBuildTarget(APRIL_TAGS_DEFINES_SYMBOL))
			{
				RemoveDefineSymbol(APRIL_TAGS_DEFINES_SYMBOL);
			}

			if (HasPackages(PACKAGE_ID, PACKAGE_FILE) && !ContainsDefineSymbolInAnyBuildTarget(APRIL_TAGS_DEFINES_SYMBOL))
			{
				AddDefineSymbol(APRIL_TAGS_DEFINES_SYMBOL);
			}
		}
		private static void EditorApplicationOnProjectChanged()
		{
		
			if (((int)PlayerSettings.Android.targetArchitectures & (~(int)AndroidArchitecture.X86_64)) == 0)
			{
				if (!ContainsDefineSymbol(BuildTargetGroup.Android, "ANDROID_X86_64"))
				{
					AddDefineSymbol(BuildTargetGroup.Android, "ANDROID_X86_64");
				}
			}
			else
			{
				if (ContainsDefineSymbol(BuildTargetGroup.Android, "ANDROID_X86_64"))
				{
					RemoveDefineSymbol(BuildTargetGroup.Android, "ANDROID_X86_64");
				}
			}
			if (((int)PlayerSettings.Android.targetArchitectures & (~(int)AndroidArchitecture.ARMv7)) == 0)
			{
				if (!ContainsDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM7"))
				{
					AddDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM7");
				}
			}
			else
			{
				if (ContainsDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM7"))
				{
					RemoveDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM7");
				}
			}

			if (((int)PlayerSettings.Android.targetArchitectures & (~(int)AndroidArchitecture.ARM64)) == 0)
			{
				if (!ContainsDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM64"))
				{
					AddDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM64");
				}
			}
			else
			{
				if (ContainsDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM64"))
				{
					RemoveDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM64");
				}
			}
			
			
		}

		public static void RemoveDefineSymbol(BuildTargetGroup targetGroup, string define)
		{
			
				if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup))
				{
					return;
				}

				var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

				if (defineSymbols.Contains(define))
				{
					defineSymbols = defineSymbols.Replace($"{define};", "");
					defineSymbols = defineSymbols.Replace(define, "");

					PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
				}
		}
		
		
		public static void RemoveDefineSymbol(string define)
		{
			foreach (BuildTargetGroup targetGroup in Enum.GetValues(typeof(BuildTargetGroup)))
			{
				if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup)) continue;

				var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

				if (defineSymbols.Contains(define))
				{
					defineSymbols = defineSymbols.Replace($"{define};", "");
					defineSymbols = defineSymbols.Replace(define, "");

					PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
				}
			}
		}

		public static void AddDefineSymbol(string define)
		{
			foreach (BuildTargetGroup targetGroup in Enum.GetValues(typeof(BuildTargetGroup)))
			{
				if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup)) continue;

				var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

				if (!defineSymbols.Contains(define))
				{
					if (defineSymbols.Length < 1)
						defineSymbols = define;
					else if (defineSymbols.EndsWith(";"))
						defineSymbols = $"{defineSymbols}{define}";
					else
						defineSymbols = $"{defineSymbols};{define}";

					PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
				}
			}
		}

		public static void AddDefineSymbol(BuildTargetGroup targetGroup, string define)
		{
		
			if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup))
			{
				return;
			}

			var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

			if (!defineSymbols.Contains(define))
			{
				if (defineSymbols.Length < 1)
					defineSymbols = define;
				else if (defineSymbols.EndsWith(";"))
					defineSymbols = $"{defineSymbols}{define}";
				else
					defineSymbols = $"{defineSymbols};{define}";

				PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
			}
		
		}
		
		public static bool ContainsDefineSymbolInAllBuildTargets(string symbol)
		{
			bool contains = false;
			foreach (BuildTargetGroup targetGroup in Enum.GetValues(typeof(BuildTargetGroup)))
			{
				if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup)) continue;

				var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
				contains = defineSymbols.Contains(symbol);
				if (!contains)
				{
					break;
				}
			}

			return contains;
		}
		
		public static bool ContainsDefineSymbolInAnyBuildTarget(string symbol)
		{
			bool contains = false;
			foreach (BuildTargetGroup targetGroup in Enum.GetValues(typeof(BuildTargetGroup)))
			{
				if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup)) continue;

				var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
				contains = defineSymbols.Contains(symbol);
				if (contains)
				{
					break;
				}
			}

			return contains;
		}

		public static bool ContainsDefineSymbol(BuildTargetGroup targetGroup, string symbol)
		{
			if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup))
			{
				return false;
			}

			var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
			return defineSymbols.Contains(symbol);
				
		}
		private static bool IsObsolete(BuildTargetGroup group)
		{
			var attrs = typeof(BuildTargetGroup).GetField(group.ToString()).GetCustomAttributes(typeof(ObsoleteAttribute), false);
			return attrs.Length > 0;
		}
	}
}

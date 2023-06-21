using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace MagicLeapNetworkingDemo.Editor
{
	/// <summary>
	/// Allows user to install jp.keijiro.apriltag
	/// </summary>
	/// <seealso cref="UnityEditor.AssetPostprocessor" />
	/// <remarks>
	/// <para>
	///   The script does the following:
	/// <list type="bullet">
	///     <item>
	///         <description> Adds [Magic Leap > Fusion Example > Install Desktop April Tag Detector] menu item. </description>
	///     </item>
	///     <item>
	///         <description> Adds defines symbol `APRIL_TAGS` if the package is installed</description>
	///     </item>
	///     <item>
	///         <description> Removes defines symbol `APRIL_TAGS` if the package is deleted or removed</description>
	///     </item>
	///     </list>
	///     </para>
	/// </remarks>
	[InitializeOnLoad]
	public class GenericAprilTagTrackingInstaller : AssetPostprocessor
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

		static GenericAprilTagTrackingInstaller()
		{

			EditorApplication.delayCall += CheckForAprilTagPackage;
			EditorApplication.quitting += OnQuit;
			Events.registeringPackages += RegisteringPackagesEventHandler;
		}

		private static void OnQuit()
		{
			EditorApplication.quitting -= OnQuit;
			Events.registeringPackages -= RegisteringPackagesEventHandler;
		}
		
		[MenuItem("Magic Leap/Fusion Example/Install Desktop April Tag Detector")]
		public static void InstallGenericAprilTagTracking()
		{
			CheckForAprilTagPackage();
			var packagePath = "MagicLeap/PhotonFusionExample/ThirdParty";
			var packageName = "jp.keijiro.apriltag.zip";
			var zipAssetFolderRelativePath = Path.Combine(Application.dataPath, packagePath, packageName).Replace("\\", "/");
			var destination = Path.Combine(Application.dataPath, "../", "Packages").Replace("\\", "/");
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
				Client.Resolve();
				
			}
		}

	
		private static void CheckForAprilTagPackage()
		{

			if (!HasPackages(PACKAGE_ID, PACKAGE_FILE) && DefineSymbolUtility.ContainsDefineSymbolInAnyBuildTarget(APRIL_TAGS_DEFINES_SYMBOL))
			{
				DefineSymbolUtility.RemoveDefineSymbol(APRIL_TAGS_DEFINES_SYMBOL);
			}

			if (HasPackages(PACKAGE_ID, PACKAGE_FILE) && !DefineSymbolUtility.ContainsDefineSymbolInAnyBuildTarget(APRIL_TAGS_DEFINES_SYMBOL))
			{
				DefineSymbolUtility.AddDefineSymbol(APRIL_TAGS_DEFINES_SYMBOL);
			}
		}

		// The method is expected to receive a PackageRegistrationEventArgs event argument to check if th packages is being installed or uninstalled.
		private static void RegisteringPackagesEventHandler(PackageRegistrationEventArgs packageRegistrationEventArgs)
		{
			foreach (var addedPackage in packageRegistrationEventArgs.added)
			{

				if (addedPackage.name == PACKAGE_ID)
				{
					if (!DefineSymbolUtility.ContainsDefineSymbolInAllBuildTargets(APRIL_TAGS_DEFINES_SYMBOL))
						DefineSymbolUtility.AddDefineSymbol(APRIL_TAGS_DEFINES_SYMBOL);
				}

			}

			foreach (var removedPackage in packageRegistrationEventArgs.removed)
			{

				if (removedPackage.name == PACKAGE_ID)
				{
					if (DefineSymbolUtility.ContainsDefineSymbolInAnyBuildTarget(APRIL_TAGS_DEFINES_SYMBOL))
						DefineSymbolUtility.RemoveDefineSymbol(APRIL_TAGS_DEFINES_SYMBOL);
				}

			}

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
	}
}

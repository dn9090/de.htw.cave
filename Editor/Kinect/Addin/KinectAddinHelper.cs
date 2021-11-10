using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Htw.Cave.Kinect.Addin
{
	public static class KinectAddinHelper
	{
		public static string packageName = "de.htw.cave";

		public static string packagesDir = Path.Combine(Application.dataPath, "..", "Packages");

		public static string pluginsDirName = "Plugins";

		public static readonly string[] kinectPluginDirNames = { "Metro", "x86", "x86_64" };

		public static readonly string kinectPluginPath = Path.Combine("Kinect", pluginsDirName);

		public static DirectoryInfo[] PluginDirs(DirectoryInfo source) =>
			kinectPluginDirNames.Select(dir => new DirectoryInfo(Path.Combine(source.FullName, dir))).ToArray();

		public static bool IsPluginInAssets()
		{
			string targetDir = Path.Combine(Application.dataPath, pluginsDirName);
			return PluginDirs(new DirectoryInfo(targetDir)).Any(dir => dir.Exists);
		}

		public static void Move(DirectoryInfo source, DirectoryInfo destination)
		{
			if (!Directory.Exists(source.FullName))
				return;

			Directory.Move(source.FullName, destination.FullName);
			File.Delete(source.FullName + ".meta");
		}

		public static void MovePluginsToAssets()
		{
			string packageDir = FindPackageDirectory(packagesDir);

			if(packageDir == null)
				throw PackageDirectoryNotFoundException();

			string kinectDir = Path.Combine(packageDir, kinectPluginPath);
			string targetDir = Path.Combine(Application.dataPath, pluginsDirName);

			if(!Directory.Exists(targetDir))
				Directory.CreateDirectory(targetDir);

			foreach(DirectoryInfo dir in PluginDirs(new DirectoryInfo(kinectDir)))
				Move(dir, new DirectoryInfo(Path.Combine(targetDir, dir.Name)));
		}

		public static void MovePluginsToPackage()
		{
			string packageDir = FindPackageDirectory(packagesDir);

			if(packageDir == null)
				throw PackageDirectoryNotFoundException();

			string kinectDir = Path.Combine(packageDir, kinectPluginPath);
			string targetDir = Path.Combine(Application.dataPath, pluginsDirName);

			foreach(DirectoryInfo dir in PluginDirs(new DirectoryInfo(targetDir)))
				Move(dir, new DirectoryInfo(Path.Combine(kinectDir, dir.Name)));

			if(!Directory.EnumerateFileSystemEntries(targetDir).Any())
			{
				Directory.Delete(targetDir);
				File.Delete(targetDir + ".meta");
			}
		}

		public static void Import()
		{
			AssetDatabase.Refresh();
		}

		public static string FindPackageDirectory(string path)
		{
			// Version control systems add the branch name to the package name sometimes.
			// So if the package cannot be found look for the substring.
			
			var packageDir = new DirectoryInfo(Path.Combine(path, packageName));

			if(packageDir.Exists)
				return packageDir.FullName;

			foreach(var dir in Directory.GetDirectories(path))
				if(Path.GetDirectoryName(dir).Contains(packageName))
					return dir;
			
			return null;
		}

		public static void FixBuildPluginSubdirectory(string executable, BuildTarget target)
		{
			// In older Unity versions the plugins are copied directly under the Plugins/ folder.
			// Newer Unity versions (2019.4 and higher) copy the plugins to a subfolder
			// based on the platform like Plugins/x86_64 which results in broken file path
			// lookups from the Kinect Addin.

			// The fix tries to restore the old directory structure.
			// In the future it would be better to replace the KinectCopyPluginDataHelper
			// with a variant that works without moving the Plugin folder everytime.

			var buildTargetDir = target == BuildTarget.StandaloneWindows64 ? "x86_64" : "x86";
			var dataPath = Path.Combine(Path.GetDirectoryName(executable), Path.GetFileNameWithoutExtension(executable) + "_Data");
			
			try
			{
				
				var pluginsDir = Path.Combine(dataPath, "Plugins");
				var plugins = Directory.GetFiles(Path.Combine(pluginsDir, buildTargetDir));

				foreach(var plugin in plugins)
					File.Move(plugin, Path.Combine(pluginsDir, Path.GetFileName(plugin)));
			} catch(DirectoryNotFoundException) {
				return; // Nothing to fix...
			}
		}

		public static DirectoryNotFoundException PackageDirectoryNotFoundException() =>
			new DirectoryNotFoundException("Unable to locate the package directory containing the Kinect Addin. "
				+ "Make sure that the package is located inside the Packages/ folder.");
	}
}

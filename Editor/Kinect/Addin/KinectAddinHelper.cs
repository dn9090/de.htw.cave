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
		public const string PackageDirName = "de.htw.cave";
		public const string PackagesDirName = "Packages";
		public const string PluginsDirName = "Plugins";

		public static readonly string[] KinectPluginDirNames = {"Metro", "x86", "x86_64"};
		public static readonly string KinectPluginPath = "ThirdParty/Kinect/" + PluginsDirName;

		public static DirectoryInfo[] PluginDirs(DirectoryInfo source)
		{
			return KinectPluginDirNames.Select(dir => new DirectoryInfo(Path.Combine(source.FullName, dir))).ToArray();
		}

		public static bool IsPluginInAssets()
		{
			char separator = Path.DirectorySeparatorChar;
			string targetDir = Application.dataPath + separator + PluginsDirName;
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
			char separator = Path.DirectorySeparatorChar;
			string packageDir = Path.Combine(Application.dataPath, ".." + separator + PackagesDirName + separator + PackageDirName);
			string kinectDir = Path.Combine(packageDir, KinectPluginPath);
			string targetDir = Application.dataPath + separator + PluginsDirName;

			if(!Directory.Exists(targetDir))
				Directory.CreateDirectory(targetDir);

			foreach(DirectoryInfo dir in PluginDirs(new DirectoryInfo(kinectDir)))
				Move(dir, new DirectoryInfo(Path.Combine(targetDir, dir.Name)));
		}

		public static void MovePluginsToPackage()
		{
			char separator = Path.DirectorySeparatorChar;
			string packageDir = Path.Combine(Application.dataPath, ".." + separator + PackagesDirName + separator + PackageDirName);
			string kinectDir = Path.Combine(packageDir, KinectPluginPath);
			string targetDir = Application.dataPath + separator + PluginsDirName;

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
	}
}

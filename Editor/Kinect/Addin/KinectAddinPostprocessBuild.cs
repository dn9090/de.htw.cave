using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Htw.Cave.Kinect.Addin
{
	public class KinectAddinPostprocessBuild : IPostprocessBuildWithReport
	{
		public int callbackOrder { get { return 1000; } }

		public void OnPostprocessBuild(BuildReport report)
		{
			// Moving Kinect DLL's from the assets back to package folder
			// after they where exported in the building process.
			KinectAddinHelper.MovePluginsToPackage();
			KinectAddinHelper.Import();

			// Fix plugin subfolder for new Unity versions.
			KinectAddinHelper.FixBuildPluginSubdirectory(report.summary.outputPath, report.summary.platform);
		}
	}
}

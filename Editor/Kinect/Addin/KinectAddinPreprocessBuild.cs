using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Htw.Cave.Kinect.Addin
{
	public class KinectAddinPreprocessBuild : IPreprocessBuildWithReport
	{
		public int callbackOrder { get { return 1; } }

		public void OnPreprocessBuild(BuildReport report)
		{
			// Moving Kinect DLL's from the package to the assets folder
			// because the Kinect helpers copy all required dlls to
			// the final plugin location.
			KinectAddinHelper.MovePluginsToAssets();
			KinectAddinHelper.Import();
		}
	}
}

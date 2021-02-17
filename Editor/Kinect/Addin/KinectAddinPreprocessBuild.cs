using System;
using System.IO;
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
			// moving kinect dlls from package space to assets space
			// because kinect helpers copy all required dlls from assets space to
			// the final plugin location.
			KinectAddinHelper.MovePluginsToAssets();
			KinectAddinHelper.Import();
		}
	}
}

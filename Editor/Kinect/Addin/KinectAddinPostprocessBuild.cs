using System;
using System.IO;
using System.Linq;
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
		}
	}
}

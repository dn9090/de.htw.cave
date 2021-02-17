using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR_WIN
using Microsoft.Win32;
#endif

namespace Htw.Cave.Kinect
{
	public static class KinectEditorUtils
	{
		[DrawGizmo(GizmoType.Active)]
		public static void PlayAreaGizmos(KinectPlayArea playArea, GizmoType type)
		{
			if(playArea.Settings == null)
				return;

			Vector3[] coordinates = playArea.GetWorldCoordinates();

			Gizmos.color = new Color(0.6f, 0.4f, 0.8f);

			Gizmos.DrawLine(coordinates[0], coordinates[1]);
			Gizmos.DrawLine(coordinates[1], coordinates[2]);
			Gizmos.DrawLine(coordinates[2], coordinates[3]);
			Gizmos.DrawLine(coordinates[3], coordinates[0]);

			Gizmos.DrawLine(coordinates[0], coordinates[0] + Vector3.up * 0.1f);
			Gizmos.DrawLine(coordinates[1], coordinates[1] + Vector3.up * 0.1f);
			Gizmos.DrawLine(coordinates[2], coordinates[2] + Vector3.up * 0.1f);
			Gizmos.DrawLine(coordinates[3], coordinates[3] + Vector3.up * 0.1f);
		}

#if UNITY_EDITOR_WIN
		private static bool sdkSearched = false;
		private static bool sdkInstallationFound = false;

		public static bool IsSDKInstalled()
		{
			if(sdkSearched)
				return sdkInstallationFound;

			string sdk = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Kinect\\v2.0", "SDKInstallPath", null) as string;

			sdkSearched = true;
			sdkInstallationFound = !string.IsNullOrEmpty(sdk);

			return sdkInstallationFound;
		}
#else
		public static bool IsSDKInstalled()
		{
			return false;
		}
#endif
	}
}

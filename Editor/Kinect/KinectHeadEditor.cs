using System;
using UnityEngine;
using UnityEditor;

namespace Htw.Cave.Kinect
{
	[CustomEditor(typeof(KinectHead))]
	public class KinectHeadEditor : Editor
	{
		private static string[] s_WearingGlasses = {
			"Unknown",
			"No",
			"Maybe",
			"Yes"
		};

		private KinectHead m_Me;	
	
		public void OnEnable()
		{
			this.m_Me = (KinectHead)target;

			EditorApplication.update += Repaint;
		}

		public void OnDisable()
		{
			EditorApplication.update -= Repaint;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField("Wearing Glasses", s_WearingGlasses[(int)this.m_Me.wearingGlasses]);

			EditorGUI.EndChangeCheck();
		}

		[DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.InSelectionHierarchy)]
		public static void DrawGizmos(KinectHead head, GizmoType type)
		{
			Vector3 origin = head.transform.position;
			Vector3 mid = origin + head.transform.forward * 0.4f;
			Vector3 point = origin + head.transform.forward * 0.6f;

			Gizmos.color = new Color(0f, 0.7f, 0f, 1f);
			Gizmos.DrawLine(origin, point);
			Gizmos.DrawLine(mid + head.transform.right * 0.2f, point);
			Gizmos.DrawLine(mid - head.transform.right * 0.2f, point);
		}
	}
}
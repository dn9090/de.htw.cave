using System;
using UnityEngine;
using UnityEditor;

namespace Htw.Cave.Kinect
{
	[CustomEditor(typeof(KinectActor))]
	public class KinectActorEditor : Editor
	{
		private KinectActor m_Me;
	
		public void OnEnable()
		{
			this.m_Me = (KinectActor)target;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField("Tracking Id", this.m_Me.trackingId.ToString());
			EditorGUILayout.LabelField("Created At", this.m_Me.createdAt + "s");
			EditorGUILayout.LabelField("Height", this.m_Me.height + "m");

			EditorGUI.EndChangeCheck();
		}

		[DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.InSelectionHierarchy)]
		public static void DrawGizmos(KinectActor actor, GizmoType type)
		{
			Gizmos.color = new Color(0f, 0.7f, 0f, 1f);
			Gizmos.DrawWireCube(actor.bounds.center, actor.bounds.size);
		}
	}
}
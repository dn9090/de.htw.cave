using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Windows.Kinect;
using Htw.Cave.Kinect.Utils;

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
			if(Application.isPlaying && this.m_Me.trackingId > 0)
			{
				EditorGUILayout.LabelField("Tracking Id", this.m_Me.trackingId.ToString());
				EditorGUILayout.LabelField("Created At", this.m_Me.createdAt + "s");
				EditorGUILayout.LabelField("Height", this.m_Me.height + "m");
			}
		}

		[DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy)]
		public static void DrawGizmos(KinectActor actor, GizmoType type)
		{
			Gizmos.color = KinectEditorUtils.green;
			Gizmos.DrawWireCube(actor.bounds.center, actor.bounds.size);
		}
	}
}
using System;
using UnityEngine;
using UnityEditor;

namespace Htw.Cave.Kinect
{
	[CustomEditor(typeof(KinectTrackingArea))]
	public class KinectTrackingAreaEditor : Editor
	{
		private KinectTrackingArea m_Me;

		private SerializedProperty m_ActorTrackerProperty;

		private SerializedProperty m_VolumeProperty;

		private SerializedProperty m_SelectionTypeProperty;
	
		public void OnEnable()
		{
			this.m_Me = (KinectTrackingArea)target;
			this.m_ActorTrackerProperty = serializedObject.FindProperty("actorTracker");
			this.m_VolumeProperty = serializedObject.FindProperty("m_Volume");
			this.m_SelectionTypeProperty = serializedObject.FindProperty("m_SelectionType");

			this.m_Me.onActorChanged += UpdateActor;
		}

		public void OnDisable()
		{
			this.m_Me.onActorChanged -= UpdateActor;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorGUILayout.PropertyField(this.m_ActorTrackerProperty);
			EditorGUILayout.PropertyField(this.m_VolumeProperty);
			EditorGUILayout.PropertyField(this.m_SelectionTypeProperty);

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();

			if(Application.isPlaying)
			{
				if(this.m_Me.actor == null)
					EditorGUILayout.LabelField("No main actor available.");
				else
					EditorGUILayout.LabelField("Main Actor", this.m_Me.actor.trackingId.ToString());
			}
		}

		private void UpdateActor(KinectActor actor)
		{
			Repaint();
		}

		[DrawGizmo(GizmoType.Active | GizmoType.Selected)]
		public static void DrawGizmos(KinectTrackingArea trackingArea, GizmoType type)
		{
			var volume = trackingArea.GetVolumeWorldSpace();

			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(volume.center, volume.size);
		}
	}
}
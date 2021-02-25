using System;
using UnityEngine;
using UnityEditor;
using Windows.Kinect;
using Microsoft.Kinect.Face;

namespace Htw.Cave.Kinect
{
	[CustomEditor(typeof(KinectManager))]
	public class KinectManagerEditor : Editor
	{
		private KinectManager m_Me;

		private SerializedProperty m_FaceFrameFeatureTypeProperty;

		private Vector2 m_ScrollPosition;
		
		public void OnEnable()
		{
			this.m_Me = (KinectManager)target;
			this.m_FaceFrameFeatureTypeProperty = serializedObject.FindProperty("faceFrameFeatureType");

			EditorApplication.update += UpdateKinectData;
		}

		public void OnDisable()
		{
			EditorApplication.update -= UpdateKinectData;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorGUILayout.PropertyField(this.m_FaceFrameFeatureTypeProperty);

			if(!KinectSdkUtil.IsSDKInstalled())
				EditorGUILayout.HelpBox("Unable to find Kinect 2.0 SDK installation.", MessageType.Warning);

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();

			OnKinectSensorGUI();
		}

		private void OnKinectSensorGUI()
		{
			if(this.m_Me.sensor == null)
				return;

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Kinect Sensor Data", EditorStyles.boldLabel);

			string open = this.m_Me.sensor.IsOpen ? "Open" : "Closed";
			string available = this.m_Me.sensor.IsAvailable ? "Available" : "Not Available";

			EditorGUILayout.LabelField("Status", open + ", " + available);

			if(!this.m_Me.sensor.IsOpen)
				return;

			EditorGUILayout.LabelField("Tilt", this.m_Me.tilt.ToString());
			EditorGUILayout.LabelField("Height Offset", this.m_Me.floor.w.ToString());

			var frame = this.m_Me.AcquireFrames(out Body[] bodies, out FaceFrameResult[] faces, out int count);

			EditorGUILayout.LabelField("Frame Number", frame.ToString());
			EditorGUILayout.LabelField("Tracked Body Count", count.ToString());

			if(count > 0)
			{
				this.m_ScrollPosition = EditorGUILayout.BeginScrollView(this.m_ScrollPosition, EditorStyles.helpBox);
				for(int i = 0; i < count; ++i)
				{
					EditorGUILayout.LabelField("Tracking Id", bodies[i].TrackingId.ToString());
				}
				EditorGUILayout.EndScrollView();
			}
		}

		private void UpdateKinectData()
		{
			if(this.m_Me.sensor != null && this.m_Me.sensor.IsOpen)
				Repaint();
		}

		[DrawGizmo(GizmoType.Active | GizmoType.Selected)]
		public static void DrawGizmos(KinectManager manager, GizmoType type)
		{
			if(manager.sensor == null || !manager.sensor.IsAvailable)
				return;

			Transform transform = manager.transform;

			var position = transform.position;
			var rotation = transform.rotation;
			var tilt = manager.tilt;

			transform.position = transform.position + Vector3.up * manager.floor.w;

			if(tilt != float.NaN)
				transform.localEulerAngles += new Vector3(tilt, 180f, 0f);

			// Kinect v2 FOV: 70.6° x 60°
			var fov = new Vector2(70.6f, 60f);
			var minRange = 0.5f;
			var maxRange = 4.5f;
			var aspectRatio = fov.x / fov.y;

			Gizmos.color = UnityEngine.Color.green;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawFrustum(Vector3.zero, fov.y, minRange, maxRange, aspectRatio); 

			transform.rotation = rotation;
			transform.position = position;
		}
	}
}
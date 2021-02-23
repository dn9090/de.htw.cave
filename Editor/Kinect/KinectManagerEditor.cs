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

		private SerializedProperty m_SensorPositionProperty;

		private SerializedProperty m_FaceFrameFeatureTypeProperty;

		private Vector2 m_ScrollPosition;
		
		public void OnEnable()
		{
			this.m_Me = (KinectManager)target;
			this.m_SensorPositionProperty = serializedObject.FindProperty("sensorPosition");
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

			EditorGUILayout.PropertyField(this.m_SensorPositionProperty);

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

			EditorGUILayout.LabelField("Sensor Status", open + ", " + available);

			if(!this.m_Me.sensor.IsOpen)
				return;

			var timeStamp = this.m_Me.AcquireFrames(out Body[] bodies, out FaceFrameResult[] faces, out int count);

			EditorGUILayout.LabelField("Frame Time Stamp", timeStamp.Ticks.ToString());
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
	}
}
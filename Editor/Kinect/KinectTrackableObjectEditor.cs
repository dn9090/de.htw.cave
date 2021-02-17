using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Utilites;

namespace Htw.Cave.Kinect
{
	[CustomEditor(typeof(KinectTrackableObject))]
	public class KinectTrackableObjectEditor : Editor
	{
		private KinectTrackableObject me;
		private SerializedProperty trackingProperty;
		private SerializedProperty jointTypeProperty;
		private SerializedProperty jointConstrainsProperty;
		private SerializedProperty rotationTypeProperty;

		public void OnEnable()
		{
			this.me = (KinectTrackableObject)base.target;
			this.trackingProperty = base.serializedObject.FindProperty("tracking");
			this.jointTypeProperty = base.serializedObject.FindProperty("jointType");
			this.jointConstrainsProperty = base.serializedObject.FindProperty("jointConstrains");
			this.rotationTypeProperty = base.serializedObject.FindProperty("rotationType");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorLayoutUtility.ScriptField(base.serializedObject);

			EditorLayoutUtility.FlagsField<TrackingMask>(this.trackingProperty);

			if(this.me.Tracking.HasFlag(TrackingMask.Position))
			{
				EditorGUILayout.Space();
				EditorLayoutUtility.HeaderField("Position");
				EditorGUILayout.PropertyField(this.jointTypeProperty);
				EditorLayoutUtility.FlagsField<JointConstrains>(this.jointConstrainsProperty);
			}

			if(this.me.Tracking.HasFlag(TrackingMask.Rotation))
			{
				EditorGUILayout.Space();
				EditorLayoutUtility.HeaderField("Rotation");
				EditorGUILayout.PropertyField(this.rotationTypeProperty);
			}

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Utilites;

namespace Htw.Cave.Kinect
{
	[CustomEditor(typeof(KinectTrackableHand))]
	public class KinectTrackableHandEditor : Editor
	{
		private KinectTrackableHand me;
		private SerializedProperty handTypeProperty;

		public void OnEnable()
		{
			this.me = (KinectTrackableHand)base.target;
			this.handTypeProperty = base.serializedObject.FindProperty("handType");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorLayoutUtility.ScriptField(base.serializedObject);

			EditorGUILayout.PropertyField(this.handTypeProperty);

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}

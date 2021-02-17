using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Utilites;

namespace Htw.Cave.Kinect
{
	[CustomEditor(typeof(KinectPlayArea))]
	public class KinectSceneEditor : Editor
	{
		private KinectPlayArea me;
		private Editor editor;
		private SerializedProperty settingsProperty;
		private SerializedProperty enableInEditorProperty;

		public void OnEnable()
		{
			this.me = (KinectPlayArea)base.target;
			this.editor = null;
			this.settingsProperty = base.serializedObject.FindProperty("settings");
			this.enableInEditorProperty = base.serializedObject.FindProperty("enableInEditor");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorLayoutUtility.ScriptField(base.serializedObject);

			EditorLayoutUtility.ScriptableField(this.settingsProperty, ref this.editor);

			EditorGUILayout.PropertyField(this.enableInEditorProperty);

			if(!KinectEditorUtils.IsSDKInstalled())
				EditorGUILayout.HelpBox("Unable to find Kinect 2.0 SDK installation.", MessageType.Warning);

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}

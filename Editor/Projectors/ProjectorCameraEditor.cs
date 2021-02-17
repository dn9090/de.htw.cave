using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Utilites;

namespace Htw.Cave.Projectors
{
	[CustomEditor(typeof(ProjectorCamera))]
	public class ProjectorCameraEditor : Editor
	{
		private ProjectorCamera me;
		private Editor editor;
		private SerializedProperty configurationProperty;
		private SerializedProperty planeProperty;

		public void OnEnable()
		{
			this.me = (ProjectorCamera)base.target;
			this.editor = null;
			this.configurationProperty = base.serializedObject.FindProperty("configuration");
			this.planeProperty = base.serializedObject.FindProperty("plane");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorLayoutUtility.ScriptField(base.serializedObject);

			EditorLayoutUtility.ScriptableField(this.configurationProperty, ref this.editor);

			EditorGUILayout.PropertyField(this.planeProperty);

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Utilites;

namespace Htw.Cave.Projectors
{
	[CustomEditor(typeof(ProjectorBrain))]
	public class ProjectorBrainEditor : Editor
	{
		private ProjectorBrain me;
		private Editor editor;
		private SerializedProperty settingsProperty;

		public void OnEnable()
		{
			this.me = (ProjectorBrain)base.target;
			this.editor = null;
			this.settingsProperty = base.serializedObject.FindProperty("settings");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorLayoutUtility.ScriptField(base.serializedObject);

			EditorLayoutUtility.ScriptableField(this.settingsProperty, ref this.editor);

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}

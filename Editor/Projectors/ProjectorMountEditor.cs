using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Utilites;

namespace Htw.Cave.Projectors
{
	[CustomEditor(typeof(ProjectorMount))]
	public class ProjectorMountEditor : Editor
	{
		private ProjectorMount me;
		private SerializedProperty eyesProperty;

		public void OnEnable()
		{
			this.me = (ProjectorMount)base.target;
			this.eyesProperty = base.serializedObject.FindProperty("eyes");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorLayoutUtility.ScriptField(base.serializedObject);

			EditorGUILayout.PropertyField(this.eyesProperty);

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}

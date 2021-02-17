using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Utilites;

namespace Htw.Cave.Controls
{
	[CustomEditor(typeof(FirstPersonControls))]
	public class FirstPersonControlsEditor : Editor
	{
		private FirstPersonControls me;
		private SerializedProperty speedProperty;
		private SerializedProperty snappynessProperty;
		private SerializedProperty sensitivityProperty;

		public void OnEnable()
		{
			this.me = (FirstPersonControls)base.target;
			this.speedProperty = base.serializedObject.FindProperty("speed");
			this.snappynessProperty = base.serializedObject.FindProperty("snappyness");
			this.sensitivityProperty = base.serializedObject.FindProperty("sensitivity");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorLayoutUtility.ScriptField(base.serializedObject);

			EditorLayoutUtility.HeaderField("Movement");

			EditorGUILayout.PropertyField(this.speedProperty);
			EditorGUILayout.PropertyField(this.snappynessProperty);

			EditorGUILayout.Space();
			EditorLayoutUtility.HeaderField("Rotation");

			EditorGUILayout.PropertyField(this.sensitivityProperty);

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}

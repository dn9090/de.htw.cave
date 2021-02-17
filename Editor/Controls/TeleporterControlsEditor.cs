using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Utilites;

namespace Htw.Cave.Controls
{
	[CustomEditor(typeof(TeleporterControls))]
	public class TeleporterControlsEditor : Editor
	{
		private TeleporterControls me;
		private SerializedProperty teleporterProperty;

		public void OnEnable()
		{
			this.me = (TeleporterControls)base.target;
			this.teleporterProperty = base.serializedObject.FindProperty("teleporter");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorLayoutUtility.ScriptField(base.serializedObject);

			EditorGUILayout.PropertyField(this.teleporterProperty);

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}

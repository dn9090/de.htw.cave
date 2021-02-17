using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Utilites;

namespace Htw.Cave.Joycons
{
	[CustomEditor(typeof(JoyconController))]
	public class JoyconControllerEditor : Editor
	{
		private JoyconController me;
		private SerializedProperty bindingProperty;

		public void OnEnable()
		{
			this.me = (JoyconController)base.target;
			this.bindingProperty = base.serializedObject.FindProperty("binding");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorLayoutUtility.ScriptField(base.serializedObject);

			EditorGUILayout.PropertyField(this.bindingProperty);

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}

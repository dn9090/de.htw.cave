using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Utilites;

namespace Htw.Cave.SpeechToText
{
	[CustomEditor(typeof(SpeechRecognizer))]
	public class SpeechRecognizerEditor : Editor
	{
		private SpeechRecognizer me;
		private SerializedProperty initialTimeoutProperty;
		private SerializedProperty automaticTimeoutProperty;

		public void OnEnable()
		{
			this.me = (SpeechRecognizer)base.target;
			this.initialTimeoutProperty = base.serializedObject.FindProperty("initialTimeout");
			this.automaticTimeoutProperty = base.serializedObject.FindProperty("automaticTimeout");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorLayoutUtility.ScriptField(base.serializedObject);

			EditorLayoutUtility.HeaderField("Timeouts");

			EditorGUILayout.PropertyField(this.initialTimeoutProperty, new GUIContent("Initial"));
			EditorGUILayout.PropertyField(this.automaticTimeoutProperty, new GUIContent("Automatic"));

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}

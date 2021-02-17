using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Htw.Cave.Utilites
{
	public static class EditorLayoutUtility
	{
		public static void ScriptField(SerializedObject serializedObject)
		{
			using(new EditorGUI.DisabledScope(true))
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
		}

		public static void HeaderField(string text)
		{
			EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
		}

		public static void FlagsField<T>(SerializedProperty serializedProperty) where T : Enum
		{
			// HACK: I am the best, believe me. This is a ugly hack to pass a generic Enum
			// to a non generic Unity flags field. But it works (with C# 7.3+).
			Enum e = (T)(object)serializedProperty.intValue;
			Enum f = EditorGUILayout.EnumFlagsField(serializedProperty.displayName, e);
			serializedProperty.intValue = (int)Convert.ChangeType(f, typeof(int));
		}

		public static void ScriptableField(SerializedProperty serializedProperty, ref Editor editor)
		{
			EditorGUILayout.PropertyField(serializedProperty);

			EditorGUI.indentLevel++;

			if(serializedProperty.objectReferenceValue == null)
				editor = null;

			if(editor == null && serializedProperty.objectReferenceValue != null)
				Editor.CreateCachedEditor(serializedProperty.objectReferenceValue, null, ref editor);

			if(editor != null)
			{
				editor.DrawDefaultInspector();
				editor.serializedObject.ApplyModifiedProperties();
			}

			EditorGUI.indentLevel--;
		}
	}
}

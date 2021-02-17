using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Htw.Cave.VirtualEnvironmentSetup
{
	public static class WindowDesignUtility
	{
		public static float SizeY { get; set; }
		public static float SizeX { get; set; }

		public static void ProgressBar(float progress)
		{
			EditorGUI.ProgressBar(new Rect(0, 0, SizeX, 5), progress, "");
		}

		public static void DrawTexture(Texture2D texture)
		{
			GUIStyle center = new GUIStyle(EditorStyles.label);
			center.alignment = TextAnchor.MiddleCenter;

			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField(new GUIContent(texture), center, GUILayout.MaxHeight(128f));
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
		}

		public static void DrawTitle(string title, Color color, Texture2D texture)
		{
			GUIStyle center = new GUIStyle(EditorStyles.boldLabel);
			center.alignment = TextAnchor.MiddleCenter;
			center.normal.textColor = color;

			GUI.DrawTexture(new Rect(0, 5, SizeX, 26), texture, ScaleMode.StretchToFill);
			EditorGUILayout.LabelField(title, center);
			EditorGUILayout.Space();
			//EditorGUI.LabelField(new Rect(0, 5, SizeX, 24), title, center);
		}

		public static void SectionLabel(string text)
		{
			EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
		}

		public static bool SmallButton(string text)
		{
			return GUILayout.Button(text, GUILayout.MinWidth(50f));
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Htw.Cave.Utilites;

namespace Htw.Cave.Controls
{
	[CustomEditor(typeof(Teleporter))]
	public class TeleporterEditor : Editor
	{
		private Teleporter me;
		private SerializedProperty collisionLayerProperty;
		private SerializedProperty detectionDistanceProperty;
		private SerializedProperty rayCurveProperty;
		private SerializedProperty curveMultiplierProperty;
		private SerializedProperty curveResolutionProperty;

		public void OnEnable()
		{
			this.me = (Teleporter)base.target;
			this.collisionLayerProperty = base.serializedObject.FindProperty("collisionLayer");
			this.detectionDistanceProperty = base.serializedObject.FindProperty("detectionDistance");
			this.rayCurveProperty = base.serializedObject.FindProperty("rayCurve");
			this.curveMultiplierProperty = base.serializedObject.FindProperty("curveMultiplier");
			this.curveResolutionProperty = base.serializedObject.FindProperty("curveResolution");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			serializedObject.Update();

			EditorLayoutUtility.ScriptField(base.serializedObject);

			EditorLayoutUtility.HeaderField("Physics");

			EditorGUILayout.PropertyField(this.collisionLayerProperty);
			EditorGUILayout.PropertyField(this.detectionDistanceProperty);

			EditorGUILayout.Space();
			EditorLayoutUtility.HeaderField("Visualization");

			EditorGUILayout.PropertyField(this.rayCurveProperty);
			EditorGUILayout.PropertyField(this.curveMultiplierProperty);
			EditorGUILayout.PropertyField(this.curveResolutionProperty);

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}

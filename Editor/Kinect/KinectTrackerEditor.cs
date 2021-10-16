using System;
using UnityEngine;
using UnityEditor;

namespace Htw.Cave.Kinect
{
	[CustomEditor(typeof(KinectTracker))]
	public class KinectTrackerEditor : Editor
	{
		private KinectTracker m_Me;

		private SerializedProperty m_ConstructionTypeProperty;

		private SerializedProperty m_PrefabProperty;
	
		public void OnEnable()
		{
			this.m_Me = (KinectTracker)target;
			this.m_ConstructionTypeProperty = serializedObject.FindProperty("constructionType");
			this.m_PrefabProperty = serializedObject.FindProperty("prefab");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(this.m_ConstructionTypeProperty);

			if(this.m_Me.constructionType == KinectActorConstructionType.Prefab)
			{
				EditorGUILayout.PropertyField(this.m_PrefabProperty);

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				if(this.m_Me.prefab == null && GUILayout.Button("Create Actor"))
				{
					var gameObject = new GameObject("Kinect Actor");
					var actor = gameObject.AddComponent<KinectActor>();
					this.m_PrefabProperty.objectReferenceValue = actor;
					Selection.activeGameObject = gameObject;
				}

				EditorGUILayout.EndHorizontal();
			}	

			serializedObject.ApplyModifiedProperties();
		}
	}
}